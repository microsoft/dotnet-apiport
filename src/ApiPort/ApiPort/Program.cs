// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPort.Resources;
using Autofac;
using Microsoft.Fx.Portability;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ApiPort
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var productInformation = new ProductInformation("ApiPort_Console");

            Console.WriteLine(LocalizedStrings.Header, LocalizedStrings.ApplicationName, productInformation.InformationalVersion, DocumentationLinks.About, DocumentationLinks.PrivacyPolicy);

            var options = CommandLineOptions.ParseCommandLineOptions(args);

            if (options.Command == AppCommand.Exit)
            {
                return -1;
            }

            Console.WriteLine();

            using (var container = DependencyBuilder.Build(options, productInformation))
            {
                var progressReport = container.Resolve<IProgressReporter>();

                try
                {
                    var client = container.Resolve<ConsoleApiPort>();

                    switch (options.Command)
                    {
                        case AppCommand.ListTargets:
                            await client.ListTargetsAsync();
                            break;
                        case AppCommand.AnalyzeAssemblies:
                        case AppCommand.DumpAnalysis:
                            await client.AnalyzeAssembliesAsync();
                            break;
                        case AppCommand.DocIdSearch:
                            await client.RunDocIdSearchAsync();
                            break;
                        case AppCommand.ListOutputFormats:
                            await client.ListOutputFormatsAsync();
                            break;
                    }

                    return 0;
                }
                catch (Autofac.Core.DependencyResolutionException ex) when (GetPortabilityException(ex) is PortabilityAnalyzerException p)
                {
                    Trace.TraceError(ex.ToString());

                    WriteException(p);
                }
                catch (PortabilityAnalyzerException ex)
                {
                    WriteException(ex);
                }
                catch (ProxyAuthenticationRequiredException ex)
                {
                    WriteException(ex);
                }
                catch (AggregateException ex)
                {
                    Trace.TraceError(ex.ToString());

                    // If the exception is known, display the message as it has already been localized
                    if (GetRecursiveInnerExceptions(ex).Any(x => x is PortabilityAnalyzerException))
                    {
                        foreach (PortabilityAnalyzerException portEx in GetRecursiveInnerExceptions(ex).Where(x => x is PortabilityAnalyzerException))
                        {
                            WriteException(portEx);
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

        private static void WriteException(Exception ex)
        {
            Trace.TraceError(ex.ToString());

            // Display the message as it has already been localized
            WriteError(ex.Message);
#if DEBUG
            // Provide additional info on inner exceptions if built for debug
            if (ex.InnerException != null)
            {
                WriteError(ex.InnerException.ToString());
            }
#endif // DEBUG
        }

        private static PortabilityAnalyzerException GetPortabilityException(Exception e)
        {
            while (e != null)
            {
                if (e is PortabilityAnalyzerException p)
                {
                    return p;
                }

                e = e.InnerException;
            }

            return null;
        }

        private static IEnumerable<Exception> GetRecursiveInnerExceptions(Exception ex)
        {
            // AggregateExceptions can have multiple inner exceptions
            if (ex is AggregateException)
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

            // Other exceptions can have only one inner exception
            else
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
            var previousColor =
#if LINUX
                // Console.get_ForegroundColor is unsopported by the Linux PAL
                ConsoleColor.White;
#else // LINUX
                Console.ForegroundColor;
#endif // LINUX

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

                if (string.Equals(errorType.FullName, "Mono.Security.Protocol.Tls.TlsException", StringComparison.Ordinal))
                {
                    Console.WriteLine(LocalizedStrings.MonoWebRequestsFailure);

                    return true;
                }
            }

            return false;
        }
    }
}
