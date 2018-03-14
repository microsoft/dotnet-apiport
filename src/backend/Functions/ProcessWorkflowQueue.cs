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
        //Allows WorkflowManager to be "injected" as needed, such as with a mock action factory when the function is called through tests
        public static Func<WorkflowManager> GetWorkflowManager { get; set; } = () => new WorkflowManager();

        [FunctionName("ProcessWorkflowQueue")]
        public static async Task Run([QueueTrigger("apiportworkflowqueue")]WorkflowQueueMessage workflowMessage, 
            [Queue("apiportworkflowqueue")]ICollector<WorkflowQueueMessage> workflowMessageQueue, ILogger log)
        {
            log.LogInformation($"processing message {workflowMessage.SubmissionId}, stage {workflowMessage.Stage}");

            var workflowMgr = GetWorkflowManager();
            var nextMsg = await workflowMgr.ExecuteActionsToNextStage(workflowMessage);

            log.LogInformation($"queueing new message {workflowMessage.SubmissionId}, stage {nextMsg.Stage}");

            if (nextMsg.Stage != WorkflowStage.Finished)
            {
                workflowMessageQueue.Add(nextMsg);
            }
        }
    }
}
