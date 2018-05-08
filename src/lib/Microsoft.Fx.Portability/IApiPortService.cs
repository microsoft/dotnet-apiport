// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Fx.Portability
{
    public interface IApiPortService
    {
        Task<ResultFormatInformation> GetDefaultResultFormatAsync();
        Task<ReportingResultWithFormat> GetReportingResultAsync(AnalyzeResponse analyzeResponse, ResultFormatInformation format);
        Task<IEnumerable<ResultFormatInformation>> GetResultFormatsAsync();
        Task<IEnumerable<AvailableTarget>> GetTargetsAsync();
        Task<AnalyzeResponse> RequestAnalysisAsync(AnalyzeRequest analyzeRequest);
        Task<IReadOnlyCollection<ApiDefinition>> SearchFxApiAsync(string query, int? top = null);
    }
}
