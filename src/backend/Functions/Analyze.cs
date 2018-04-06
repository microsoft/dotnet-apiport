// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.ObjectModel;
using WorkflowManagement;

namespace Functions
{
    public static class Analyze
    {
        [FunctionName("analyze")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
            [Queue("apiportworkflowqueue")]ICollector<WorkflowQueueMessage> workflowMessageQueue, 
            ILogger log)
        {
            var submissionId = Guid.NewGuid().ToString();

            var analyzeRequest = await DeserializeRequest(req.Content);
            if (analyzeRequest == null)
            {
                log.LogError("Invalid request {SubmissionId}", submissionId);
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            log.LogInformation("Created submission id {SubmissionId}", submissionId);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(submissionId);

            var workflowMgr = WorkflowManager.Initialize();
            var msg = WorkflowManager.GetFirstStage(submissionId);
            workflowMessageQueue.Add(msg);
            log.LogInformation("Queuing new message {SubmissionId}, stage {Stage}", msg.SubmissionId, msg.Stage);

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
