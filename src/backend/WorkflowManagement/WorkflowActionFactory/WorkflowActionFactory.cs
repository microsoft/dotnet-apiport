// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace WorkflowManagement
{
    public class WorkflowActionFactory : IWorkflowActionFactory
    {
        public IWorkflowAction[] CreateActions()
        {
            IWorkflowAction[] actions = new IWorkflowAction[3];

            actions[(int)WorkflowStage.Analyze] = new AnalyzeAction();
            actions[(int)WorkflowStage.Report] = new ReportAction();
            actions[(int)WorkflowStage.Telemetry] = new TelemetryAction();

            return actions;
        }
    }
}
