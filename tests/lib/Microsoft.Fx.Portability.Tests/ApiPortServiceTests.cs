// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Fx.Portability.Tests
{
    public sealed class ApiPortServiceTests : IDisposable
    {
        private readonly ApiPortService _apiPortService;

        public ApiPortServiceTests()
        {
            var httpMessageHandler = new TestHandler(HttpRequestConverter);
            var productInformation = new ProductInformation("ApiPort_Tests");

            //Create a fake ApiPortService which uses the TestHandler to send back the response message
            _apiPortService = new ApiPortService("http://localhost", httpMessageHandler, productInformation);
        }

        public void Dispose()
        {
            _apiPortService.Dispose();
        }

        [Fact]
        public static void VerifyParameterChecks()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ApiPortService(null, new ProductInformation("")));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ApiPortService(string.Empty, new ProductInformation("")));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ApiPortService(" \t", new ProductInformation("")));
        }

        [Fact]
        public async Task ApiPortService_GetDocIdsWithValidDocIdAsync()
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

            var serviceResponse = await _apiPortService.QueryDocIdsAsync(docIds);
            var result = serviceResponse.Response;

            Assert.Equal(docIds.Count(), result.Count());
            Assert.Empty(docIds.Except(result.Select(r => r.Definition.DocId)));
        }

        [Fact]
        public async Task ApiPortService_GetAvailableFormatsAsync()
        {
            var expected = new List<string> { "Json", "HTML", "Excel" };

            var serviceResponse = await _apiPortService.GetResultFormatsAsync();
            var result = serviceResponse.Response;

            Assert.Equal(expected.Count(), result.Count());
            Assert.Empty(expected.Except(result.Select(r => r.DisplayName)));
        }

        private static HttpResponseMessage HttpRequestConverter(HttpRequestMessage request)
        {
            string resourceFile = null;
            var query = request.RequestUri.PathAndQuery;
            if (string.Equals(query, "/api/resultformat", StringComparison.OrdinalIgnoreCase))
            {
                resourceFile = "FormatsHttpContent.json";
            }
            else if (string.Equals(query, "/api/fxapi", StringComparison.OrdinalIgnoreCase))
            {
                resourceFile = "DocIdsHttpContent.json";
            }
            else
            {
                return null;
            }

            var assembly = typeof(ApiPortServiceTests).GetTypeInfo().Assembly;
            var resourceName = assembly.GetManifestResourceNames().Single(n => n.EndsWith(resourceFile, StringComparison.Ordinal));
            var resourceStream = assembly.GetManifestResourceStream(resourceName);

            var streamContent = new StreamContent(resourceStream);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            response.Content = streamContent;
            return response;
        }
    }

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
    }
}
