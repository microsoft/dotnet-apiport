// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Fx.Portability.Azure.Storage
{
    public class AzureStorage : IStorage
    {
        private readonly StorageBlob _blob;

        public AzureStorage(CloudStorageAccount storageAccount)
        {
            if (storageAccount == null)
            {
                throw new ArgumentNullException(nameof(storageAccount));
            }

            _blob = new StorageBlob(storageAccount);
        }

        public Task SaveRequestToBlobAsync(AnalyzeRequest analyzeRequest, string submissionId)
        {
            return _blob.SaveRequestToBlobAsync(submissionId, analyzeRequest);
        }

        public Task<AnalyzeRequest> RetrieveRequestAsync(string uniqueId)
        {
            return _blob.RetrieveRequestFromBlobAsync(uniqueId);
        }

        public Task DeleteRequestFromBlobAsync(string uniqueId)
        {
            return _blob.DeleteRequestFromBlobAsync(uniqueId);
        }

        public Task AddJobToQueueAsync(string submissionId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ProjectSubmission> GetProjectSubmissions()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> RetrieveSubmissionIdsAsync()
        {
            throw new NotImplementedException();
        }

        public Task SaveResultToBlobAsync(string submissionId, AnalyzeResult result)
        {
            return _blob.SaveResultToBlobAsync(submissionId, result);
        }

        public Task<AnalyzeResult> RetrieveResultFromBlobAsync(string submissionid)
        {
            return _blob.RetrieveResultFromBlobAsync(submissionid);
        }

        public Task DeleteResultFromBlobAsync(string submissionid)
        {
            return _blob.DeleteResultFromBlobAsync(submissionid);
        }
    }
}
