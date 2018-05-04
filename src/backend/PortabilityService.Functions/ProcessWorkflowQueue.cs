// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using PortabilityService.WorkflowManagement;

namespace PortabilityService.Functions
{
    public static class ProcessWorkflowQueue
    {
        [FunctionName("ProcessWorkflowQueue")]
        public static async Task Run([QueueTrigger("apiportworkflowqueue")]WorkflowQueueMessage workflowMessage,
            [Queue("apiportworkflowqueue")]ICollector<WorkflowQueueMessage> workflowMessageQueue,
            ILogger log,
            CancellationToken cancelToken)
        {
            Action<WorkflowQueueMessage> cancelAsync = (msg) =>
            {
                log.LogError("Timeout during workflow action execution. SubmissionId: {SubmissionId} Stage: {Stage}.", msg.SubmissionId, msg.Stage);
                throw new TimeoutException($"Timeout during workflow action execution. SubmissionId: {msg.SubmissionId}, Stage: {msg.Stage}");
            };

            //This cancellation token will let us know if this function has timed out.  When it times out, throw an exception so that the 
            //function completes with a failure.  This will ensure that the message is removed from apiportworkflowqueue and automatically added to apiportworkflowqueue-poison so that
            //we can treat a function timeout as an ordinary failure which requires that the end-user resubmit their submission for analysis.
            if (!cancelToken.IsCancellationRequested)
            {
                using (CancellationTokenRegistration ctr = cancelToken.Register(() => cancelAsync(workflowMessage)))
                {
                    log.LogInformation("Processing message {SubmissionId}, stage {Stage}", workflowMessage.SubmissionId, workflowMessage.Stage);

                    var workflowMgr = WorkflowManager.Initialize();
                    var nextMsg = await workflowMgr.ExecuteActionsToNextStage(workflowMessage, cancelToken).ConfigureAwait(false);

                    if (nextMsg.Stage != WorkflowStage.Finished)
                    {
                        log.LogInformation("Queueing new message {SubmissionId}, stage {Stage}", workflowMessage.SubmissionId, nextMsg.Stage);
                        workflowMessageQueue.Add(nextMsg);
                    }
                    else
                    {
                        //TODO: Add notification to client that analysis workflow has completed successfully
                    }
                }
            }
            else
            {
                log.LogError("Timeout before workflow action started.SubmissionId: {SubmissionId}, Stage: {Stage}", workflowMessage.SubmissionId, workflowMessage.Stage);
                throw new TimeoutException($"Timeout before workflow action started. SubmissionId test: {workflowMessage.SubmissionId} Stage: {workflowMessage.Stage}.");
            }
        }
    }
}
