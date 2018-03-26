// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Fx.Portability.Azure.Storage
{
    public class StorageBlob
    {
        private static readonly Version StorageVersion = new Version(1, 0, 0, 0);

        private readonly CloudStorageAccount _storageAccount;

        public StorageBlob(CloudStorageAccount storageAccount)
        {
            _storageAccount = storageAccount;
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
            DateTime currentDate = DateTime.Now;
            string containerName = GetCurrentMonthContainerName(currentDate);
            CloudBlobContainer container = await GetBlobContainerAsync(containerName);

            if (container == null)
            {
                return false;
            }

            byte[] content = request.SerializeAndCompress();
            var containerPath = $"{currentDate.Day.ToString("00", CultureInfo.InvariantCulture)}/{uniqueId}";

            CloudBlockBlob blob = container.GetBlockBlobReference(containerPath);
            blob.Metadata.Add("Version", request.Version.ToString(CultureInfo.InvariantCulture));
            await blob.UploadFromByteArrayAsync(content, 0, content.Length);
            await blob.SetMetadataAsync();
            return true;
        }

        public async Task<AnalyzeRequest> RetrieveFromBlobAsync(string uniqueId, string containerName)
        {
            CloudBlobContainer container = await GetBlobContainerAsync(containerName);
            CloudBlockBlob blob = container.GetBlockBlobReference(uniqueId);

            using (var blobStream = new MemoryStream())
            {
                await blob.DownloadToStreamAsync(blobStream);

                var blobData = blobStream.ToArray();

                return blobData.DecompressToObject<AnalyzeRequest>();
            }
        }

        public async Task<IEnumerable<string>> RetrieveRequestIdsAsync()
        {
            var blobs = await GetBlobsAsync();
            List<string> results = new List<string>();
            foreach (var blob in blobs)
            {
                results.Add(blob.Name);
            }

            return results;
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
            var query = blobs.SelectMany(b => b)
                .OfType<CloudBlockBlob>();
            return query;
        }

        public static string GetCurrentMonthContainerName()
        {
            return GetCurrentMonthContainerName(DateTime.Now);
        }

        public static string GetCurrentMonthContainerName(DateTime currentDate)
        {
            return GetContainerName(currentDate.Month, currentDate.Year);
        }

        private static string GetContainerName(int month, int year)
        {
            string containerName = string.Format(CultureInfo.InvariantCulture, "container{0}{1}", year, month.ToString("00", CultureInfo.InvariantCulture));
            return containerName;
        }

        public async Task<CloudBlobContainer> GetCurrentContainerAsync()
        {
            DateTime currentDate = DateTime.Now;
            return await GetBlobContainerAsync(GetCurrentMonthContainerName(currentDate));
        }

        private async Task<CloudBlobContainer> GetBlobContainerAsync(string containerName)
        {
            CloudBlobClient blobClient = _storageAccount.CreateCloudBlobClient();
            CloudBlobContainer catalogContainer = blobClient.GetContainerReference(containerName);
            if (await catalogContainer.CreateIfNotExistsAsync())
            {
                await catalogContainer.SetPermissionsAsync(new BlobContainerPermissions() { PublicAccess = BlobContainerPublicAccessType.Off });
            }

            return catalogContainer;
        }
    }
}
