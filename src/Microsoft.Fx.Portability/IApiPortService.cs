// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reporting.ObjectModel;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Fx.Portability
{
    public interface IApiPortService
    {
        Task<ServiceResponse<IEnumerable<AvailableTarget>>> GetTargetsAsync();
        Task<ServiceResponse<AnalyzeResponse>> SendAnalysisAsync(AnalyzeRequest a);
        Task<ServiceResponse<byte[]>> SendAnalysisAsync(AnalyzeRequest a, string format);
        Task<ServiceResponse<UsageDataCollection>> GetUsageDataAsync(int? skip = null, int? top = null, UsageDataFilter? filter = null, IEnumerable<string> targets = null);
        Task<ServiceResponse<AnalyzeResponse>> GetAnalysisAsync(string submissionId);
        Task<ServiceResponse<byte[]>> GetAnalysisAsync(string submissionId, string format);
        Task<ServiceResponse<ApiInformation>> GetApiInformationAsync(string docId);
        Task<ServiceResponse<IReadOnlyCollection<ApiDefinition>>> SearchFxApiAsync(string query, int? top = null);
        Task<ServiceResponse<IEnumerable<ResultFormatInformation>>> GetResultFormatsAsync();
    }
}
