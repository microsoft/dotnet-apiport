// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.ObjectModel;
using System.Collections.Generic;
using System.Linq;

namespace PortabilityService.AnalysisService
{
    public class GitCatalogRecommendations : AncestorApiRecommendations
    {
        private readonly ApiPortData _gitApiPortData;

        public GitCatalogRecommendations(IApiCatalogLookup catalog, ApiPortData gitData)
            : base(catalog)
        {
            _gitApiPortData = gitData;
        }

        protected override IEnumerable<BreakingChange> GetBreakingChanges(string docId)
        {
            return GetValueFromDictionary(_gitApiPortData.BreakingChangesDictionary, docId);
        }

        protected override string GetComponent(string docId)
        {
            return GetValueFromDictionary(_gitApiPortData.Components, docId);
        }

        /// <summary>
        /// Gets the recommended change for a docId.
        /// If a recommended change does not exist for a particular docId,
        /// but exists for its parent, then it'll return that recommended change.
        /// Otherwise, will return null;
        /// </summary>
        protected override string GetRecommendedChanges(string docId)
        {
            return GetValueFromDictionary(_gitApiPortData.RecommendedChanges, docId);
        }

        private IEnumerable<T> GetValueFromDictionary<T>(IDictionary<string, ICollection<T>> dictionary, string key)
        {
            ICollection<T> value;

            if (dictionary.TryGetValue(key, out value))
            {
                return value;
            }

            return Enumerable.Empty<T>();
        }

        private string GetValueFromDictionary(IDictionary<string, string> dictionary, string key)
        {
            string value;

            if (dictionary.TryGetValue(key, out value))
            {
                return value;
            }

            return string.Empty;
        }
    }
}
