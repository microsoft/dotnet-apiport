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
        private const string RequestContainerNamePrefix = "requests";
        private const string ResultContainerNamePrefix = "results";

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
        public async Task SaveRequestToBlobAsync(string uniqueId, AnalyzeRequest request)
        {
            var currentDate = DateTime.Now;
            var containerName = GetCurrentMonthContainerName(currentDate);
            var container = await GetBlobContainerAsync(containerName).ConfigureAwait(false)
                ?? throw new InvalidOperationException("Azure CloubBlobContainer is not expected to be null");

            var containerPath = $"{currentDate.Day.ToString("00", CultureInfo.InvariantCulture)}/{uniqueId}";
            using (var content = new MemoryStream())
            {
                request.SerializeAndCompressToMemoryStream(content, leaveOpen: true);
                content.Seek(0, SeekOrigin.Begin);

                var blob = container.GetBlockBlobReference(containerPath);

                blob.Metadata.Add("Version", request.Version.ToString(CultureInfo.InvariantCulture));
                await blob.UploadFromStreamAsync(content).ConfigureAwait(false);
                await blob.SetMetadataAsync().ConfigureAwait(false);
            }
        }

        public async Task<AnalyzeRequest> RetrieveRequestFromBlobAsync(string uniqueId)
        {
            var blobs = await GetRequestBlobsAsync();
            // the CloudBlockBlob name contains folder name too: {day}/{submissionid}
            var blob = blobs.SingleOrDefault(b => b.Name.EndsWith(uniqueId, StringComparison.OrdinalIgnoreCase));

            if (blob == null)
            {
                return null;
            }

            using (var blobStream = await blob.OpenReadAsync())
            {
                return blobStream.DecompressToObject<AnalyzeRequest>();
            }
        }

        /// <summary>
        /// Deletes an analyze request from the blob storage. Does nothing if the blob
        /// does not exist.
        /// </summary>
        /// <param name="uniqueId">the submission id of the analyze request</param>
        /// <returns></returns>
        public async Task DeleteRequestFromBlobAsync(string uniqueId)
        {
            var blobs = await GetRequestBlobsAsync();
            // the CloudBlockBlob name contains folder name too: {day}/{submissionid}
            var blob = blobs.SingleOrDefault(b => b.Name.EndsWith(uniqueId, StringComparison.OrdinalIgnoreCase));

            if (blob != null)
            {
                await blob.DeleteIfExistsAsync();
            }
        }

        public async Task SaveResultToBlobAsync(string uniqueId, AnalyzeResult result)
        {
            var container = await GetBlobContainerAsync(ResultContainerNamePrefix).ConfigureAwait(false)
                ?? throw new InvalidOperationException("Azure CloubBlobContainer is not expected to be null");

            using (var content = new MemoryStream())
            {
                result.SerializeAndCompressToMemoryStream(content, leaveOpen: true);
                content.Seek(0, SeekOrigin.Begin);

                var blob = container.GetBlockBlobReference(uniqueId);

                await blob.UploadFromStreamAsync(content).ConfigureAwait(false);
            }
        }

        public async Task<AnalyzeResult> RetrieveResultFromBlobAsync(string uniqueId)
        {
            var container = await GetBlobContainerAsync(ResultContainerNamePrefix).ConfigureAwait(false);

            if (container == null)
            {
                throw new InvalidOperationException("Azure CloubBlobContainer is not expected to be null");
            }

            var blob = container.GetBlockBlobReference(uniqueId);

            if (blob == null)
            {
                return null;
            }

            using (var blobStream = await blob.OpenReadAsync())
            {
                return blobStream.DecompressToObject<AnalyzeResult>();
            }
        }

        public async Task DeleteResultFromBlobAsync(string uniqueId)
        {
            var container = await GetBlobContainerAsync(ResultContainerNamePrefix).ConfigureAwait(false);

            if (container == null)
            {
                throw new InvalidOperationException("Azure CloubBlobContainer is not expected to be null");
            }

            var blob = container.GetBlockBlobReference(uniqueId);

            await blob.DeleteIfExistsAsync();
        }

        private async Task<IEnumerable<CloudBlockBlob>> GetRequestBlobsAsync()
        {
            var blobClient = _storageAccount.CreateCloudBlobClient();
            var containers = await blobClient.ListContainersAsync(prefix: RequestContainerNamePrefix).ConfigureAwait(false);
            var blobTasks = containers.Select(c => c.ListBlobsAsync());
            var blobs = await Task.WhenAll(blobTasks).ConfigureAwait(false);

            var directories = blobs
                .SelectMany(b => b)
                .OfType<CloudBlobDirectory>();

            var blockTasks = directories.Select(d => d.ListBlobsAsync());
            var blocks = await Task.WhenAll(blockTasks).ConfigureAwait(false);
            return blocks.SelectMany(b => b)
                .OfType<CloudBlockBlob>();
        }

        public static string GetCurrentMonthContainerName(DateTime currentDate)
        {
            return GetContainerName(currentDate.Month, currentDate.Year);
        }

        private static string GetContainerName(int month, int year)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", RequestContainerNamePrefix, year, month.ToString("00", CultureInfo.InvariantCulture));
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
