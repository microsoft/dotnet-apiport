// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.ObjectModel;
using PortabilityService.Functions.DependencyInjection;
using PortabilityService.WorkflowManagement;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace PortabilityService.Functions
{
    public static class Analyze
    {
        [FunctionName("analyze")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
            [Queue("%WorkflowQueueName%")]ICollector<WorkflowQueueMessage> workflowMessageQueue,
            [Inject]IStorage storage,
            ILogger log)
        {
            var submissionId = Guid.NewGuid().ToString();
            var analyzeRequest = await DeserializeRequest(req.Content);

            if (analyzeRequest == null)
            {
                log.LogError("Invalid request {SubmissionId}", submissionId);
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            try
            {
                log.LogInformation("Created submission id {SubmissionId}", submissionId);

                var saved = await storage.SaveToBlobAsync(analyzeRequest, submissionId);
                if (!saved)
                {
                    log.LogError("Analyze request not saved to storage for submission {submissionId}", submissionId);
                    return req.CreateResponse(HttpStatusCode.InternalServerError);
                }

                var workflowMgr = WorkflowManager.Initialize();
                var msg = WorkflowManager.GetFirstStage(submissionId);
                workflowMessageQueue.Add(msg);
                log.LogInformation("Queuing new workflow message {SubmissionId}, stage {Stage}", msg.SubmissionId, msg.Stage);

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(submissionId);

                return response;
            }
            catch (Exception ex)
            {
                log.LogError("Error occurred during analyze request for submission {submissionId}: {exception}", submissionId, ex);
                return req.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
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
