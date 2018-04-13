// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Fx.Portability.Azure.Storage
{
    public class StorageBlob
    {
        private readonly CloudStorageAccount _storageAccount;

        public StorageBlob(CloudStorageAccount storageAccount)
        {
            _storageAccount = storageAccount ?? throw new ArgumentNullException(nameof(storageAccount));
        }

        /// <summary>
        /// Saves the given analysis request to the current month's container
        /// keyed by its uniqueId as shown below:
        /// * container{year}{month}/{day}/{uniqueId}
        /// * Example:
        ///     container201705/10/1234-ABCD represents the request 1234-ABCD
        ///     that was submitted on May 10, 2017.
        /// </summary>
        /// <param name="uniqueId">Guid for the analysis request</param>
        /// <param name="request">The contents of the request.</param>
        /// <returns></returns>
        public async Task<bool> SaveToBlobAsync(string uniqueId, AnalyzeRequest request)
        {
            var currentDate = DateTime.Now;
            var containerName = GetCurrentMonthContainerName(currentDate);
            var container = await GetBlobContainerAsync(containerName).ConfigureAwait(false);

            if (container == null)
            {
                throw new InvalidOperationException("Azure CloubBlobContainer is not expected to be null");
            }

            var content = request.SerializeAndCompress();
            var containerPath = $"{currentDate.Day.ToString("00", CultureInfo.InvariantCulture)}/{uniqueId}";
            var blob = container.GetBlockBlobReference(containerPath);

            blob.Metadata.Add("Version", request.Version.ToString(CultureInfo.InvariantCulture));
            await blob.UploadFromByteArrayAsync(content, 0, content.Length).ConfigureAwait(false);
            await blob.SetMetadataAsync().ConfigureAwait(false);

            return true;
        }

        public async Task<AnalyzeRequest> RetrieveFromBlobAsync(string uniqueId)
        {
            var blobs = await GetBlobsAsync();
            var blob = blobs.SingleOrDefault(b => b.Name == uniqueId);

            if (blob == null)
            {
                return null;
            }

            using (var blobStream = await blob.OpenReadAsync())
            {
                return blobStream.DecompressToObject<AnalyzeRequest>();
            }
        }

        private async Task<IEnumerable<CloudBlockBlob>> GetBlobsAsync()
        {
            var blobClient = _storageAccount.CreateCloudBlobClient();
            var containers = await blobClient.ListContainersAsync(prefix: "container").ConfigureAwait(false);
            var blobTasks = containers.Select(c => c.ListBlobsAsync());
            var blobs = await Task.WhenAll(blobTasks).ConfigureAwait(false);

            return blobs.SelectMany(b => b)
                .OfType<CloudBlockBlob>();
        }

        public static string GetCurrentMonthContainerName(DateTime currentDate)
        {
            return GetContainerName(currentDate.Month, currentDate.Year);
        }

        private static string GetContainerName(int month, int year)
        {
            return string.Format(CultureInfo.InvariantCulture, "container{0}{1}", year, month.ToString("00", CultureInfo.InvariantCulture));
        }

        private async Task<CloudBlobContainer> GetBlobContainerAsync(string containerName)
        {
            var blobClient = _storageAccount.CreateCloudBlobClient();
            var catalogContainer = blobClient.GetContainerReference(containerName);

            if (await catalogContainer.CreateIfNotExistsAsync().ConfigureAwait(false))
            {
                await catalogContainer
                    .SetPermissionsAsync(new BlobContainerPermissions() { PublicAccess = BlobContainerPublicAccessType.Off })
                    .ConfigureAwait(false);
            }

            return catalogContainer;
        }
    }
}
