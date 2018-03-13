// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace WorkflowManagement
{
    internal class MockActionFactory : IWorkflowActionFactory
    {
        public IWorkflowAction[] CreateActions()
        {
            IWorkflowAction[] actions = new IWorkflowAction[3];

            actions[(int)WorkflowStage.Analyze] = new MockAction();
            actions[(int)WorkflowStage.Report] = new MockAction();
            actions[(int)WorkflowStage.Telemetry] = new MockAction();

            return actions;
        }
    }
}
