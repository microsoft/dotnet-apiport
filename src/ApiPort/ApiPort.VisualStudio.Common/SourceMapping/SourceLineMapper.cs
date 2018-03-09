// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Contracts;
using ApiPortVS.Resources;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.Reporting;
using Microsoft.Fx.Portability.Reporting.ObjectModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ApiPortVS.SourceMapping
{
    public abstract class SourceLineMapper : ISourceLineMapper
    {
        private readonly IFileSystem _fileSystem;
        private readonly TextWriter _textOutput;
        private readonly IProgressReporter _progressReporter;

        public SourceLineMapper(IFileSystem fileSystem, TextWriter textOutputTarget, IProgressReporter progressReporter)
        {
            _fileSystem = fileSystem;
            _textOutput = textOutputTarget;
            _progressReporter = progressReporter;
        }

        public IEnumerable<ISourceMappedItem> GetSourceInfo(IEnumerable<string> assemblyPaths, ReportingResult report)
        {
            var items = new List<ISourceMappedItem>();

            _textOutput.WriteLine();
            _textOutput.WriteLine(LocalizedStrings.FindingSourceLineInformationFor);

            int currentIssues = _progressReporter.Issues.Count;

            foreach (var assembly in assemblyPaths)
            {
                using (var task = _progressReporter.StartTask(string.Format(CultureInfo.InvariantCulture, "\t{0}\b\b\b", Path.GetFileName(assembly))))
                {
                    try
                    {
                        var pdbPath = _fileSystem.ChangeFileExtension(assembly, "pdb");

                        if (!_fileSystem.FileExists(pdbPath))
                        {
                            _progressReporter.ReportIssue(string.Format(CultureInfo.CurrentCulture, LocalizedStrings.PdbNotFoundFormat, assembly));
                            task.Abort();
                        }
                        else
                        {
                            try
                            {
                                var sourceItems = GetSourceInfo(assembly, pdbPath, report);
                                items.AddRange(sourceItems);
                            }
                            catch (OutOfMemoryException ex)
                            {
                                // TODO: Update Microsoft.CCI to support parsing portable pdbs.
                                // Due to an OutOfMemoryException thrown when trying to parse portable pdb files.
                                // https://github.com/icsharpcode/ILSpy/issues/789
                                // There is no public build for https://github.com/Microsoft/cci yet, which supports it.
                                Trace.TraceError("OOM while trying to parse pdb file." + Environment.NewLine + ex.ToString());

                                _progressReporter.ReportIssue(string.Format(CultureInfo.CurrentCulture, LocalizedStrings.SourceLineMappingNotSupportedPortablePdb, assembly));

                                task.Abort();
                            }
                        }
                    }
                    catch (PortabilityAnalyzerException)
                    {
                        task.Abort();
                    }
                }

                var issues = _progressReporter.Issues.ToArray();

                // There were more issues reported while running this current task.
                if (currentIssues < issues.Length)
                {
                    for (int i = currentIssues; i < issues.Length; i++)
                    {
                        _textOutput.WriteLine(issues[i]);
                    }

                    currentIssues = issues.Length;
                }
            }

            return items;
        }

        public abstract IEnumerable<ISourceMappedItem> GetSourceInfo(string assemblyPath, string pdbPath, ReportingResult report);
    }
}
