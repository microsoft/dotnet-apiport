// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPort.Resources;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.Analyzer;
using Microsoft.Fx.Portability.Reporting;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ApiPort
{
    public class Program
    {
        private static int Main(string[] args)
        {
            var productInformation = new ProductInformation("ApiPort_Console");

            Console.WriteLine(LocalizedStrings.Header, LocalizedStrings.ApplicationName, productInformation.Version);

            IProgressReporter progressReport = new ConsoleProgressReporter();
            CommandLineOptions options = CommandLineOptions.ParseCommandLineOptions();

            if (options == null)
            {
                // we could not parse the options. nothing to do.
                return -1;
            }

            try
            {
                var targetMapper = new TargetMapper();
                targetMapper.LoadFromConfig();

                using (var apiPortService = new ApiPortService(options.ServiceEndpoint, productInformation))
                {
                    var apiPortClient = new ApiPortClient(apiPortService, progressReport, targetMapper, new ReflectionMetadataDependencyFinder());
                    switch (options.Command)
                    {
                        case AppCommands.ListTargets:
                            ListTargets(apiPortClient, targetMapper).Wait();
                            break;
                        case AppCommands.AnalyzeAssemblies:
                            AnalyzeAssemblies(apiPortClient, options, progressReport).Wait();
                            break;
                    }
                }

                return 0;
            }
            catch (PortabilityAnalyzerException ex)
            {
                // Make sure that we flag the task as complete.
                progressReport.AbortTask();

                Trace.TraceError(ex.ToString());

                // Display the message as it has already been localized
                WriteError(ex.Message);
            }
            catch (AggregateException ex)
            {
                // Make sure that we flag the task as complete.
                progressReport.AbortTask();

                Trace.TraceError(ex.ToString());

                // If the exception is known, display the message as it has already been localized
                if (ex.InnerException is PortabilityAnalyzerException)
                {
                    WriteError(ex.InnerException.Message);
                }
                else if (!IsWebSecurityFailureOnMono(ex))
                {
                    WriteError(LocalizedStrings.UnknownException);
                }
            }
            catch (Exception ex)
            {
                // Make sure that we flag the task as complete.
                progressReport.AbortTask();

                Trace.TraceError(ex.ToString());

                WriteError(LocalizedStrings.UnknownException);
            }
            finally
            {
                if (progressReport != null)
                {
                    Console.WriteLine();

                    foreach (var issue in progressReport.Issues)
                    {
                        WriteWarning("* " + issue);
                    }
                }
            }

            return -1;
        }



        private static void WriteColorLine(string message, ConsoleColor color)
        {
            var previousColor = Console.ForegroundColor;

            try
            {
                Console.ForegroundColor = color;
                Console.WriteLine(message, color);
            }
            finally
            {
                Console.ForegroundColor = previousColor;
            }
        }

        private static void WriteError(string message)
        {
            Console.WriteLine();
            WriteColorLine(message, ConsoleColor.Red);
        }

        private static void WriteWarning(string message)
        {
            WriteColorLine(message, ConsoleColor.Yellow);
        }

        /// <summary>
        /// Mono does not come installed with root certificates.  If a user runs this without configuring them,
        /// they will receive a Mono.Security.Protocol.Tls.TlsException.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        private static bool IsWebSecurityFailureOnMono(Exception ex)
        {
            if (ex.InnerException is System.Net.WebException && ex.InnerException.InnerException is System.IO.IOException && ex.InnerException.InnerException.InnerException != null)
            {
                var errorType = ex.InnerException.InnerException.InnerException.GetType();

                if (String.Equals(errorType.FullName, "Mono.Security.Protocol.Tls.TlsException", StringComparison.Ordinal))
                {
                    Console.WriteLine(LocalizedStrings.MonoWebRequestsFailure);

                    return true;
                }
            }

            return false;
        }

        private static async Task ListTargets(ApiPortClient apiPortClient, ITargetMapper targetMapper)
        {
            const string SelectedMarker = "*";

            var targets = await apiPortClient.ListTargets();

            if (targets.Any())
            {
                Console.WriteLine();
                Console.WriteLine(LocalizedStrings.AvailableTargets);

                var expandableTargets = targets.Where(target => target.ExpandedTargets.Any());
                var groupedTargets = targets.Where(target => !target.ExpandedTargets.Any()).GroupBy(target => target.Name);

                foreach (var item in groupedTargets)
                {
                    Console.WriteLine(LocalizedStrings.TargetsList, item.Key, String.Join(LocalizedStrings.VersionListJoin, item.Select(v => v.Version.ToString() + (v.IsSet ? SelectedMarker : String.Empty))));
                }

                if (expandableTargets.Any())
                {
                    Console.WriteLine();
                    Console.WriteLine(Microsoft.Fx.Portability.Resources.LocalizedStrings.AvailableGroupedTargets);

                    foreach (var item in expandableTargets)
                    {
                        Console.WriteLine(LocalizedStrings.TargetsListGrouped, item.Name, String.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator + " ", item.ExpandedTargets));
                    }
                }
            }

            if (targetMapper.Aliases.Any())
            {
                Console.WriteLine();
                Console.WriteLine(LocalizedStrings.AvailableAliases);

                foreach (var alias in targetMapper.Aliases)
                {
                    Console.WriteLine(LocalizedStrings.TargetsListNoVersion, alias);
                }
            }

            Console.WriteLine();
            Console.WriteLine(LocalizedStrings.NotesOnUsage);
            Console.WriteLine(LocalizedStrings.TargetsListNoVersion, Microsoft.Fx.Portability.Resources.LocalizedStrings.HowToSpecifyVersion);
            Console.WriteLine();
            Console.WriteLine(LocalizedStrings.TargetsListNoVersion, LocalizedStrings.WhatAsteriskMeans);
        }

        private static async Task AnalyzeAssemblies(ApiPortClient apiPort, CommandLineOptions options, IProgressReporter progressReport)
        {
            var filePath = Path.GetFullPath(options.OutputFileName);
            var outputDirectory = Path.GetDirectoryName(filePath);
            var outputFileName = Path.GetFileName(filePath);

            foreach (var errorInput in options.InvalidInputFiles)
            {
                progressReport.ReportIssue(Microsoft.Fx.Portability.Resources.LocalizedStrings.InvalidFileName, errorInput);
            }

            var result = await apiPort.GetAnalysisReportAsync(options);

            string outputFilePath = string.Empty;
            if (result != null)
            {
                progressReport.StartTask(LocalizedStrings.WritingReport);

                var fileSystem = new WindowsFileSystem();
                var reportWriter = new ReportFileWriter(fileSystem, progressReport);

                outputFilePath = await reportWriter.WriteReportAsync(result, options.OutputFormat, outputDirectory, outputFileName, overwrite: true);
                progressReport.FinishTask();
            }

            Console.WriteLine();
            Console.WriteLine(LocalizedStrings.OutputWrittenTo, outputFilePath);
        }
    }
}
