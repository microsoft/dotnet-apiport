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
            if (String.IsNullOrWhiteSpace(docId))
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
                && String.Equals(RecommendedChanges, other.RecommendedChanges, StringComparison.Ordinal)
                && String.Equals(SourceCompatibleChanges, other.SourceCompatibleChanges, StringComparison.Ordinal)
                && String.Equals(Component, other.Component, StringComparison.Ordinal);
        }
    }

    public class ApiNote : IComparable<ApiNote>
    {
        public string Title { get; set; }
        public string Html { get; set; }
        public IEnumerable<string> ApplicableApis { get; set; }
        public IEnumerable<string> RelatedNotes { get; set; }

        public int CompareTo(ApiNote other)
        {
            if (other == null)
            {
                return -1;
            }

            return String.CompareOrdinal(Title, other.Title);
        }
    }
}
