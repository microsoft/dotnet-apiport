// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
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
        private const string ServiceEndpoint = "http://portability.cloudapp.net";

        private readonly ApiPortService _apiPortService = new ApiPortService(
            ServiceEndpoint,
            new ProductInformation("ApiPort_Tests", typeof(ApiPortServiceTests)));

        [Fact]
        public void VerifyParameterChecks()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ApiPortService(null, new ProductInformation("")));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ApiPortService(string.Empty, new ProductInformation("")));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ApiPortService(" \t", new ProductInformation("")));
        }

        [Fact(Skip = "Skipping because this will ping the live service when running.")]
        public void ApiPortService_GetDocIdsWithValidDocId()
        {
            var apiPortService = new ApiPortService(
                ServiceEndpoint,
                new ProductInformation("ApiPort_Tests", typeof(ApiPortServiceTests)));

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

            var result = apiPortService.QueryDocIdsAsync(docIds).Result.Response;
            Assert.Equal(docIds.Count(), result.Count());
            Assert.Equal(0, docIds.Except(result.Select(r => r.Definition.DocId)).Count());
        }

        [Fact(Skip = "Skipping because this will ping the live service when running.")]
        public void ApiPortService_GetAvailableFormats()
        {
            var expected = new List<string> { "Json", "HTML", "Excel" };

            var apiPortService = new ApiPortService(
                ServiceEndpoint,
                new ProductInformation("ApiPort_Tests", typeof(ApiPortServiceTests)));

            var result = apiPortService.GetResultFormatsAsync().Result.Response;
            Assert.Equal(expected.Count(), result.Count());
            Assert.Equal(0, expected.Except(result.Select(r => r.DisplayName)).Count());
        }

    }
}
