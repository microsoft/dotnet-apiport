using Microsoft.Fx.Portability.ObjectModel;
using System.Reflection;
using Xunit;

namespace Microsoft.Fx.Portability.Tests.ObjectModel
{
    public class CloudApiCatalogLookupTests
    {
        [InlineData("N:System.Collections", null)]
        [InlineData("M:System.Collections.Concurrent.ConcurrentBag`1.get_Count", "P:System.Collections.Concurrent.ConcurrentBag`1.Count;T:System.Collections.Concurrent.ConcurrentBag`1;N:System.Collections.Concurrent")]
        [InlineData("T:System.Collections.Concurrent.ConcurrentBag`1", "N:System.Collections.Concurrent")]
        [Theory]
        public void GetAncestorsTest(string docId, string ancestors)
        {
            var dotnetCatalog = GetDotNetCatalog();
            var catalog = new CloudApiCatalogLookup(dotnetCatalog);

            var expected = ancestors != null ? ancestors.Split(';') : null;

            int index = 0;

            foreach (var result in catalog.GetAncestors(docId))
            {
                Assert.Equal(expected[index], result);
                index++;
            }
        }

        private static DotNetCatalog GetDotNetCatalog()
        {
            const string catalogName = "Microsoft.Fx.Portability.Tests.TestAssets.DummyApiCatalog.json";

            DotNetCatalog catalog = null;

            using (var template = typeof(CloudApiCatalogLookupTests).GetTypeInfo().Assembly.GetManifestResourceStream(catalogName))
            {
                catalog = template.Deserialize<DotNetCatalog>();
            }

            return catalog;
        }
    }
}
