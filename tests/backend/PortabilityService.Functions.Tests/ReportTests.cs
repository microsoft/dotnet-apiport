// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;
using Microsoft.Fx.Portability.Azure;
using Microsoft.Fx.Portability.ObjectModel;
using NSubstitute;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;

namespace PortabilityService.Functions.Tests
{
    public class ReportTests
    {
        [Fact]
        public async Task ReturnsUnauthorizedForInvalidToken()
        {
            var storage = Substitute.For<IStorage>();
            storage.RetrieveRequestAsync(Arg.Any<string>()).Returns(new AnalyzeRequest());

            var tokenValidator = Substitute.For<IReportTokenValidator>();
            tokenValidator.RequestHasValidToken(Arg.Any<HttpRequestMessage>()).Returns(false);

            var request = new HttpRequestMessage(HttpMethod.Get, "");

            var response = await Report.Run(request, "submissionId", storage, tokenValidator, Substitute.For<ILogger>());

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task ReturnsAcceptedWhenAnalysisNotComplete()
        {
            var storage = Substitute.For<IStorage>();
            storage.RetrieveRequestAsync(Arg.Any<string>()).Returns(new AnalyzeRequest());

            var tokenValidator = Substitute.For<IReportTokenValidator>();
            tokenValidator.RequestHasValidToken(Arg.Any<HttpRequestMessage>()).Returns(true);

            var request = new HttpRequestMessage(HttpMethod.Get, "");

            var response = await Report.Run(request, "submissionId", storage, tokenValidator, Substitute.For<ILogger>());

            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        }

        [Fact]
        public async Task AcceptedResponseIncludesRetryAfter()
        {
            var storage = Substitute.For<IStorage>();
            storage.RetrieveRequestAsync(Arg.Any<string>()).Returns(new AnalyzeRequest());

            var tokenValidator = Substitute.For<IReportTokenValidator>();
            tokenValidator.RequestHasValidToken(Arg.Any<HttpRequestMessage>()).Returns(true);

            var request = new HttpRequestMessage(HttpMethod.Get, "");

            var response = await Report.Run(request, "submissionId", storage, tokenValidator, Substitute.For<ILogger>());

            Assert.NotNull(response.Headers.RetryAfter.Delta);
        }

        [Fact]
        public async Task ReturnsUnsupportedMediaTypeWhenNoReportWriterFound()
        {
            var storage = Substitute.For<IStorage>();
            storage.RetrieveRequestAsync(Arg.Any<string>()).Returns(new AnalyzeRequest());
            storage.RetrieveResultFromBlobAsync(Arg.Any<string>()).Returns(new AnalyzeResult());

            var tokenValidator = Substitute.For<IReportTokenValidator>();
            tokenValidator.RequestHasValidToken(Arg.Any<HttpRequestMessage>()).Returns(true);

            var request = new HttpRequestMessage(HttpMethod.Get, "");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("unknown/mediatype"));

            var response = await Report.Run(request, "submissionId", storage, tokenValidator, Substitute.For<ILogger>());

            Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
        }
    }
}
