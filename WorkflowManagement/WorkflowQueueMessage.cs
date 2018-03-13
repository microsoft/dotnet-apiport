// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace WorkflowManagement
{
    public class WorkflowQueueMessage
    {
        public WorkflowStage Stage { get; set; }
        public string SubmissionId { get; set; }

    }
}
