// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;

namespace Microsoft.Fx.Portability
{
    public class ApiInformation
    {
        public ApiInformation() { }

        public ApiInformation(string docId, IApiCatalogLookup catalog, IApiRecommendations recommendations)
        {
            if (string.IsNullOrWhiteSpace(docId))
            {
                throw new ArgumentNullException("docId");
            }

            Definition = catalog.GetApiDefinition(docId);

            Supported = catalog.GetSupportedVersions(docId);

            AdditionalInformation = recommendations.GetNotes(docId);
            SourceCompatibleChanges = recommendations.GetSourceCompatibleChanges(docId);
            RecommendedChanges = recommendations.GetRecommendedChanges(docId);
            Component = recommendations.GetComponent(docId);
        }

        public ApiDefinition Definition { get; set; }

        public IEnumerable<FrameworkName> Supported { get; set; }

        public IEnumerable<ApiNote> AdditionalInformation { get; set; }

        public string RecommendedChanges { get; set; }

        public string SourceCompatibleChanges { get; set; }

        public string Component { get; set; }

        public override int GetHashCode()
        {
            return new { a = Definition, b = RecommendedChanges, c = SourceCompatibleChanges, d = Component }.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as ApiInformation;

            if (other == null)
            {
                return false;
            }

            return Equals(Definition, other.Definition)
                && string.Equals(RecommendedChanges, other.RecommendedChanges, StringComparison.Ordinal)
                && string.Equals(SourceCompatibleChanges, other.SourceCompatibleChanges, StringComparison.Ordinal)
                && string.Equals(Component, other.Component, StringComparison.Ordinal);
        }
    }
}
