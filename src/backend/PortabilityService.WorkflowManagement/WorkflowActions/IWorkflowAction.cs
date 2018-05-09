// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;

namespace PortabilityService.WorkflowManagement
{
    public interface IWorkflowAction
    {
        /// <summary>
        /// Returns the next stage after executing the workflow action.
        Task<WorkflowStage> ExecuteAsync(string submissionId, CancellationToken cancelToken);

        /// <summary>
        /// Gets the current stage.
        /// </summary>
        WorkflowStage CurrentStage { get; }

        /// <summary>
        /// Gets the url of service that a workflow action invokes.
        /// </summary>
        string ServiceUrl { get; }
    }
}
