// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.VisualStudio.Shell.Interop;

namespace ApiPortVS
{
    public interface IBuildServices
    {
        IVsSolutionBuildManager2 BuildManager { get; }
    }
}
