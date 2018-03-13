// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Xunit;
using Functions.Tests.Mock;
using WorkflowManagement;
using Microsoft.Extensions.Logging.Abstractions;

namespace Functions.Tests
{
    public class ProcessWorkflowQueueTests
    {
        [Fact]
        public static async Task ProcessQueueMessageStages()
        {
            ProcessWorkflowQueue.GetActionFactory = () => new MockActionFactory();
            var workflowQueue = new MockCollector<WorkflowQueueMessage>();
            var submissionId = new Guid().ToString();

            await ProcessWorkflowQueue.Run(new WorkflowQueueMessage() { SubmissionId = submissionId, Stage = WorkflowStage.Analyze }, workflowQueue, NullLogger.Instance);

            Assert.Single(workflowQueue.Items);
            var msg = (WorkflowQueueMessage)workflowQueue.Items[0];
            Assert.Equal(submissionId, msg.SubmissionId);
            Assert.Equal(WorkflowStage.Report, msg.Stage);

            workflowQueue.Items.Clear();
            await ProcessWorkflowQueue.Run(new WorkflowQueueMessage() { SubmissionId = submissionId, Stage = WorkflowStage.Report }, workflowQueue, NullLogger.Instance);

            Assert.Single(workflowQueue.Items);
            msg = (WorkflowQueueMessage)workflowQueue.Items[0];
            Assert.Equal(submissionId, msg.SubmissionId);
            Assert.Equal(WorkflowStage.Telemetry, msg.Stage);

            workflowQueue.Items.Clear();
            await ProcessWorkflowQueue.Run(new WorkflowQueueMessage() { SubmissionId = submissionId, Stage = WorkflowStage.Telemetry }, workflowQueue, NullLogger.Instance);

            Assert.Empty(workflowQueue.Items);
        }
    }
}
