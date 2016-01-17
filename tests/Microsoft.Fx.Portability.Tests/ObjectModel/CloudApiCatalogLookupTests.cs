using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Tests.TestData;
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
            var dotnetCatalog = new TestDotNetCatalog();
            var catalog = new CloudApiCatalogLookup(dotnetCatalog);

            var expected = ancestors != null ? ancestors.Split(';') : null;

            int index = 0;

            foreach (var result in catalog.GetAncestors(docId))
            {
                Assert.Equal(expected[index], result);
                index++;
            }
        }

        [InlineData("N:System.Collections.Concurrent")]
        [InlineData("N:NonExistentDocId")]
        [Theory]
        public void GetAncestorsTestEmpty(string docId)
        {
            var dotnetCatalog = new TestDotNetCatalog();
            var catalog = new CloudApiCatalogLookup(dotnetCatalog);

            var results = catalog.GetAncestors(docId);

            Assert.Empty(results);
        }
    }
}
