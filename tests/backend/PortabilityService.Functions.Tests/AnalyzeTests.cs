// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.ObjectModel;
using NSubstitute;
using PortabilityService.WorkflowManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Xunit;

namespace PortabilityService.Functions.Tests
{
    public class AnalyzeTests
    {
        [Fact]
        public static async Task ReturnsBadRequestForMalformedContent()
        {
            // Arrange
            var request = PostFromConsoleApiPort;
            request.Content = new StringContent("{ \"json\": \"json\" }");

            var storage = Substitute.For<IStorage>();
            var workflowQueue = Substitute.For<ICollector<WorkflowQueueMessage>>();
            
            // Act
            var response = await Analyze.Run(request, workflowQueue, storage, NullLogger.Instance);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public static async Task ReturnsAnalyzeResponseForCompressedAnalyzeRequest()
        {
            // Arrange
            var request = PostFromConsoleApiPort;

            var workflowQueue = Substitute.For<ICollector<WorkflowQueueMessage>>();
            var storage = Substitute.For<IStorage>();
            storage.SaveRequestToBlobAsync(Arg.Any<AnalyzeRequest>(), Arg.Any<string>()).Returns(Task.FromResult(true));

            // Act
            var response = await Analyze.Run(request, workflowQueue, storage, NullLogger.Instance);

            // Assert
            var analyzeResponse = await response.Content.ReadAsAsync<AnalyzeResponse>();
            Assert.IsType<AnalyzeResponse>(analyzeResponse);
        }

        [Fact]
        public static async Task EnqueuesWorkflowMessage()
        {
            var request = PostFromConsoleApiPort;

            var workflowQueue = Substitute.For<ICollector<WorkflowQueueMessage>>();
            var storage = Substitute.For<IStorage>();
            storage.SaveToBlobAsync(Arg.Any<AnalyzeRequest>(), Arg.Any<string>()).Returns(Task.FromResult(true));

            var response = await Analyze.Run(request, workflowQueue, storage, NullLogger.Instance);
            var analyzeResponse = await response.Content.ReadAsAsync<AnalyzeResponse>();

            workflowQueue.Received()
                .Add(Arg.Is<WorkflowQueueMessage>(x => x.SubmissionId == analyzeResponse.SubmissionId &&
                                                       x.Stage == WorkflowStage.Analyze));
        }

        [Fact]
        public static async Task SavedRequestMatchesOriginal()
        {
            // Arrange
            var gzippedAnalyzeRequestStream = typeof(AnalyzeTests).Assembly
                .GetManifestResourceStream("AnalyzeRequest.json.gz");

            var expectedStream = new MemoryStream();
            gzippedAnalyzeRequestStream.CopyTo(expectedStream);
            gzippedAnalyzeRequestStream.Seek(0, SeekOrigin.Begin);

            var request = PostFromConsoleApiPort;

            var workflowQueue = Substitute.For<ICollector<WorkflowQueueMessage>>();
            var storage = new TestStorage();

            // Act
            var response = await Analyze.Run(request, workflowQueue, storage, NullLogger.Instance);

            // Assert
            Assert.Equal(expectedStream.ToArray(), storage.Stored);
        }

        [Fact]
        public static async Task ResponseHasEndpointStatusHeader()
        {
            var request = PostFromConsoleApiPort;

            var workflowQueue = Substitute.For<ICollector<WorkflowQueueMessage>>();
            var storage = Substitute.For<IStorage>();
            storage.SaveToBlobAsync(null, null).ReturnsForAnyArgs(true);

            var response = await Analyze.Run(request, workflowQueue, storage, NullLogger.Instance);

            Assert.True(response.Headers.TryGetValues(nameof(EndpointStatus), out _));
        }

        [Fact]
        public static async Task ResponseHasLocationHeader()
        {
            var request = PostFromConsoleApiPort;

            var workflowQueue = Substitute.For<ICollector<WorkflowQueueMessage>>();
            var storage = Substitute.For<IStorage>();
            storage.SaveToBlobAsync(null, null).ReturnsForAnyArgs(Task.FromResult(true));

            var response = await Analyze.Run(request, workflowQueue, storage, NullLogger.Instance);

            Assert.NotNull(response.Headers.Location);
        }

        private static HttpRequestMessage PostFromConsoleApiPort
        {
            get
            {
                var req = new HttpRequestMessage(HttpMethod.Post, "http://portability.local/api/analyze");
                req.SetConfiguration(new HttpConfiguration());

                req.Headers.Add("Accept", "application/json");
                req.Headers.Add("Accept-Encoding", "gzip, deflate");
                req.Headers.Add("Client-Type", "ApiPort_Console");
                req.Headers.Add("Client-Version", "2.4.0.2");
                req.Headers.Add("Expect", "100-continue");

                var gzippedAnalyzeRequest = typeof(AnalyzeTests).Assembly
                    .GetManifestResourceStream("AnalyzeRequest.json.gz");
                req.Content = new StreamContent(gzippedAnalyzeRequest);
                req.Content.Headers.Add("Content-Encoding", "gzip");
                req.Content.Headers.Add("Content-Type", "application/json");

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

            public Task DeleteResultFromBlobAsync(string submissionid)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<ProjectSubmission> GetProjectSubmissions()
            {
                throw new NotImplementedException();
            }

            public Task<AnalyzeRequest> RetrieveRequestAsync(string uniqueId)
            {
                throw new NotImplementedException();
            }

            public Task<AnalyzeResponse> RetrieveResultFromBlobAsync(string submissionId)
            {
                throw new NotImplementedException();
            }

            public Task<IEnumerable<string>> RetrieveSubmissionIdsAsync()
            {
                throw new NotImplementedException();
            }

            public Task SaveRequestToBlobAsync(AnalyzeRequest analyzeRequest, string submissionId)
            {
                Stored = analyzeRequest.SerializeAndCompress();
                return Task.FromResult(true);
            }

            public Task SaveResultToBlobAsync(string submissionId, AnalyzeResponse result)
            {
                throw new NotImplementedException();
            }
        }
    }
}
