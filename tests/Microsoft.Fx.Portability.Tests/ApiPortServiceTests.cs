// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Fx.Portability.Tests
{
    /*TODO: This should be Mocking the ApiPortService by adding this functionality into the
            CompressedHttpClient in order to Mock
        internal class TestHandler : HttpMessageHandler
        {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _converter;

        public TestHandler(Func<HttpRequestMessage, HttpResponseMessage> converter)
        {
            _converter = converter;
        }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_converter(request));
        }
    } */

    public class ApiPortServiceTests
    {
        private static readonly ApiPortService s_apiPortService = new ApiPortService(
            "http://portability.dot.net",
            new ProductInformation("ApiPort_Tests", typeof(ApiPortServiceTests)));
        private static readonly ApiPortService s_oldApiService = new ApiPortService(
            "http://portability.cloudapp.net",
            new ProductInformation("ApiPort_Tests", typeof(ApiPortServiceTests)));

        public static IEnumerable<object[]> ApiPortServices
        {
            get
            {
                yield return new object[] { s_apiPortService, EndpointStatus.Alive };
                yield return new object[] { s_oldApiService, EndpointStatus.Deprecated };
            }
        }

        [Fact]
        public void VerifyParameterChecks()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ApiPortService(null, new ProductInformation("")));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ApiPortService(string.Empty, new ProductInformation("")));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ApiPortService(" \t", new ProductInformation("")));
        }

        [Theory(Skip = "Skipping because this will ping the live service when running.")]
        [MemberData("ApiPortServices")]
        public async Task ApiPortService_GetDocIdsWithValidDocIdAsync(ApiPortService apiPortService, EndpointStatus expectedStatus)
        {
            var docIds = new List<string>
            {
                "T:System.Console",
                "M:System.Xml.Serialization.XmlSerializer.Serialize(System.IO.Stream,System.Object)",
                "M:System.Xml.Serialization.XmlSerializer.Serialize(System.IO.TextWriter,System.Object)",
                "M:System.Xml.Serialization.XmlSerializer.Serialize(System.Object,System.Xml.Serialization.XmlSerializationWriter)",
                "M:System.Xml.Serialization.XmlSerializer.Serialize(System.Xml.XmlWriter,System.Object)",
                "M:System.Xml.Serialization.XmlSerializer.Serialize(System.IO.Stream,System.Object,System.Xml.Serialization.XmlSerializerNamespaces)",
                "M:System.Xml.Serialization.XmlSerializer.Serialize(System.IO.TextWriter,System.Object,System.Xml.Serialization.XmlSerializerNamespaces)",
                "M:System.Xml.Serialization.XmlSerializer.Serialize(System.Xml.XmlWriter,System.Object,System.Xml.Serialization.XmlSerializerNamespaces)",
                "M:System.Xml.Serialization.XmlSerializer.Serialize(System.Xml.XmlWriter,System.Object,System.Xml.Serialization.XmlSerializerNamespaces,System.String)"
            };

            var serviceResponse = await apiPortService.QueryDocIdsAsync(docIds);
            var headers = serviceResponse.Headers;
            var result = serviceResponse.Response;

            Assert.Equal(expectedStatus, headers.Status);
            Assert.Equal(docIds.Count(), result.Count());
            Assert.Equal(0, docIds.Except(result.Select(r => r.Definition.DocId)).Count());
        }

        [Theory(Skip = "Skipping because this will ping the live service when running.")]
        [MemberData("ApiPortServices")]
        public async Task ApiPortService_GetAvailableFormatsAsync(ApiPortService apiPortService, EndpointStatus expectedStatus)
        {
            var expected = new List<string> { "Json", "HTML", "Excel" };

            var serviceResponse = await apiPortService.GetResultFormatsAsync();
            var headers = serviceResponse.Headers;
            var result = serviceResponse.Response;

            Assert.Equal(expectedStatus, headers.Status);
            Assert.Equal(expected.Count(), result.Count());
            Assert.Equal(0, expected.Except(result.Select(r => r.DisplayName)).Count());
        }
    }
}
