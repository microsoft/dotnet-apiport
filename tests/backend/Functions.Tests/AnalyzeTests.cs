// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.ObjectModel;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using WorkflowManagement;
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

            var storage = Substitute.For<IStorage>();
            var response = await Analyze.Run(request, Substitute.For<ICollector<WorkflowQueueMessage>>(), storage, NullLogger.Instance);

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

            WorkflowManager.Initialize();
            var workflowQueue = Substitute.For<ICollector<WorkflowQueueMessage>>();
            var storage = Substitute.For<IStorage>();
            storage.SaveToBlobAsync(Arg.Any<AnalyzeRequest>(), Arg.Any<string>()).Returns(Task.FromResult(true));
            var response = await Analyze.Run(request, workflowQueue, storage, NullLogger.Instance);
            var body = await response.Content.ReadAsStringAsync();

            Assert.True(Guid.TryParse(body, out var submissionId));

            workflowQueue.Received().Add(Arg.Is<WorkflowQueueMessage>(x => x.SubmissionId == submissionId.ToString() && x.Stage == WorkflowStage.Analyze));
        }

        [Fact]
        public static async Task SavedRequestMatchesOriginal()
        {
            var gzippedAnalyzeRequestStream = typeof(AnalyzeTests).Assembly
                .GetManifestResourceStream("Functions.Tests.Resources.apiport.exe.AnalyzeRequest.json.gz");

            var expectedStream = new MemoryStream();
            gzippedAnalyzeRequestStream.CopyTo(expectedStream);
            gzippedAnalyzeRequestStream.Seek(0, SeekOrigin.Begin);

            var request = PostFromConsoleApiPort;
            request.SetConfiguration(new HttpConfiguration());
            request.Content = new StreamContent(gzippedAnalyzeRequestStream);
            request.Content.Headers.Add("Content-Encoding", "gzip");
            request.Content.Headers.Add("Content-Type", "application/json");

            WorkflowManager.Initialize();
            var workflowQueue = Substitute.For<ICollector<WorkflowQueueMessage>>();
            var storage = new TestStorage();
            var response = await Analyze.Run(request, workflowQueue, storage, NullLogger.Instance);

            Assert.Equal(expectedStream.ToArray(), storage.Stored);
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

        private class TestStorage : IStorage
        {
            public byte[] Stored { get; set; }

            public Task AddJobToQueueAsync(string submissionId)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<ProjectSubmission> GetProjectSubmissions()
            {
                throw new NotImplementedException();
            }

            public Task<IEnumerable<UsageData>> GetUsageDataAsync()
            {
                throw new NotImplementedException();
            }

            public Task<AnalyzeRequest> RetrieveRequestAsync(string uniqueId)
            {
                throw new NotImplementedException();
            }

            public Task<IEnumerable<string>> RetrieveSubmissionIdsAsync()
            {
                throw new NotImplementedException();
            }

            public Task<bool> SaveToBlobAsync(AnalyzeRequest analyzeRequest, string submissionId)
            {
                Stored = analyzeRequest.SerializeAndCompress();
                return Task.FromResult(true);
            }
        }
    }
}
