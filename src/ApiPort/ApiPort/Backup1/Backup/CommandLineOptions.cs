// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPort.Resources;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.ObjectModel;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.CommandLine;
using System.IO;

namespace ApiPort
{
    internal static class CommandLineOptions
    {
        public const string DefaultName = "ApiPortAnalysis";

        public static ICommandLineOptions ParseCommandLineOptions(string[] args)
        {
            bool overwriteOutput = false;
            IReadOnlyList<string> file = Array.Empty<string>();
            string outFile = DefaultName;
            string description = string.Empty;
            IReadOnlyList<string> target = Array.Empty<string>();
            IReadOnlyList<string> result = Array.Empty<string>();
            bool showNonPortableApis = true;
            bool showBreakingChanges = false;
            bool showRetargettingIssues = false;
            bool noDefaultIgnoreFile = false;
            IReadOnlyList<string> ignoreAssemblyFile = Array.Empty<string>();
            IReadOnlyList<string> suppressBreakingChange = Array.Empty<string>();
            string targetMap = string.Empty;
            string endpoint = "https://portability.dot.net";
            AppCommand command = default;

            ArgumentSyntax argSyntax = default;
            try
            {
                ArgumentSyntax.Parse(args, syntax =>
                {
                    syntax.HandleErrors = false;

                    syntax.DefineCommand("analyze", ref command, AppCommand.AnalyzeAssemblies, LocalizedStrings.CmdAnalyzeMessage);
                    syntax.DefineOptionList("f|file", ref file, LocalizedStrings.CmdAnalyzeFileInput);
                    syntax.DefineOption("o|out", ref outFile, LocalizedStrings.CmdAnalyzeOutputFileName);
                    syntax.DefineOption("d|description", ref description, LocalizedStrings.CmdAnalyzeDescription);
                    syntax.DefineOption("e|endpoint", ref endpoint, LocalizedStrings.CmdEndpoint);
                    syntax.DefineOptionList("t|target", ref target, LocalizedStrings.CmdAnalyzeTarget);
                    syntax.DefineOptionList("r|resultFormat", ref result, LocalizedStrings.CmdAnalyzeResultFormat);
                    syntax.DefineOption("p|showNonPortableApis", ref showNonPortableApis, LocalizedStrings.CmdAnalyzeShowNonPortableApis);
                    syntax.DefineOption("b|showBreakingChanges", ref showBreakingChanges, LocalizedStrings.CmdAnalyzeShowBreakingChanges);
                    syntax.DefineOption("u|showRetargettingIssues", ref showRetargettingIssues, LocalizedStrings.CmdAnalyzeShowRetargettingIssues);
                    syntax.DefineOption("force", ref overwriteOutput, LocalizedStrings.OverwriteFile);
                    syntax.DefineOption("noDefaultIgnoreFile", ref noDefaultIgnoreFile, LocalizedStrings.CmdAnalyzeNoDefaultIgnoreFile);
                    syntax.DefineOptionList("i|ignoreAssemblyFile", ref ignoreAssemblyFile, LocalizedStrings.CmdAnalyzeIgnoreAssembliesFile);
                    syntax.DefineOptionList("s|suppressBreakingChange", ref suppressBreakingChange, LocalizedStrings.CmdAnalyzeSuppressBreakingChange);
                    syntax.DefineOption("targetMap", ref targetMap, LocalizedStrings.CmdAnalyzeTargetMap);

#if !FEATURE_OFFLINE
                    syntax.DefineCommand("dump", ref command, AppCommand.DumpAnalysis, LocalizedStrings.CmdDumpAnalysis);
                    syntax.DefineOptionList("f|file", ref file, LocalizedStrings.CmdAnalyzeFileInput);
                    syntax.DefineOption("o|out", ref outFile, LocalizedStrings.CmdAnalyzeOutputFileName);
#endif

                    syntax.DefineCommand("listTargets", ref command, AppCommand.ListTargets, LocalizedStrings.ListTargets);
                    syntax.DefineOption("e|endpoint", ref endpoint, LocalizedStrings.CmdEndpoint);

                    syntax.DefineCommand("listOutputFormats", ref command, AppCommand.ListOutputFormats, LocalizedStrings.ListOutputFormats);
                    syntax.DefineOption("e|endpoint", ref endpoint, LocalizedStrings.CmdEndpoint);

                    syntax.DefineCommand("docId", ref command, AppCommand.DocIdSearch, LocalizedStrings.CmdDocId);
                    syntax.DefineOption("e|endpoint", ref endpoint, LocalizedStrings.CmdEndpoint);

                    argSyntax = syntax;
                });
            }
            catch (ArgumentSyntaxException e)
            {
                Console.WriteLine();

                Console.WriteLine(e.Message);

                if (argSyntax != null)
                {
                    Console.WriteLine(argSyntax.GetHelpText());
                }

                return new ConsoleApiPortOptions(AppCommand.Exit);
            }

            // Set OverwriteOutputFile to true if the output file name is explicitly specified
            if (!string.Equals(DefaultName, outFile, StringComparison.Ordinal))
            {
                overwriteOutput = true;
            }

            var (inputFiles, invalidFiles) = ProcessInputAssemblies(file);

            return new ConsoleApiPortOptions(command)
            {
                BreakingChangeSuppressions = suppressBreakingChange,
                Description = description,
                IgnoredAssemblyFiles = ignoreAssemblyFile,
                InputAssemblies = inputFiles,
                InvalidInputFiles = invalidFiles,
                OutputFileName = outFile,
                OutputFormats = result,
                OverwriteOutputFile = overwriteOutput,
                RequestFlags = GetRequestFlags(showBreakingChanges, showRetargettingIssues, showNonPortableApis),
                ServiceEndpoint = endpoint,
                TargetMapFile = targetMap,
                Targets = target,
            };
        }

