// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Contracts;
using Microsoft.VisualStudio.ProjectSystem;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace ApiPortVS.VS2015
{
    public class VSThreadingService : IVSThreadingService
    {
        private readonly IThreadHandling _threadHandling;

        public VSThreadingService(IThreadHandling threadHandling)
        {
            _threadHandling = threadHandling;
        }

        public async Task SwitchToMainThreadAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            await _threadHandling.AsyncPump.SwitchToMainThreadAsync(cancellationToken);
        }
    }
}
