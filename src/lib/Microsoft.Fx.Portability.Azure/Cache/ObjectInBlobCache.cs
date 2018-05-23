// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Fx.Portability.Cache
{
    /// <summary>
    /// This class represents a cache for items stored as blobs in Azure BLOB storage. The data stored will be the actual (deserialized, etc) object -- not just a byte[] for the blob.
    /// The way this works is:
    ///   -- When we are asked for the value of the cache we are going to return it.
    ///   -- Periodically we are going to check and see if the value of the blob is storage has changed (by using timestamps)
    /// </summary>
    public class ObjectInBlobCache<TObject> : UpdatingObjectCache<TObject>
    {
        private readonly string _blobName;
        private readonly CloudBlobContainer _blobContainer;

        public ObjectInBlobCache(CloudStorageAccount storageAccount, string containerName, string blobName, TimeSpan updateFrequency, CancellationToken cancellationToken)
            : base(cancellationToken, updateFrequency, String.Format("{0}/{1}", containerName, blobName))
        {
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            _blobName = blobName;
            _blobContainer = blobClient.GetContainerReference(containerName);

            if (_blobContainer == null)
            {
                throw new InvalidOperationException(string.Format("Could not find container '{0}'", containerName));
            }
        }

        protected override async Task<DateTimeOffset> GetTimeStampAsync(CancellationToken token)
        {
            var blob = _blobContainer.GetBlockBlobReference(_blobName);

            // Checking if a blob exists will populate properties
            try
            {
                if (await blob.ExistsAsync())
                {
                    return blob.Properties.LastModified ?? DateTimeOffset.MinValue;
                }
            }
            catch (StorageException)
            {
                Trace.WriteLine("ERROR: Could not connect to storage account");
            }

            Trace.WriteLine(String.Format("Could not find blob: {0}/{1}", _blobContainer.Name, _blobName));

            return DateTimeOffset.MinValue;
        }

        protected override async Task<TObject> UpdateObjectAsync(CancellationToken token)
        {
            // This is only called when the timestamp requires an update, therefore it exists
            var blob = await _blobContainer.GetBlobReferenceFromServerAsync(_blobName);

            return await GetObjectFromStorageAsync(blob, token);
        }

        protected virtual async Task<TObject> GetObjectFromStorageAsync(ICloudBlob blob, CancellationToken cancellationToken)
        {
            using (var stream = await blob.OpenReadAsync(accessCondition: null, options: new BlobRequestOptions(), operationContext: new OperationContext()))
            {
                return stream.DecompressToObject<TObject>();
            }
        }
    }
}