        private static AnalyzeRequestFlags GetRequestFlags(bool showBreakingChanges, bool showRetargettingIssues, bool showNonPortableApis)
        {
            var requestFlags = default(AnalyzeRequestFlags);

            if (showBreakingChanges)
            {
                requestFlags |= AnalyzeRequestFlags.ShowBreakingChanges;
            }

            if (showRetargettingIssues)
            {
                requestFlags |= AnalyzeRequestFlags.ShowRetargettingIssues;
                requestFlags |= AnalyzeRequestFlags.ShowBreakingChanges;
            }

            if (showNonPortableApis)
            {
                requestFlags |= AnalyzeRequestFlags.ShowNonPortableApis;
            }

            // If nothing is set, default to ShowNonPortableApis
            if ((requestFlags & (AnalyzeRequestFlags.ShowBreakingChanges | AnalyzeRequestFlags.ShowNonPortableApis)) == AnalyzeRequestFlags.None)
            {
                requestFlags |= AnalyzeRequestFlags.ShowNonPortableApis;
            }

            return requestFlags;
        }

        private static (ImmutableDictionary<IAssemblyFile, bool>, IReadOnlyCollection<string>) ProcessInputAssemblies(IEnumerable<string> files)
        {
            var s_ValidExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".dll",
                ".exe",
                ".winmd",
                ".ilexe",
                ".ildll"
            };

            var inputAssemblies = new SortedDictionary<IAssemblyFile, bool>(AssemblyFileComparer.Instance);
            var invalidInputFiles = new List<string>();

            void ProcessInputAssemblies(string path, bool isExplicitlySpecified)
            {
                bool HasValidPEExtension(string assemblyLocation)
                {
                    return s_ValidExtensions.Contains(Path.GetExtension(assemblyLocation));
                }

                if (Directory.Exists(path))
                {
                    foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
                    {
                        // If the user passes in a whole directory, any assembly we find in there
                        // was not explicitly passed in.
                        ProcessInputAssemblies(file, isExplicitlySpecified: false);
                    }
                }
                else if (File.Exists(path))
                {
                    // Only add files with valid PE extensions to the list of
                    // assemblies to analyze since others are not valid assemblies
                    if (HasValidPEExtension(path))
                    {
                        var filePath = new FilePathAssemblyFile(path);
                        if (inputAssemblies.TryGetValue(filePath, out var isAssemblySpecified))
                        {
                            // If the assembly already exists, and it was not
                            // specified explicitly, in the the case where one
                            // value does not match the other, we default to
                            // saying that the assembly is specified.
                            inputAssemblies[filePath] = isExplicitlySpecified || isAssemblySpecified;
                        }
                        else
                        {
                            inputAssemblies.Add(filePath, isExplicitlySpecified);
                        }
                    }
                }
                else
                {
                    invalidInputFiles.Add(path);
                }
            }

            foreach (var file in files)
            {
                ProcessInputAssemblies(file, isExplicitlySpecified: true);
            }

            return (inputAssemblies.ToImmutableDictionary(), invalidInputFiles);
        }

        private class ConsoleApiPortOptions : ReadWriteApiPortOptions, ICommandLineOptions
        {
            public ConsoleApiPortOptions(AppCommand command)
            {
                Command = command;
            }

            public AppCommand Command { get; }

            public string TargetMapFile { get; set; }
        }
    }
}
