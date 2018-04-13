// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace PortabilityService.WorkflowManagement
{
    public class WorkflowQueueMessage
    {
        public WorkflowStage Stage { get; }

        public string SubmissionId { get; }

        public WorkflowQueueMessage(string submissionId, WorkflowStage stage)
        {
            Stage = stage;
            SubmissionId = submissionId;
        }
    }
}
