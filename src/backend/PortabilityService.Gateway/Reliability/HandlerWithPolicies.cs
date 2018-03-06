// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Polly;
using Polly.Wrap;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PortabilityService.Gateway.Reliability
{
    internal class HandlerWithPolicies : DelegatingHandler
    {
        private readonly PolicyWrap _policy;

        public HandlerWithPolicies(Policy[] policies)
        {
            _policy = Policy.WrapAsync(policies);
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return await _policy.ExecuteAsync(async ct => await base.SendAsync(request, ct), cancellationToken);
        }
    }
}
