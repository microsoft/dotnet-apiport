// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Contracts;
using Microsoft.VisualStudio.ProjectSystem;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ApiPortVS.VS2017
{
    public class VSThreadingService : IVSThreadingService
    {
        private readonly IProjectThreadingService _threadingService;

        public VSThreadingService(IProjectThreadingService threadingService)
        {
            _threadingService = threadingService ?? throw new ArgumentNullException(nameof(threadingService));
        }

        public async Task SwitchToMainThreadAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            await _threadingService.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
        }
    }
}
