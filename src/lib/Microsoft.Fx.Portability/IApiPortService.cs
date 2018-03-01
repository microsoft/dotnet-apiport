// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Fx.Portability
{
    public interface IApiPortService
    {
        Task<ServiceResponse<IEnumerable<AvailableTarget>>> GetTargetsAsync();
        Task<ServiceResponse<AnalyzeResponse>> SendAnalysisAsync(AnalyzeRequest a);
        Task<ServiceResponse<IEnumerable<ReportingResultWithFormat>>> SendAnalysisAsync(AnalyzeRequest a, IEnumerable<string> format);
        Task<ServiceResponse<UsageDataCollection>> GetUsageDataAsync(int? skip = null, int? top = null, UsageDataFilter? filter = null, IEnumerable<string> targets = null);
        Task<ServiceResponse<ApiInformation>> GetApiInformationAsync(string docId);
        Task<ServiceResponse<IReadOnlyCollection<ApiDefinition>>> SearchFxApiAsync(string query, int? top = null);
        Task<ServiceResponse<IEnumerable<ResultFormatInformation>>> GetResultFormatsAsync();
        Task<ServiceResponse<ResultFormatInformation>> GetDefaultResultFormatAsync();
        Task<ServiceResponse<IReadOnlyCollection<ApiInformation>>> QueryDocIdsAsync(IEnumerable<string> docIds);
    }
}
