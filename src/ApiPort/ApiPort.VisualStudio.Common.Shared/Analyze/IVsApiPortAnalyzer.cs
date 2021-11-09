// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Reporting;
using Microsoft.Fx.Portability.Reporting.ObjectModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiPortVS.Analyze
{
    public interface IVsApiPortAnalyzer
    {
        Task<ReportingResult> WriteAnalysisReportsAsync(
            string entrypoint,
            IEnumerable<string> inputAssemblyPaths,
            IEnumerable<string> installedPackages,
            IFileWriter reportWriter,
            bool includeJson);
    }
}
