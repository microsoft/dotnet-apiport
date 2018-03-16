// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;

namespace WorkflowManagement
{
    public enum WorkflowStage
    {
        Analyze,
        Report,
        Telemetry,
        Finished
    }

    /// <summary>
    /// Workflow stages are in the following order: Analyze -> Report -> Telemetry -> Finished
    /// </summary>
    public class WorkflowManager
    {
        readonly IWorkflowAction[] actions;
        static WorkflowManager manager;

        /// <returns></returns>
        public static WorkflowManager Initialize(IWorkflowAction[] workflowActions)
        {
            if (manager == null)
            {
                manager = new WorkflowManager(workflowActions);
            }

            return manager;
        }

        public static WorkflowManager Initialize()
        {
            if (manager == null)
            {
                manager = new WorkflowManager();
            }

            return manager;
        }

        private WorkflowManager()
        {
            actions = new IWorkflowAction[Enum.GetValues(typeof(WorkflowStage)).Length-1];

            AddAction<AnalyzeAction>();
            AddAction<ReportAction>();
            AddAction<TelemetryAction>();
        }

        private WorkflowManager(IWorkflowAction[] workflowActions)
        {
            actions = workflowActions;
        }

        private void AddAction<T>() where T : IWorkflowAction, new()
        {
            T action = new T();
            actions[(int)action.CurrentStage] = action;
        }

        /// <summary>
        /// Gets the first message used to start a workflow for a submission.
        /// </summary>
        public static WorkflowQueueMessage GetFirstStage(string submissionId)
        {
            return new WorkflowQueueMessage(submissionId, WorkflowStage.Analyze);
        }

        /// <summary>
        /// Executes the action for the current stage in the workflow. After the action completes,
        /// returns a message for the next stage in the queue. 
        /// </summary>
        public async Task<WorkflowQueueMessage> ExecuteActionsToNextStage(WorkflowQueueMessage currentMsg)
        {
            //Execute the action
            WorkflowStage nextStage = await actions[(int)currentMsg.Stage].ExecuteAsync(currentMsg.SubmissionId);

            return new WorkflowQueueMessage(currentMsg.SubmissionId, nextStage);
        }
    }
}
