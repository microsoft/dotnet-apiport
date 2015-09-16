// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using NDesk.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ApiPort.CommandLine
{
    internal abstract class CommandLineOptionSet : OptionSet, ICommandLineOptions
    {
        public const ICommandLineOptions ExitCommandLineOption = null;

        private readonly string _name;
        private readonly ICollection<FileInfo> _inputAssemblies = new SortedSet<FileInfo>(new FileInfoComparer());

        // Case insensitive so that if this is run on a case-sensitive file system, we don't override anything
        private readonly ICollection<string> _invalidInputFiles = new SortedSet<string>(StringComparer.Ordinal);
        private readonly ICollection<string> _ignoredAssemblyFiles = new SortedSet<string>(StringComparer.Ordinal);

        // Targets, breaking change IDs, and output formats are not case sensitive
        private readonly ICollection<string> _targets = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly ICollection<string> _outputFormats = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly ICollection<string> _breakingChangeSuppressions = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

        private readonly static string[] s_ValidExtensions = new string[]
        {
            ".dll",
            ".exe",
            ".winmd",
            ".ilexe",
            ".ildll"
        };

        public CommandLineOptionSet(string name, string summaryMessage)
        {
            _name = name;

            SummaryMessage = summaryMessage;
            ServiceEndpoint = "http://portability.cloudapp.net";
            Description = string.Empty;
            OutputFileName = "ApiPortAnalysis";
            RequestFlags = AnalyzeRequestFlags.None;
        }

        public string SummaryMessage { get; }

        public void Add(string prototype, string description, Action<string> action, bool isRequired)
        {
            var prefix = isRequired ? "[Required]" : "[Optional]";

            Add(prototype, $"{prefix} {description}", action);
        }

        public new ICommandLineOptions Parse(IEnumerable<string> arguments)
        {
            // Add common commands
            Add("h|?|help", "Show help", h => ShowHelp = h != null);

            try
            {
                base.Parse(arguments);
            }
            catch (OptionException e)
            {
                Program.WriteColorLine(e.Message, ConsoleColor.Red);

                return ExitCommandLineOption;
            }

            if (ShowHelp || !ValidateValues())
            {
                var codebase = new Uri(this.GetType().GetTypeInfo().Assembly.CodeBase);
                var path = Path.GetFileName(codebase.AbsolutePath);

                Console.WriteLine($"{path} {_name} [options]");
                Console.WriteLine();
                Console.WriteLine(SummaryMessage);
                Console.WriteLine();

                WriteOptionDescriptions(Console.Out);

                return ExitCommandLineOption;
            }

            return this;
        }

        protected override bool Parse(string argument, OptionContext c)
        {
            string flag, name, sep, value;
            if (GetOptionParts(argument, out flag, out name, out sep, out value))
            {
                name = GetCorrectOptionCasing(name);
                return base.Parse(flag + name + sep + value, c);
            }
            return base.Parse(argument, c);
        }

        private string GetCorrectOptionCasing(string name)
        {
            foreach (string s in Dictionary.Keys)
            {
                if (s.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return s;
                }
            }
            return name;
        }

        /// <summary>
        /// Validate input values and check if okay to proceed
        /// </summary>
        protected virtual bool ValidateValues()
        {
            return true;
        }

        public bool ShowHelp { get; set; }

        public abstract AppCommands Command { get; }

        public string OutputFileName { get; set; }

        public IEnumerable<string> InvalidInputFiles { get { return _invalidInputFiles; } }

        public string Description { get; set; }

        public IEnumerable<FileInfo> InputAssemblies { get { return _inputAssemblies; } }

        public IEnumerable<string> OutputFormats { get { return _outputFormats; } }

        public AnalyzeRequestFlags RequestFlags { get; set; }

        public string ServiceEndpoint { get; set; }

        public IEnumerable<string> Targets { get { return _targets; } }

        public IEnumerable<string> IgnoredAssemblyFiles { get { return _ignoredAssemblyFiles; } }

        public IEnumerable<string> BreakingChangeSuppressions { get { return _breakingChangeSuppressions; } }

        protected void UpdateTargets(string target)
        {
            _targets.Add(target);
        }

        protected void UpdateOutputFormats(string format)
        {
            _outputFormats.Add(format);
        }

        protected void UpdateIgnoredAssemblyFiles(string file)
        {
            _ignoredAssemblyFiles.Add(file);
        }

        protected void UpdateBreakingChangeSuppressions(string breakingChangeId)
        {
            // Since users might have a lot of breaking changes to ignore, allow them to specify multiple values delimited by , or ;
            foreach (string s in breakingChangeId.Split(new char[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                _breakingChangeSuppressions.Add(s);
            }
        }

        /// <summary>
        /// This will search the input given and find all paths
        /// </summary>
        /// <param name="path">A file and directory path</param>
        protected void UpdateInputAssemblies(string path)
        {
            if (Directory.Exists(path))
            {
                foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
                {
                    UpdateInputAssemblies(file);
                }
            }
            else if (File.Exists(path))
            {
                // Only add files with valid PE extensions to the list of
                // assemblies to analyze since others are not valid assemblies
                if (HasValidPEExtension(path))
                {
                    _inputAssemblies.Add(new FileInfo(path));
                }
            }
            else
            {
                _invalidInputFiles.Add(path);
            }
        }

        private bool HasValidPEExtension(string assemblyLocation)
        {
            return s_ValidExtensions.Contains(Path.GetExtension(assemblyLocation), StringComparer.OrdinalIgnoreCase);
        }

        private class FileInfoComparer : IComparer<FileInfo>
        {
            public int Compare(FileInfo x, FileInfo y)
            {
                if (x == null)
                {
                    return y == null ? 0 : -1;
                }

                return x.FullName.CompareTo(y?.FullName);
            }
        }
    }
}
