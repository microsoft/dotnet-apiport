using System;
using System.Collections.Generic;

namespace Microsoft.Fx.Portability.ObjectModel
{
    public class UsageData
    {
        public UsageData() { }

        public UsageData(string docId, IApiCatalogLookup catalog, IApiRecommendations recommendations)
        {
            Api = new ApiInformation(docId, catalog, recommendations);
        }

        public ApiInformation Api { get; set; }

        public int Count { get; set; }

        public int Index { get; set; }


        public override bool Equals(object obj)
        {
            var other = obj as UsageData;

            if (other == null) return false;

            return Count == other.Count
                && Object.Equals(Api, other.Api);
        }

        public override int GetHashCode()
        {
            return new { Count, Api }.GetHashCode();

        }
    }
}
