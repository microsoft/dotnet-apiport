// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;

namespace WorkflowManagement
{
    class ReportAction : IWorkflowAction
    {
        public async Task Execute(string submissionId)
        {
            // TODO: Update to call Report Generator Service
            await Task.Delay(5);
        }
    }
}
