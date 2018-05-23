// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Fx.Portability.Cache
{
    /// <summary>
    /// This class represents a cache for items stored as blobs in Azure BLOB storage. 
    /// 
    /// The way this works is:
    ///   -- When we are asked for the value of the cache we are going to return it.
    ///   -- Periodically we are going to check and see if the value of the blob is storage has changed (by using timestamps)
    /// </summary>
    public class AzureUrlBlobCache<TObject> : UpdatingObjectCache<TObject>
    {
        private readonly string _url;
        private readonly bool _compressed;

        public AzureUrlBlobCache(string url, bool compressed, CancellationToken token, TimeSpan cachePollInterval)
            : base(token, cachePollInterval, url)
        {
            _url = url;
            _compressed = compressed;
        }

        protected override async Task<DateTimeOffset> GetTimeStampAsync(CancellationToken token)
        {
            var info = await GetObjectFromStorage<EmptyForProperties>(HttpMethod.Head, token);

            return info.LastModified;
        }

        protected override async Task<TObject> UpdateObjectAsync(CancellationToken token)
        {
            var blob = await GetObjectFromStorage<TObject>(HttpMethod.Get, token);

            return blob.Blob;
        }

        private async Task<BlobInfo<T>> GetObjectFromStorage<T>(HttpMethod method, CancellationToken token)
        {
            using (var client = new HttpClient())
            {
                var result = await client.SendAsync(new HttpRequestMessage(method, _url), token);

                if (!result.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException("Failed to retrieve object.");
                }

                using (var stream = await result.Content.ReadAsStreamAsync())
                {
                    var blobInfo = new BlobInfo<T>();

                    if (_compressed)
                    {
                        blobInfo.Blob = stream.DecompressToObject<T>();
                    }
                    else
                    {
                        blobInfo.Blob = stream.Deserialize<T>();
                    }

                    blobInfo.LastModified = result.Content.Headers.LastModified ?? DateTimeOffset.MinValue;

                    return blobInfo;
                }
            }
        }

        private class BlobInfo<T>
        {
            public T Blob { get; set; }
            public DateTimeOffset LastModified { get; set; }
        }

        private class EmptyForProperties { }
    }
}
