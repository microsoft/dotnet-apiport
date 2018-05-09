// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using PortabilityService.WorkflowManagement;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PortabilityService.Functions.Tests
{
    public class ProcessWorkflowQueueTests
    {
        /// <remarks>Because WorkflowManager only initializes once and returns
        /// the singleton instance, it won't work to initialize it with
        /// different set of workflow actiosn in different tests.
        /// Thus mocking is done before tests are executed.
        /// </remarks>
        public ProcessWorkflowQueueTests()
        {
            var workflowActions = new IWorkflowAction[4];

            var analyzeAction = Substitute.For<IWorkflowAction>();
            analyzeAction.CurrentStage.Returns(WorkflowStage.Analyze);
            analyzeAction.ExecuteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(WorkflowStage.Report));

            var reportAction = Substitute.For<IWorkflowAction>();
            reportAction.CurrentStage.Returns(WorkflowStage.Report);
            reportAction.ExecuteAsync(Arg.Any<string>(), CancellationToken.None).Returns(Task.FromResult(WorkflowStage.Telemetry));

            var telemetryAction = Substitute.For<IWorkflowAction>();
            telemetryAction.CurrentStage.Returns(WorkflowStage.Telemetry);
            telemetryAction.ExecuteAsync(Arg.Any<string>(), CancellationToken.None).Returns(Task.FromResult(WorkflowStage.Finished));

            var finishedAction = Substitute.For<IWorkflowAction>();

            workflowActions[(int)WorkflowStage.Analyze] = analyzeAction;
            workflowActions[(int)WorkflowStage.Report] = reportAction;
            workflowActions[(int)WorkflowStage.Telemetry] = telemetryAction;
            workflowActions[(int)WorkflowStage.Finished] = finishedAction;

            WorkflowManager.Initialize(workflowActions);
        }

        [Fact]
        public async Task ProcessQueueMessageStages()
        {
            var submissionId = new Guid().ToString();

            var workflowQueue = Substitute.For<ICollector<WorkflowQueueMessage>>();

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
        public async Task CancellationBeforeActionExecuted()
        {
            var submissionId = new Guid().ToString();

            var workflowQueue = Substitute.For<ICollector<WorkflowQueueMessage>>();

            var cancelTokenSrc = new CancellationTokenSource();
            cancelTokenSrc.Cancel();

            var ex = await Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                await ProcessWorkflowQueue.Run(new WorkflowQueueMessage(submissionId, WorkflowStage.Analyze), workflowQueue, NullLogger.Instance, cancelTokenSrc.Token);
            });

            Assert.Equal($"Timeout before workflow action started. SubmissionId test: {submissionId} Stage: {WorkflowStage.Analyze}.", ex.Message);
        }
    }
}
