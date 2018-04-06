// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using PortabilityService.WorkflowManagement;
using System;
using System.Threading.Tasks;
using Xunit;

namespace PortabilityService.Functions.Tests
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

            await ProcessWorkflowQueue.Run(new WorkflowQueueMessage(submissionId, WorkflowStage.Analyze), workflowQueue, NullLogger.Instance);
            workflowQueue.Received().Add(Arg.Is<WorkflowQueueMessage>(x => x.SubmissionId == submissionId && x.Stage == WorkflowStage.Report));
            workflowQueue.ClearReceivedCalls();

            await ProcessWorkflowQueue.Run(new WorkflowQueueMessage(submissionId, WorkflowStage.Report), workflowQueue, NullLogger.Instance);
            workflowQueue.Received().Add(Arg.Is<WorkflowQueueMessage>(x => x.SubmissionId == submissionId && x.Stage == WorkflowStage.Telemetry));
            workflowQueue.ClearReceivedCalls();

            await ProcessWorkflowQueue.Run(new WorkflowQueueMessage(submissionId, WorkflowStage.Telemetry), workflowQueue, NullLogger.Instance);
            workflowQueue.DidNotReceive().Add(Arg.Any<WorkflowQueueMessage>());
        }
    }
}
