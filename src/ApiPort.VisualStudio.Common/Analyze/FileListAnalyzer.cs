// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Reporting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiPortVS.Analyze
{
    public class FileListAnalyzer
    {
        private readonly IVsApiPortAnalyzer _analyzer;
        private readonly IFileWriter _reportWriter;

        public FileListAnalyzer(IVsApiPortAnalyzer analyzer, IFileWriter reportWriter)
        {
            _analyzer = analyzer;
            _reportWriter = reportWriter;
        }

        public async Task AnalyzeProjectAsync(IEnumerable<string> inputAssemblyPaths)
        {
            await _analyzer.WriteAnalysisReportsAsync(inputAssemblyPaths, _reportWriter, false).ConfigureAwait(false);
        }
    }
}
