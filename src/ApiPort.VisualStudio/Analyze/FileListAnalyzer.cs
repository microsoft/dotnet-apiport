// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Contracts;
using ApiPortVS.ViewModels;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.Reporting;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ApiPortVS.Analyze
{
    public class FileListAnalyzer : ApiPortVsAnalyzer
    {
        private readonly IFileSystem _fileSystem;
        private readonly IReportViewer _reportViewer;
        private readonly IFileWriter _reportWriter;

        public FileListAnalyzer(ApiPortClient client, OptionsViewModel optionsViewModel, IFileSystem fileSystem, IFileWriter reportWriter, IReportViewer reportViewer, TextWriter outputWindow, IProgressReporter reporter)
            : base(client, optionsViewModel, outputWindow, reporter)
        {
            _fileSystem = fileSystem;
            _reportWriter = reportWriter;
            _reportViewer = reportViewer;
        }

        public async Task AnalyzeProjectAsync(IEnumerable<string> inputAssemblyPaths)
        {
            var reports = await WriteAnalysisReportsAsync(inputAssemblyPaths, _reportWriter, false);

            foreach (var reportPath in reports.Paths)
            {
                _reportViewer.View(reportPath);
            }
        }
    }
}
