// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Fx.Portability.Azure.Storage
{
    /// <summary>
    /// Provides useful extensions to Azure Storage classes
    /// </summary>
    public static class AzureStorageExtensions
    {
        public static async Task<IEnumerable<CloudBlobContainer>> ListContainersAsync(this CloudBlobClient cloudBlobClient, string prefix = null)
        {
            BlobContinuationToken continuationToken = null;
            var results = new List<CloudBlobContainer>();
            do
            {
                var segment = await cloudBlobClient.ListContainersSegmentedAsync(continuationToken).ConfigureAwait(false);
                continuationToken = segment.ContinuationToken;
                if (string.IsNullOrEmpty(prefix))
                {
                    results.AddRange(segment.Results);
                }
                else
                {
                    foreach (var result in segment.Results)
                    {
                        if (result.Name.StartsWith(prefix, StringComparison.Ordinal))
                        {
                            results.Add(result);
                        }
                    }
                }
            } while (continuationToken != null);

            return results;
        }

        public static async Task<IEnumerable<IListBlobItem>> ListBlobsAsync(this CloudBlobContainer cloudBlobContainer)
        {
            BlobContinuationToken continuationToken = null;
            var results = new List<IListBlobItem>();
            do
            {
                var segment = await cloudBlobContainer.ListBlobsSegmentedAsync(continuationToken).ConfigureAwait(false);
                continuationToken = segment.ContinuationToken;
                results.AddRange(segment.Results);
            } while (continuationToken != null);

            return results;
        }
    }

}
