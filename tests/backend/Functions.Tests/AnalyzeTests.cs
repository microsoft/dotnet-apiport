// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Azure.WebJobs.Host;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Xunit;

namespace Functions.Tests
{
    public class AnalyzeTests
    {
        [Fact]
        public static async Task ReturnsBadRequestForMalformedContent()
        {
            var request = PostFromConsoleApiPort;
            request.Content = new StringContent("{ \"json\": \"json\" }");

            var response = await Analyze.Run(request, new DoNothingTraceWriter());

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public static async Task ReturnsGuidForCompressedAnalyzeRequest()
        {
            var gzippedAnalyzeRequest = typeof(AnalyzeTests).Assembly
                .GetManifestResourceStream("Functions.Tests.Resources.apiport.exe.AnalyzeRequest.json.gz");

            var request = PostFromConsoleApiPort;
            request.SetConfiguration(new HttpConfiguration());
            request.Content = new StreamContent(gzippedAnalyzeRequest);
            request.Content.Headers.Add("Content-Encoding", "gzip");
            request.Content.Headers.Add("Content-Type", "application/json");

            var response = await Analyze.Run(request, new DoNothingTraceWriter());
            var body = await response.Content.ReadAsStringAsync();

            Assert.True(Guid.TryParse(body, out var _));
        }

        private static HttpRequestMessage PostFromConsoleApiPort
        {
            get
            {
                var req = new HttpRequestMessage(HttpMethod.Post, "");
                req.Headers.Add("Accept", "application/json");
                req.Headers.Add("Accept-Encoding", "gzip, deflate");
                req.Headers.Add("Client-Type", "ApiPort_Console");
                req.Headers.Add("Client-Version", "2.4.0.2");
                req.Headers.Add("Expect", "100-continue");

                return req;
            }
        }

        private class DoNothingTraceWriter : TraceWriter
        {
            public DoNothingTraceWriter() : base(TraceLevel.Off)
            { }

            public override void Trace(TraceEvent traceEvent)
            { }
        }
    }
}
