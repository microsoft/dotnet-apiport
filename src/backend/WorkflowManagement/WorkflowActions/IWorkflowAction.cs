// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;

namespace WorkflowManagement
{
    public interface IWorkflowAction
    {
        /// <summary>
        /// Returns the next stage after executing the workflow action.
        Task<WorkflowStage> ExecuteAsync(string submissionId);

        /// <summary>
        /// Gets the current stage.
        /// </summary>
        WorkflowStage CurrentStage { get; }
    }
}
