// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Contracts;
using ApiPortVS.Resources;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.Reporting;
using Microsoft.Fx.Portability.Reporting.ObjectModel;
using System.Collections.Generic;
using System.IO;

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

            foreach (var assembly in assemblyPaths)
            {
                using (var task = _progressReporter.StartTask(string.Format("\t{0}\b\b\b", Path.GetFileName(assembly))))
                {
                    try
                    {
                        var pdbPath = _fileSystem.ChangeFileExtension(assembly, "pdb");

                        if (!_fileSystem.FileExists(pdbPath))
                        {
                            _progressReporter.ReportIssue(string.Format(LocalizedStrings.PdbNotFoundFormat, assembly));
                            task.Abort();
                            continue;
                        }

                        items.AddRange(GetSourceInfo(assembly, pdbPath, report));
                    }
                    catch (PortabilityAnalyzerException)
                    {
                        task.Abort();
                    }
                }
            }

            return items;
        }

        public abstract IEnumerable<ISourceMappedItem> GetSourceInfo(string assemblyPath, string pdbPath, ReportingResult report);
    }
}
