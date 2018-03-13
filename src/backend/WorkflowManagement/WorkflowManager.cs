// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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

        public static WorkflowManager GetInstance(IWorkflowActionFactory actionFactory)
        {
            if (manager == null)
            {
                manager = new WorkflowManager(actionFactory.CreateActions());
            }

            return manager;
        }

        private WorkflowManager(IWorkflowAction[] workflowActions)
        {
            actions = workflowActions;
        }

        /// <summary>
        /// Gets the first message used to start a workflow for a submission.
        /// </summary>
        public WorkflowQueueMessage GetFirstStage(string submissionId)
        {
            return new WorkflowQueueMessage() { SubmissionId = submissionId, Stage = WorkflowStage.Analyze };
        }

        /// <summary>
        /// Executes the action for the current stage in the workflow. After the action completes,
        /// returns a message for the next stage in the queue. 
        /// </summary>
        public async Task<WorkflowQueueMessage> ExecuteActionsToNextStage(WorkflowQueueMessage currentMsg)
        {
            //Execute the action for the current stage
            await actions[(int)currentMsg.Stage].Execute(currentMsg.SubmissionId);

            //Determine which stage in the workflow the submission moves to next
            WorkflowStage nextStage;
            switch (currentMsg.Stage)
            {
                case WorkflowStage.Analyze:
                    nextStage = WorkflowStage.Report;
                    break;
                case WorkflowStage.Report:
                    nextStage = WorkflowStage.Telemetry;
                    break;
                default:
                    nextStage = WorkflowStage.Finished;
                    break;
            }

            return new WorkflowQueueMessage() { SubmissionId = currentMsg.SubmissionId, Stage = nextStage };
        }
    }
}
