// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Xunit;
using Functions.Tests.Mock;
using WorkflowManagement;
using Microsoft.Extensions.Logging.Abstractions;

namespace Functions.Tests
{
    public class AnalyzeTests
    {
        [Fact]
        public static async Task ReturnsBadRequestForMalformedContent()
        {
            var request = PostFromConsoleApiPort;
            request.Content = new StringContent("{ \"json\": \"json\" }");

            var response = await Analyze.Run(request, new MockCollector<WorkflowQueueMessage>(), NullLogger.Instance);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public static async Task ReturnsGuidForCompressedAnalyzeRequest()
        {
            Analyze.GetActionFactory = () => new MockActionFactory();

            var gzippedAnalyzeRequest = typeof(AnalyzeTests).Assembly
                .GetManifestResourceStream("Functions.Tests.Resources.apiport.exe.AnalyzeRequest.json.gz");

            var request = PostFromConsoleApiPort;
            request.SetConfiguration(new HttpConfiguration());
            request.Content = new StreamContent(gzippedAnalyzeRequest);
            request.Content.Headers.Add("Content-Encoding", "gzip");
            request.Content.Headers.Add("Content-Type", "application/json");

            var workflowQueue = new MockCollector<WorkflowQueueMessage>();
            var response = await Analyze.Run(request, workflowQueue, NullLogger.Instance);
            var body = await response.Content.ReadAsStringAsync();

            Guid submissionId;
            Assert.True(Guid.TryParse(body, out submissionId));

            Assert.Single(workflowQueue.Items);
            var msg = (WorkflowQueueMessage)workflowQueue.Items[0];
            Assert.Equal(submissionId.ToString(), msg.SubmissionId);
            Assert.Equal(WorkflowStage.Analyze, msg.Stage);
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
    }
}
