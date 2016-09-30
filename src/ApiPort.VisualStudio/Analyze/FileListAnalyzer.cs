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
        private readonly IFileWriter _reportWriter;

        public FileListAnalyzer(
            ApiPortClient client,
            OptionsViewModel optionsViewModel,
            TextWriter outputWindow,
            IReportViewer viewer,
            IProgressReporter reporter,
            IFileWriter reportWriter)
            : base(client, optionsViewModel, outputWindow, viewer, reporter)
        {
            _reportWriter = reportWriter;
        }

        public async Task AnalyzeProjectAsync(IEnumerable<string> inputAssemblyPaths)
        {
            await WriteAnalysisReportsAsync(inputAssemblyPaths, _reportWriter, false);
        }
    }
}
