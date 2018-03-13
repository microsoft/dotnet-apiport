// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.ObjectModel;
using Newtonsoft.Json;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using WorkflowManagement;

namespace Functions
{
    public static class Analyze
    {
        //Allows ActionFactory to be "injected" as needed, such as with a mock action factory when the function is called through tests
        public static Func<IWorkflowActionFactory> GetActionFactory { get; set; } = () => new WorkflowActionFactory();

        [FunctionName("analyze")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
            [Queue("apiportworkflowqueue")]ICollector<WorkflowQueueMessage> workflowMessageQueue, ILogger log)
        {
            var analyzeRequest = await DeserializeRequest(req.Content);
            if (analyzeRequest == null)
            {
                log.LogError("invalid request");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            var submissionId = Guid.NewGuid().ToString();
            log.LogInformation($"created submission id {submissionId}");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(submissionId);

            var workflowMgr = WorkflowManager.GetInstance(GetActionFactory());
            var msg = workflowMgr.GetFirstStage(submissionId);
            workflowMessageQueue.Add(msg);
            log.LogInformation($"queuing new message {msg.SubmissionId}, stage {msg.Stage}");

            return response;
        }

        public static async Task<AnalyzeRequest> DeserializeRequest(HttpContent content)
        {
            try
            {
                var body = await DecompressContent(content);
                return JsonConvert.DeserializeObject<AnalyzeRequest>(body, DataExtensions.JsonSettings);
            }
            catch
            {
                return null;
            }
        }

        public static async Task<string> DecompressContent(HttpContent content)
        {
            var contentStream = await content.ReadAsStreamAsync();
            using (var gzstream = new GZipStream(contentStream, CompressionMode.Decompress))
            using (var reader = new StreamReader(gzstream))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}
