// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Fx.Portability.Azure.Storage
{
    public class AzureStorage : IStorage
    {
        private readonly StorageBlob _blob;
        private CloudStorageAccount _storageAccount;

        public AzureStorage(CloudStorageAccount storageAccount)
        {

        }

        public Task AddJobToQueueAsync(string submissionId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ProjectSubmission> GetProjectSubmissions()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<UsageData>> GetUsageDataAsync()
        {
            throw new NotImplementedException();
        }

        public Task<AnalyzeRequest> RetrieveRequestAsync(string uniqueId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> RetrieveSubmissionIdsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> SaveToBlobAsync(AnalyzeRequest analyzeRequest, string submissionId)
        {
            throw new NotImplementedException();
        }
    }
}
