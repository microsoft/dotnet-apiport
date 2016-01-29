// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Fx.Portability.ObjectModel
{
    public class AncestorApiRecommendations : IApiRecommendations
    {
        protected IApiCatalogLookup Catalog { get; }

        protected AncestorApiRecommendations(IApiCatalogLookup catalog)
        {
            Catalog = catalog;
        }

        public static IApiRecommendations Create(IApiCatalogLookup catalog)
        {
            return new AncestorApiRecommendations(catalog);
        }

        IEnumerable<ApiNote> IApiRecommendations.GetNotes(string docId)
        {
            return GetMetadata(docId, GetNotes)
                .FirstOrDefault(note => note?.Any() ?? false) ?? Enumerable.Empty<ApiNote>();
        }

        protected virtual IEnumerable<ApiNote> GetNotes(string docId)
        {
            return Enumerable.Empty<ApiNote>();
        }

        string IApiRecommendations.GetRecommendedChanges(string docId)
        {
            return GetMetadata(docId, GetRecommendedChanges)
                .FirstOrDefault(metadata => !string.IsNullOrEmpty(metadata)) ?? string.Empty;
        }

        protected virtual string GetRecommendedChanges(string docId)
        {
            return Catalog.GetRecommendedChange(docId);
        }

        string IApiRecommendations.GetSourceCompatibleChanges(string docId)
        {
            return GetMetadata(docId, GetSourceCompatibleChanges)
                .FirstOrDefault(metadata => !string.IsNullOrEmpty(metadata)) ?? string.Empty;
        }

        protected virtual string GetSourceCompatibleChanges(string docId)
        {
            return Catalog.GetSourceCompatibilityEquivalent(docId);
        }

        string IApiRecommendations.GetComponent(string docId)
        {
            return GetMetadata(docId, GetComponent)
                .FirstOrDefault(metadata => !string.IsNullOrEmpty(metadata)) ?? string.Empty;
        }

        protected virtual string GetComponent(string docId)
        {
            return String.Empty;
        }

        IEnumerable<BreakingChange> IApiRecommendations.GetBreakingChanges(string docId)
        {
            return GetMetadata(docId, GetBreakingChanges)
                .FirstOrDefault(change => change?.Any() ?? false) ?? Enumerable.Empty<BreakingChange>();
        }

        protected virtual IEnumerable<BreakingChange> GetBreakingChanges(string docId)
        {
            return Enumerable.Empty<BreakingChange>();
        }

        private IEnumerable<T> GetMetadata<T>(string docid, Func<string, T> converter)
        {
            yield return converter(docid);

            foreach (var ancestor in Catalog.GetAncestors(docid))
            {
                yield return converter(ancestor);
            }
        }
    }
}
