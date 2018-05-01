// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Resources;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static System.Reflection.IntrospectionExtensions;

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
            _apiPortService = new ApiPortService("http://localhost", httpMessageHandler, productInformation, null);
        }

        public void Dispose()
        {
            _apiPortService.Dispose();
        }

        [Fact]
        public static void VerifyParameterChecks()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ApiPortService(null, new ProductInformation(""), null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ApiPortService(string.Empty, new ProductInformation(""), null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ApiPortService(" \t", new ProductInformation(""), null));
        }

        [Fact(Skip = "QueryDocIdsAsync not yet implemented")] // TODO
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

            var result = await _apiPortService.QueryDocIdsAsync(docIds);

            Assert.Equal(docIds.Count(), result.Count());
            Assert.Empty(docIds.Except(Enumerable.Select(result, (r => r.Definition.DocId))));
        }

        [Fact]
        public async Task CompressesAnalyzeRequest()
        {
            var handler = new TestHandler(request =>
            {
                var content = request.Content.ReadAsStreamAsync().GetAwaiter().GetResult();

                // verify the content is a compressed serialization of the expected AnalyzeRequest
                var actual = DataExtensions.DecompressToObject<AnalyzeRequest>(content);
                Assert.Equal(actual.ApplicationName, MockAnalyzeRequest.ApplicationName);

                return new HttpResponseMessage { Content = new StringContent("{}"), StatusCode = HttpStatusCode.OK };
            });

            var service = new ApiPortService("http://localhost", handler, new ProductInformation(""), Substitute.For<IProgressReporter>());
            await service.RequestAnalysisAsync(MockAnalyzeRequest);
        }

        [Fact]
        public async Task SetsClientTypeAndVersionHeadersOnAllRequests()
        {
            string responseJson = "";
            var handler = new TestHandler(request =>
            {
                Assert.True(request.Headers.TryGetValues("Client-Type", out _));
                Assert.True(request.Headers.TryGetValues("Client-Version", out _));

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(responseJson)
                };
            });

            var service = new ApiPortService("http://localhost", handler, new ProductInformation(""), Substitute.For<IProgressReporter>());

            responseJson = "{}"; // these methods expect a json object
            await service.RequestAnalysisAsync(MockAnalyzeRequest);
            await service.GetDefaultResultFormatAsync();
            await service.GetReportingResultAsync(
                new AnalyzeResponse { ResultUrl = new Uri("http://localhost") },
                new ResultFormatInformation { MimeType = "foo/bar" }
            );
            // TODO GetApiInformationAsync isn't implemented
            // await service.GetApiInformationAsync("docId");

            responseJson = "[{}]"; // these methods expect a json array
            await service.GetResultFormatsAsync();
            await service.GetTargetsAsync();
            // TODO QueryDocIdsAsync isn't implemented
            // await service.QueryDocIdsAsync(new[] { "docId" });
        }

        [Fact]
        public async Task ReportsDeprecatedEndpoint()
        {
            var handler = new TestHandler(request =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{}")
                };
                response.Headers.Add(typeof(EndpointStatus).Name, EndpointStatus.Deprecated.ToString());

                return response;
            });
            var reporter = Substitute.For<IProgressReporter>();
            var service = new ApiPortService("http://localhost", handler, new ProductInformation(""), reporter);

            await service.RequestAnalysisAsync(MockAnalyzeRequest);

            reporter.Received()
                .ReportIssue(Arg.Is<string>(
                    message => string.Equals(message, LocalizedStrings.ServerEndpointDeprecated, StringComparison.Ordinal)
                ));
        }

        [Fact]
        public async Task GetsReportFromAnalyzeResponseUri()
        {
            var expectedUri = new Uri("http://localhost/expected-url");
            var handler = new TestHandler(request =>
            {
                Assert.Equal(HttpMethod.Get, request.Method);
                Assert.Equal(expectedUri, request.RequestUri);

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{}")
                };
            });
            var service = new ApiPortService("http://localhost", handler, new ProductInformation(""), Substitute.For<IProgressReporter>());
            var analyzeResponse = new AnalyzeResponse { ResultUrl = expectedUri };

            await service.GetReportingResultAsync(analyzeResponse, new ResultFormatInformation { MimeType = "foo/bar" });
        }

        [Fact]
        public async Task IncludesAuthTokenWhenGettingResults()
        {
            var expectedAuthToken = "authtoken";
            var handler = new TestHandler(request =>
            {
                Assert.Equal(HttpMethod.Get, request.Method);
                Assert.Equal(expectedAuthToken, request.Headers.Authorization.Parameter);

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{}")
                };
            });
            var analyzeResponse = new AnalyzeResponse
            {
                ResultAuthToken = expectedAuthToken,
                ResultUrl = new Uri("http://localhost"),
            };
            var service = new ApiPortService("http://localhost", handler, new ProductInformation(""), Substitute.For<IProgressReporter>());

            await service.GetReportingResultAsync(analyzeResponse, new ResultFormatInformation { MimeType = "foo/bar" });
        }

        private AnalyzeRequest MockAnalyzeRequest => new AnalyzeRequest
        {
            ApplicationName = "name",
            Dependencies = new Dictionary<MemberInfo, ICollection<AssemblyInfo>>
            {
                {
                    new MemberInfo { MemberDocId = "item1" },
                    new HashSet<AssemblyInfo>
                    {
                        new AssemblyInfo { AssemblyIdentity = "string1" }, new AssemblyInfo { AssemblyIdentity = "string2" }
                    }
                }
            },
            Targets = new List<string> { "target1", "target2" },
            UnresolvedAssemblies = new List<string> { "assembly1", "assembly2" },
            UserAssemblies = new List<AssemblyInfo>
            {
                new AssemblyInfo { AssemblyIdentity = "name1" }, new AssemblyInfo { AssemblyIdentity = "name2" }
            },
            Version = AnalyzeRequest.CurrentVersion
        };

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

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = streamContent
            };

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
