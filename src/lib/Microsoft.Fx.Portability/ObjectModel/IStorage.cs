// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Fx.Portability.ObjectModel
{
    public interface IStorage
    {
        Task SaveRequestToBlobAsync(AnalyzeRequest analyzeRequest, string submissionId);
        Task<AnalyzeRequest> RetrieveRequestAsync(string uniqueId);
        Task DeleteRequestFromBlobAsync(string uniqueId);

        Task SaveResultToBlobAsync(string submissionId, AnalyzeResponse result);
        Task<AnalyzeResponse> RetrieveResultFromBlobAsync(string submissionId);
        Task DeleteResultFromBlobAsync(string submissionid);

        Task<IEnumerable<string>> RetrieveSubmissionIdsAsync();
        Task AddJobToQueueAsync(string submissionId);
        IEnumerable<ProjectSubmission> GetProjectSubmissions();
    }
}
