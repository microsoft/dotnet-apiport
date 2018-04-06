// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;

namespace PortabilityService.WorkflowManagement
{
    class ReportAction : IWorkflowAction
    {
        public async Task<WorkflowStage> ExecuteAsync(string submissionId)
        {
            // TODO: Update to call Report Generator Service
            await Task.Delay(5);

            return WorkflowStage.Telemetry;
        }

        public WorkflowStage CurrentStage
        {
            get { return WorkflowStage.Report;  }
        }
    }
}
