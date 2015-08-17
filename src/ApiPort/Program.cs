// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPort.Resources;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.Reporting;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ApiPort
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var productInformation = new ProductInformation("ApiPort_Console");

            Console.WriteLine(LocalizedStrings.Header, LocalizedStrings.ApplicationName, productInformation.Version);

            var options = CommandLineOptions.ParseCommandLineOptions(args);

            if (options == null)
            {
                // we could not parse the options. nothing to do.
                return -1;
            }

            using (var container = DependencyBuilder.Build(options, productInformation))
            {
                var progressReport = container.Resolve<IProgressReporter>();

                try
                {
                    var apiPortClient = container.Resolve<ApiPortClient>();

                    switch (options.Command)
                    {
                        case AppCommands.ListTargets:
                            ListTargets(apiPortClient, container.Resolve<ITargetMapper>()).Wait();
                            break;
                        case AppCommands.AnalyzeAssemblies:
                            AnalyzeAssembliesAsync(apiPortClient, container.Resolve<IApiPortOptions>(), progressReport, container.Resolve<IFileWriter>()).Wait();
                            break;
#if DOCID_SEARCH
                        case AppCommands.DocIdSearch:
                            var docIdSearch = container.Resolve<DocIdSearchRepl>();
                            docIdSearch.DocIdSearch();
                            break;
#endif
                        case AppCommands.ListOutputFormats:
                            ListOutputFormats(apiPortClient).Wait();
                            break;
                    }

                    return 0;
                }
                catch (PortabilityAnalyzerException ex)
                {
                    Trace.TraceError(ex.ToString());

                    // Display the message as it has already been localized
                    WriteError(ex.Message);
                }
                catch (AggregateException ex)
                {
                    Trace.TraceError(ex.ToString());

                    // If the exception is known, display the message as it has already been localized
                    if (GetRecursiveInnerExceptions(ex).Any(x => x is PortabilityAnalyzerException))
                    {
                        foreach (PortabilityAnalyzerException portEx in GetRecursiveInnerExceptions(ex).Where(x => x is PortabilityAnalyzerException))
                        {
                            WriteError(portEx.Message);
                        }
                    }
                    else if (!IsWebSecurityFailureOnMono(ex))
                    {
                        WriteError(LocalizedStrings.UnknownException);
                    }
                }
                catch (Exception ex)
                {
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
        }

        private static IEnumerable<Exception> GetRecursiveInnerExceptions(Exception ex)
        {
            if (ex is AggregateException) // AggregateExceptions can have multiple inner exceptions
            {
                foreach (var innerEx in (ex as AggregateException).InnerExceptions)
                {
                    yield return innerEx;
                    foreach (var innerInnerEx in GetRecursiveInnerExceptions(innerEx))
                    {
                        yield return innerInnerEx;
                    }

                }
            }
            else // Other exceptions can have only one inner exception
            {
                if (ex.InnerException != null)
                {
                    yield return ex.InnerException;
                    foreach (var innerInnerEx in GetRecursiveInnerExceptions(ex.InnerException))
                    {
                        yield return innerInnerEx;
                    }
                }
            }
        }

        public static void WriteColorLine(string message, ConsoleColor color)
        {
            var previousColor = Console.ForegroundColor;

            try
            {
                Console.ForegroundColor = color;
                Console.WriteLine(message);
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

        private static async Task ListOutputFormats(ApiPortClient apiPortClient)
        {
            var outputFormats = await apiPortClient.ListResultFormatsAsync();

            if (outputFormats.Any())
            {
                Console.WriteLine();
                Console.WriteLine(LocalizedStrings.AvailableOutputFormats);

                foreach (var outputFormat in outputFormats)
                {
                    Console.WriteLine(string.Format(LocalizedStrings.TargetsListNoVersion, outputFormat));
                }
            }
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

        private static async Task AnalyzeAssembliesAsync(ApiPortClient apiPort, IApiPortOptions options, IProgressReporter progressReport, IFileWriter writer)
        {
            foreach (var errorInput in options.InvalidInputFiles)
            {
                progressReport.ReportIssue(string.Format(Microsoft.Fx.Portability.Resources.LocalizedStrings.InvalidFileName, errorInput));
            }

            var results = await apiPort.GetAnalysisReportAsync(options);
            var outputPaths = new List<string>();

            foreach (var resultAndFormat in results.Zip(options.OutputFormats, (r, f) => new { Result = r, Format = f }))
            {
                var outputPath = await CreateReport(resultAndFormat.Result, apiPort, options.OutputFileName, resultAndFormat.Format, progressReport, writer);

                outputPaths.Add(outputPath);
            }

            Console.WriteLine();
            Console.WriteLine(LocalizedStrings.OutputWrittenTo);

            foreach (var outputPath in outputPaths)
            {
                Console.WriteLine(outputPath);
            }
        }

        private static async Task<string> CreateReport(byte[] result, ApiPortClient apiPort, string suppliedOutputFileName, string outputFormat, IProgressReporter progressReport, IFileWriter writer)
        {
            var filePath = Path.GetFullPath(suppliedOutputFileName);
            var outputDirectory = Path.GetDirectoryName(filePath);
            var outputFileName = Path.GetFileName(filePath);

            using (var progressTask = progressReport.StartTask(LocalizedStrings.WritingReport))
            {
                try
                {
                    var extension = await apiPort.GetExtensionForFormat(outputFormat);

                    return await writer.WriteReportAsync(result, extension, outputDirectory, outputFileName, overwrite: false);
                }
                catch (Exception)
                {
                    progressTask.Abort();
                    throw;
                }
            }
        }
    }
}
