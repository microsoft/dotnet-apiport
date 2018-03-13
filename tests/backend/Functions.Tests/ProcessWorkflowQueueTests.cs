// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging.Abstractions;
using WorkflowManagement;
using Functions.Tests.Mock;

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
            Assert.Equal(WorkflowStage.Report, ((WorkflowQueueMessage)workflowQueue.Items[0]).Stage);

            workflowQueue.Items.Clear();
            await ProcessWorkflowQueue.Run(new WorkflowQueueMessage() { SubmissionId = submissionId, Stage = WorkflowStage.Report }, workflowQueue, NullLogger.Instance);

            Assert.Single(workflowQueue.Items);
            msg = (WorkflowQueueMessage)workflowQueue.Items[0];
            Assert.Equal(submissionId, msg.SubmissionId);
            Assert.Equal(WorkflowStage.Telemetry, msg.Stage);

            workflowQueue.Items.Clear();
            await ProcessWorkflowQueue.Run(new WorkflowQueueMessage() { SubmissionId = submissionId, Stage = WorkflowStage.Telemetry }, workflowQueue, NullLogger.Instance);

            Assert.Single(workflowQueue.Items);
            msg = (WorkflowQueueMessage)workflowQueue.Items[0];
            Assert.Equal(submissionId, msg.SubmissionId);
            Assert.Equal(WorkflowStage.Finished, msg.Stage);
        }
    }
}
