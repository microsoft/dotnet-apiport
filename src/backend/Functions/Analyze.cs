// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.ObjectModel;
using Newtonsoft.Json;
using WorkflowManagement;

namespace Functions
{
    public static class Analyze
    {
        //Allows ActionFactory to be "injected" as needed, such as with a mock action factory when the function is called through tests
        public static Func<WorkflowManager> GetWorkflowManager { get; set; } = () => new WorkflowManager();

        [FunctionName("analyze")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
            [Queue("apiportworkflowqueue")]ICollector<WorkflowQueueMessage> workflowMessageQueue, 
            ILogger log)
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

            var workflowMgr = GetWorkflowManager();
            var msg = workflowMgr.GetFirstStage(submissionId);
            workflowMessageQueue.Add(msg);
            log.LogInformation($"queuing new message {msg.SubmissionId}, stage {msg.Stage}");

            return response;
        }

        public static async Task<AnalyzeRequest> DeserializeRequest(HttpContent content)
        {
            try
            {
                var stream = await content.ReadAsStreamAsync();
                return DataExtensions.DecompressToObject<AnalyzeRequest>(stream);
            }
            catch
            {
                return null;
            }
        }
    }
}
