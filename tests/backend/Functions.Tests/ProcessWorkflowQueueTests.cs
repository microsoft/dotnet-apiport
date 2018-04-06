// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using NSubstitute;
using WorkflowManagement;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Azure.WebJobs;

namespace Functions.Tests
{
    public class ProcessWorkflowQueueTests
    {
        [Fact]
        public static async Task ProcessQueueMessageStages()
        {
            IWorkflowAction[] workflowActions = new IWorkflowAction[3];
            workflowActions[(int)WorkflowStage.Analyze] = Substitute.For<IWorkflowAction>();
            workflowActions[(int)WorkflowStage.Report] = Substitute.For<IWorkflowAction>();
            workflowActions[(int)WorkflowStage.Telemetry] = Substitute.For<IWorkflowAction>();

            WorkflowManager.Initialize();
            var workflowQueue = Substitute.For<ICollector<WorkflowQueueMessage>>();
            var submissionId = new Guid().ToString();

            await ProcessWorkflowQueue.Run(new WorkflowQueueMessage(submissionId, WorkflowStage.Analyze), workflowQueue, NullLogger.Instance, CancellationToken.None);
            workflowQueue.Received().Add(Arg.Is<WorkflowQueueMessage>(x => x.SubmissionId == submissionId && x.Stage == WorkflowStage.Report));
            workflowQueue.ClearReceivedCalls();

            await ProcessWorkflowQueue.Run(new WorkflowQueueMessage(submissionId, WorkflowStage.Report), workflowQueue, NullLogger.Instance, CancellationToken.None);
            workflowQueue.Received().Add(Arg.Is<WorkflowQueueMessage>(x => x.SubmissionId == submissionId && x.Stage == WorkflowStage.Telemetry));
            workflowQueue.ClearReceivedCalls();

            await ProcessWorkflowQueue.Run(new WorkflowQueueMessage(submissionId, WorkflowStage.Telemetry), workflowQueue, NullLogger.Instance, CancellationToken.None);
            workflowQueue.DidNotReceive().Add(Arg.Any<WorkflowQueueMessage>());
        }

        [Fact]
        public static async Task CancellationBeforeActionExecuted()
        {
            IWorkflowAction[] workflowActions = new IWorkflowAction[3];
            workflowActions[(int)WorkflowStage.Analyze] = Substitute.For<IWorkflowAction>();
            workflowActions[(int)WorkflowStage.Report] = Substitute.For<IWorkflowAction>();
            workflowActions[(int)WorkflowStage.Telemetry] = Substitute.For<IWorkflowAction>();

            WorkflowManager.Initialize();
            var workflowQueue = Substitute.For<ICollector<WorkflowQueueMessage>>();
            var submissionId = new Guid().ToString();

            var cancelTokenSrc = new CancellationTokenSource();
            cancelTokenSrc.Cancel();

            var ex = await Assert.ThrowsAsync<TimeoutException>(async () => await ProcessWorkflowQueue.Run(new WorkflowQueueMessage(submissionId, WorkflowStage.Analyze), workflowQueue, NullLogger.Instance, cancelTokenSrc.Token));
            Assert.Equal($"Timeout before workflow action started. SubmissionId test: {submissionId} Stage: {WorkflowStage.Analyze}.", ex.Message);
        }
    }
}
