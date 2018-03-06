// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ApiPortVS.Contracts
{
    public interface IProjectBuilder
    {
        Task<bool> BuildAsync(IEnumerable<Project> projects);

        Task<IEnumerable<string>> GetBuildOutputFilesAsync(Project project, CancellationToken cancellationToken = default(CancellationToken));
    }
}
