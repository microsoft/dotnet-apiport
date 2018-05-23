// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.Azure;
using Microsoft.Fx.Portability.Cache;
using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Threading;
using System.Threading.Tasks;

namespace PortabilityService.AnalysisService
{
    public class CatalogInBlobIndexCache : ObjectInBlobCache<CatalogIndex>
    {
        private readonly IServiceSettings _settings;

        public CatalogInBlobIndexCache(IServiceSettings settings, CancellationToken cancellationToken)
            : base(settings.StorageAccount, CatalogConstants.CatalogContainerName, CatalogConstants.CatalogBlobName, settings.UpdateFrequency, cancellationToken)
        {
            _settings = settings;
        }

        protected override async Task<CatalogIndex> GetObjectFromStorageAsync(ICloudBlob blob, CancellationToken cancellationToken)
        {
            using (var stream = await blob.OpenReadAsync(accessCondition: null, options: new BlobRequestOptions(), operationContext: new OperationContext()))
            {
                var dotNetCatalog = stream.DecompressToObject<DotNetCatalog>();

                var catalog = new UnioningApiCatalogLookup(dotNetCatalog, _settings);

                //TODO (yumeng): figure out the Lucene story
                //return new CatalogIndex(catalog, new ApiCatalogLuceneSearcher(catalog));
                return new CatalogIndex(catalog, null);
            }
        }

        protected override CatalogIndex GetDefaultObject()
        {
            return new CatalogIndex(null, null);
        }
    }
}
