// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DependencyInjection;
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
            [Inject]IStorage storage,
            ILogger log)
        {
            var analyzeRequest = await DeserializeRequest(req.Content);
            if (analyzeRequest == null)
            {
                log.LogError("invalid request");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            var submissionId = Guid.NewGuid().ToString();
            log.LogInformation("Created submission id {SubmissionId}", submissionId);

            try
            {
                var saved = await storage.SaveToBlobAsync(analyzeRequest, submissionId);
                if (!saved)
                {
                    log.LogError("Analyze request not saved to storage for submission {submissionId}", submissionId);
                    return req.CreateResponse(HttpStatusCode.InternalServerError);
                }
            }
            catch (Exception ex)
            {
                log.LogError("Error occurs when saving analyze request to storage for submission {submissionId}: {exception}", submissionId, ex);
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }

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
