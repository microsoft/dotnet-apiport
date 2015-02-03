using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Fx.Portability.Tests
{
    [TestClass]
    public class UrlBuilderTest
    {
        [TestMethod]
        public void NoParameters()
        {
            var url = UrlBuilder.Create("test").Url;

            Assert.AreEqual("test", url);
        }

        [TestMethod]
        public void AddQueryParam()
        {
            var url = UrlBuilder.Create("test").AddQuery("name", "value").Url;

            Assert.AreEqual("test?name=value", url);
        }

        [TestMethod]
        public void AddNullableQueryParam()
        {
            int? value = 5;

            var url = UrlBuilder.Create("test").AddQuery("name", "value").AddQuery("test", value).Url;

            Assert.AreEqual("test?name=value&test=5", url);
        }

        [TestMethod]
        public void AddNullableQueryParamNull()
        {
            int? value = null;

            var url = UrlBuilder.Create("test").AddQuery("name", "value").AddQuery("test", value).Url;

            Assert.AreEqual("test?name=value", url);
        }

        [TestMethod]
        public void AddQueryListNull()
        {
            var url = UrlBuilder.Create("test").AddQuery("name", "value").AddQueryList("test", null).Url;

            Assert.AreEqual("test?name=value", url);
        }

        [TestMethod]
        public void AddQueryList1Item()
        {
            var list = new[] { "test1" };

            var url = UrlBuilder.Create("test").AddQuery("name", "value").AddQueryList("test", list).Url;

            Assert.AreEqual("test?name=value&test=test1", url);
        }

        [TestMethod]
        public void AddQueryListMultiple()
        {
            var list = new[] { "test1", "test2", "test3" };
            var url = UrlBuilder.Create("test").AddQuery("name", "value").AddQueryList("test", list).Url;

            Assert.AreEqual("test?name=value&test=test1&test=test2&test=test3", url);
        }

        [TestMethod]
        public void AddQueryListMultipleNullItem()
        {
            var list = new[] { "test1", null, "test3" };
            var url = UrlBuilder.Create("test").AddQuery("name", "value").AddQueryList("test", list).Url;

            Assert.AreEqual("test?name=value&test=test1&test=test3", url);
        }

        [TestMethod]
        public void AddODataParam()
        {
            var url = UrlBuilder.Create("test").AddODataQuery("top", 10).Url;

            Assert.AreEqual("test?$top=10", url);
        }

        [TestMethod]
        public void HttpEncodeValues()
        {
            var url = UrlBuilder.Create("test").AddQuery("name with spaces", "value with spaces").Url;

            Assert.AreEqual("test?name%20with%20spaces=value%20with%20spaces", url);
        }

        [TestMethod]
        public void HttpEncodeValuesOData()
        {
            var url = UrlBuilder.Create("test").AddODataQuery("name with spaces", "value with spaces").Url;

            Assert.AreEqual("test?$name%20with%20spaces=value%20with%20spaces", url);
        }

        [TestMethod]
        public void AddPath()
        {
            var url = UrlBuilder.Create("test").AddPath("blah ");

            Assert.AreEqual("test/blah%20", url.Url);
        }

        [TestMethod]
        public void NullObject()
        {
            var original = UrlBuilder.Create("test");
            var withNull = original.AddQuery("null", null);

            Assert.AreSame(original, withNull);
            Assert.AreEqual("test", withNull.Url);
        }
    }
}
