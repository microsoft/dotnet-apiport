// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using WorkflowManagement;

namespace Functions
{
    public static class ProcessWorkflowPoisonQueue
    {
        [FunctionName("ProcessWorkflowPoisonQueue")]
        public static void Run([QueueTrigger("apiportworkflowqueue-poison")]WorkflowQueueMessage workflowMessage, 
            ILogger log)
        {
            //TODO: Add notification to client when a failure occurs; when any type of failure occurs, the user is expected to resubmit their submission for analysis.

            log.LogError("Workflow message failure {SubmissionId}, stage {Stage}", workflowMessage.SubmissionId, workflowMessage.Stage);
        }
    }
}
