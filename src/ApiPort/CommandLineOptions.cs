// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPort.CommandLine;
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
        private const string DefaultName = "ApiPortAnalysis";

        public static ICommandLineOptions ParseCommandLineOptions(string[] args)
        {
            bool overwriteOutput = false;
            IReadOnlyList<string> file = Array.Empty<string>();
            string outFile = string.Empty;
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
            AppCommands command = default;

            ArgumentSyntax.Parse(args, syntax =>
            {
                syntax.DefineCommand("analyze", ref command, AppCommands.AnalyzeAssemblies, LocalizedStrings.CmdAnalyzeHelp);
                syntax.DefineOptionList("f|file", ref file, LocalizedStrings.CmdHelpAnalyzeFile);
                syntax.DefineOption("o|out", ref outFile, LocalizedStrings.CmdHelpAnalyzeOut);
                syntax.DefineOption("d|description", ref description, LocalizedStrings.CmdHelpDescription);
                syntax.DefineOption("e|endpoint", ref endpoint, LocalizedStrings.CmdEndpoint);
                syntax.DefineOptionList("t|target", ref target, LocalizedStrings.CmdHelpTarget);
                syntax.DefineOptionList("r|resultFormat", ref result, LocalizedStrings.CmdResultFormat);
                syntax.DefineOption("p|showNonPortableApis", ref showNonPortableApis, LocalizedStrings.CmdHelpShowNonPortableApis);
                syntax.DefineOption("b|showBreakingChanges", ref showBreakingChanges, LocalizedStrings.CmdHelpShowBreakingChanges);
                syntax.DefineOption("u|showRetargettingIssues", ref showRetargettingIssues, LocalizedStrings.CmdShowRetargettingIssues);
                syntax.DefineOption("noDefaultIgnoreFile", ref noDefaultIgnoreFile, LocalizedStrings.CmdHelpNoDefaultIgnoreFile);
                syntax.DefineOptionList("i|ignoreAssemblyFile", ref ignoreAssemblyFile, LocalizedStrings.CmdHelpIgnoreAssembliesFile);
                syntax.DefineOptionList("s|suppressBreakingChange", ref suppressBreakingChange, LocalizedStrings.CmdHelpSuppressBreakingChange);
                syntax.DefineOption("targetMap", ref targetMap, LocalizedStrings.CmdTargetMap);

                syntax.DefineCommand("listTargets", ref command, AppCommands.ListTargets, LocalizedStrings.ListTargets);
                syntax.DefineOption("e|endpoint", ref endpoint, LocalizedStrings.CmdEndpoint);

                syntax.DefineCommand("listOutputFormats", ref command, AppCommands.ListOutputFormats, LocalizedStrings.ListOutputFormats);
                syntax.DefineOption("e|endpoint", ref endpoint, LocalizedStrings.CmdEndpoint);

                syntax.DefineCommand("docId", ref command, AppCommands.DocIdSearch, LocalizedStrings.CmdDocId);
                syntax.DefineOption("e|endpoint", ref endpoint, LocalizedStrings.CmdEndpoint);
            });

            // Set OverwriteOutputFile to true if the output file name is explicitly specified
            if (!string.IsNullOrWhiteSpace(outFile))
            {
                overwriteOutput = true;
                outFile = DefaultName;
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
                TargetMapFile = targetMap,
                ServiceEndpoint = endpoint,
                RequestFlags = GetRequestFlags(showBreakingChanges, showRetargettingIssues, showNonPortableApis),
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
            public ConsoleApiPortOptions(AppCommands command)
            {
                Command = command;
            }

            public AppCommands Command { get; }

            public string TargetMapFile { get; set; }
        }
    }
}
