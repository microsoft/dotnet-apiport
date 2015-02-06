// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPort.Resources;
using Microsoft.Fx.CommandLine;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.Reporting.ObjectModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ApiPort
{
    public enum AppCommands
    {
        ListTargets,
        AnalyzeAssemblies
    }

    public enum OutputType
    {
        Excel,
        HTML
    }

    public class CommandLineOptions : IApiPortOptions
    {
        private AppCommands _command;
        private string[] _inputAssemblies;
        private string _outputFileName = Constants.DefaultOutputFile;
        private string _targets;
        private string _description;
        private bool _noTelemetry = false;
        private string _endpoint = Constants.ServiceEndpoint;
        private ResultFormat _resultFormat;

        public IEnumerable<string> InvalidInputFiles { get; private set; }
        public AppCommands Command { get { return _command; } }
        public IEnumerable<FileInfo> InputAssemblies { get; private set; }
        public string OutputFileName { get { return _outputFileName; } }

        private List<string> LibPaths { get; set; }

        public void AddLibPaths(IEnumerable<string> paths)
        {
            if (paths == null)
                return;

            foreach (var path in paths)
                AddLibPath(path);
        }

        public virtual void AddLibPath(string path)
        {
            this.LibPaths.Add(path);
        }

        private static string[] _probingExtensions = new string[]
        {
            ".dll",
            ".ildll",
            ".ni.dll",
            ".winmd",
            ".exe",
            ".ilexe",
            //".ni.exe" Do these actually exist?
        };
        public IEnumerable<string> Targets
        {
            get
            {
                if (_targets == null)
                {
                    return Enumerable.Empty<string>();
                }

                return _targets.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            }
        }
        public string Description { get { return _description; } }
        public bool NoTelemetry { get { return _noTelemetry; } }
        public string ServiceEndpoint { get { return _endpoint; } }
        public ResultFormat OutputFormat { get { return _resultFormat; } }

        public static CommandLineOptions ParseCommandLineOptions()
        {
            CommandLineOptions options = new CommandLineOptions();

            try
            {
                var outputFormat = OutputType.Excel;

                CommandLineParser.ParseForConsoleApplication(p =>
                {
                    p.DefineAliases("targets", "t");
                    p.DefineAliases("applicationName", "name");
                    p.DefineAliases("outputFormat", "f");

                    p.DefineParameterSet("listTargets", ref options._command, AppCommands.ListTargets, LocalizedStrings.ListTargets);
#if DEBUG
                    p.DefineOptionalQualifier("endpoint", ref options._endpoint, "Service endpoint");
#endif
                    p.DefineDefaultParameterSet(ref options._command, AppCommands.AnalyzeAssemblies, LocalizedStrings.Analyze);
                    p.DefineOptionalQualifier("out", ref options._outputFileName, LocalizedStrings.OutputFileName);
                    string allFormats = string.Join(",", Enum.GetNames(typeof(ResultFormat)));
                    string defaultFormat = ResultFormat.Excel.ToString();
                    p.DefineOptionalQualifier("outputFormat", ref outputFormat, LocalizedStrings.ResultFormatHelp);
                    // This needs to be a string and not a string[] as the parser cannot handle strings with ',' in it, such as 'ProjectK,Version=1.0'
                    p.DefineOptionalQualifier("targets", ref options._targets, LocalizedStrings.TargetsToCheckAgainst);
                    p.DefineOptionalQualifier("description", ref options._description, LocalizedStrings.DescriptionHelp);

#if DEBUG
                    p.DefineOptionalQualifier("endpoint", ref options._endpoint, "Service endpoint");
#endif

                    // TODO: Decide if this should be enabled.
                    //p.DefineOptionalQualifier("noTelemetry", ref options._noTelemetry, LocalizedStrings.NoTelemetry);

                    p.DefineParameter("assemblies", ref options._inputAssemblies, LocalizedStrings.ListOfAssembliesToAnalyze);
                });

                if (options.Command == AppCommands.AnalyzeAssemblies)
                {
                    // Expand input assemblies
                    var inputAssemblies = new List<FileInfo>();
                    var invalidInputs = new List<string>();

                    foreach (var input in GetFilePaths(options._inputAssemblies, System.IO.SearchOption.AllDirectories))
                    {
                        try
                        {
                            inputAssemblies.Add(new FileInfo(input));
                        }
                        catch (ArgumentException)
                        {
                            invalidInputs.Add(input);
                        }
                    }

                    options.InputAssemblies = inputAssemblies;
                    options.InvalidInputFiles = invalidInputs;
                }

                options._resultFormat = ConvertOutputFormat(outputFormat);

                return options;
            }
            catch (CommandLineParserException parserEx)
            {
                Console.WriteLine(string.Format(LocalizedStrings.InvalidCommandLineArguments, parserEx.Message));
            }
            return null;
        }

        public static IEnumerable<string> GetFilePaths(IEnumerable<string> paths, SearchOption searchOption)
        {
            if (searchOption == SearchOption.TopDirectoryOnly)
                return GetFilePaths(paths);

            // expand the path into a list of paths that contains all the subdirectories
            Stack<string> unexpandedPaths = new Stack<string>(paths);

            HashSet<string> allPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var path in paths)
            {
                allPaths.Add(path);

                // if the path did not point to a directory, continue
                if (!Directory.Exists(path))
                    continue;

                foreach (var dir in Directory.EnumerateDirectories(path, "*.*", SearchOption.AllDirectories))
                {
                    allPaths.Add(dir);
                }
            }

            // make sure we remove any duplicated folders (ie. if the user specified both a root folder and a leaf one)
            return GetFilePaths(allPaths);
        }

        public static IEnumerable<string> GetFilePaths(IEnumerable<string> paths)
        {
            return GetFilePaths(paths, (resolvedPath) => { });
        }


        private IEnumerable<string> GetFilePathsAndAddResolvedDirectoriesToLibPaths(IEnumerable<string> paths)
        {
            return GetFilePaths(paths, (resolvedPath) => this.LibPaths.Add(resolvedPath));
        }

        private static IEnumerable<string> GetFilePaths(IEnumerable<string> paths, Action<string> perResolvedPathAction, bool recursive = false)
        {
            foreach (var path in paths)
            {
                if (path == null)
                    continue;

                string resolvedPath = Environment.ExpandEnvironmentVariables(path);

                if (Directory.Exists(resolvedPath))
                {
                    perResolvedPathAction(resolvedPath);

                    for (int extIndex = 0; extIndex < _probingExtensions.Length; extIndex++)
                    {
                        var searchPattern = "*" + _probingExtensions[extIndex];
                        foreach (var file in Directory.EnumerateFiles(resolvedPath, searchPattern))
                        {
                            yield return file;
                        }
                    }
                    if (recursive)
                    {
                        //recursively do the same for sub-folders
                        foreach (var file in GetFilePaths(Directory.EnumerateDirectories(resolvedPath), perResolvedPathAction, recursive))
                        {
                            yield return file;
                        }
                    }
                }
                else if (Path.GetFileName(resolvedPath).Contains('*'))
                {
                    IEnumerable<string> files;

                    // Cannot yield a value in the body of a try-catch with catch clause.
                    try
                    {
                        files = Directory.EnumerateFiles(Path.GetDirectoryName(resolvedPath), Path.GetFileName(resolvedPath));
                    }
                    catch (ArgumentException)
                    {
                        files = new[] { resolvedPath };
                    }

                    foreach (var file in files)
                        yield return file;
                }
                else
                {
                    yield return resolvedPath;
                }
            }
        }

        private static ResultFormat ConvertOutputFormat(OutputType outputFormat)
        {
            switch (outputFormat)
            {
                case OutputType.Excel:
                    return ResultFormat.Excel;
                case OutputType.HTML:
                    return ResultFormat.HTML;
                default:
                    throw new ArgumentOutOfRangeException("outputFormat");
            }
        }
    }
}
