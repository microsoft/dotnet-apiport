// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using WorkflowManagement;

namespace Functions
{
    public static class ProcessWorkflowQueue
    {
        [FunctionName("ProcessWorkflowQueue")]
        public static async Task Run([QueueTrigger("apiportworkflowqueue")]WorkflowQueueMessage workflowMessage,
            [Queue("apiportworkflowqueue")]ICollector<WorkflowQueueMessage> workflowMessageQueue,
            ILogger log)
        {
            log.LogInformation("Processing message {SubmissionId}, stage {Stage}", workflowMessage.SubmissionId, workflowMessage.Stage);

            var workflowMgr = WorkflowManager.Initialize();
            var nextMsg = await workflowMgr.ExecuteActionsToNextStage(workflowMessage);

            if (nextMsg.Stage != WorkflowStage.Finished)
            {
                log.LogInformation("Queueing new message {SubmissionId}, stage {Stage}", workflowMessage.SubmissionId, nextMsg.Stage);
                workflowMessageQueue.Add(nextMsg);
            }
        }
    }
}
