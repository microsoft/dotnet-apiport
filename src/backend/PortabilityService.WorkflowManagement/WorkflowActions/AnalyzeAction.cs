// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PortabilityService.WorkflowManagement
{
    internal class AnalyzeAction : BaseAction, IWorkflowAction
    {
        public AnalyzeAction(string serviceUrl) : base(serviceUrl) { }

        public async Task<WorkflowStage> ExecuteAsync(string submissionId, CancellationToken token)
        {
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("submissionId", submissionId)
            });

            var response = await httpClient.PostAsync(ServiceUrl, formContent).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            return WorkflowStage.Telemetry;
        }

        public WorkflowStage CurrentStage
        {
            get { return WorkflowStage.Analyze; }
        }
    }
}
