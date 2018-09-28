// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace Microsoft.Fx.Portability.Tests
{
#if FEATURE_STRONGNAMESIGNING
    public class UrlBuilderTest
    {
        [Fact]
        public static void NoParameters()
        {
            var url = UrlBuilder.Create("test").Url;

            Assert.Equal("test", url);
        }

        [Fact]
        public static void AddQueryParam()
        {
            var url = UrlBuilder.Create("test").AddQuery("name", "value").Url;

            Assert.Equal("test?name=value", url);
        }

        [Fact]
        public static void AddNullableQueryParam()
        {
            int? value = 5;

            var url = UrlBuilder.Create("test").AddQuery("name", "value").AddQuery("test", value).Url;

            Assert.Equal("test?name=value&test=5", url);
        }

        [Fact]
        public static void AddNullableQueryParamNull()
        {
            int? value = null;

            var url = UrlBuilder.Create("test").AddQuery("name", "value").AddQuery("test", value).Url;

            Assert.Equal("test?name=value", url);
        }

        [Fact]
        public static void AddQueryListNull()
        {
            var url = UrlBuilder.Create("test").AddQuery("name", "value").AddQueryList("test", null).Url;

            Assert.Equal("test?name=value", url);
        }

        [Fact]
        public static void AddQueryList1Item()
        {
            var list = new[] { "test1" };

            var url = UrlBuilder.Create("test").AddQuery("name", "value").AddQueryList("test", list).Url;

            Assert.Equal("test?name=value&test=test1", url);
        }

        [Fact]
        public static void AddQueryListMultiple()
        {
            var list = new[] { "test1", "test2", "test3" };
            var url = UrlBuilder.Create("test").AddQuery("name", "value").AddQueryList("test", list).Url;

            Assert.Equal("test?name=value&test=test1&test=test2&test=test3", url);
        }

        [Fact]
        public static void AddQueryListMultipleNullItem()
        {
            var list = new[] { "test1", null, "test3" };
            var url = UrlBuilder.Create("test").AddQuery("name", "value").AddQueryList("test", list).Url;

            Assert.Equal("test?name=value&test=test1&test=test3", url);
        }

        [Fact]
        public static void AddODataParam()
        {
            var url = UrlBuilder.Create("test").AddODataQuery("top", 10).Url;

            Assert.Equal("test?$top=10", url);
        }

        [Fact]
        public static void HttpEncodeValues()
        {
            var url = UrlBuilder.Create("test").AddQuery("name with spaces", "value with spaces").Url;

            Assert.Equal("test?name%20with%20spaces=value%20with%20spaces", url);
        }

        [Fact]
        public static void HttpEncodeValuesOData()
        {
            var url = UrlBuilder.Create("test").AddODataQuery("name with spaces", "value with spaces").Url;

            Assert.Equal("test?$name%20with%20spaces=value%20with%20spaces", url);
        }

        [Fact]
        public static void AddPath()
        {
            var url = UrlBuilder.Create("test").AddPath("blah ");

            Assert.Equal("test/blah%20", url.Url);
        }

        [Fact]
        public static void NullObject()
        {
            var original = UrlBuilder.Create("test");
            var withNull = original.AddQuery("null", null);

            Assert.Same(original, withNull);
            Assert.Equal("test", withNull.Url);
        }
    }
#endif
}
