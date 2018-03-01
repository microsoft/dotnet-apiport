// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;
using System.Threading.Tasks;

namespace ApiPortVS.Contracts
{
    /// <summary>
    /// Maps a <see cref="Project"/> to its associated COM types in Visual Studio
    /// </summary>
    public interface IProjectMapper
    {
        Task<IVsHierarchy> GetVsHierarchyAsync(Project project);

        Task<IVsCfg> GetVsProjectConfigurationAsync(Project project);
    }
}
