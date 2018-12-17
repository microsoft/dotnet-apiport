// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.ComponentModel.Composition;

namespace ApiPortVS
{
    [Export(typeof(IBuildServices))]
    public class BuildServices : IBuildServices
    {
        public BuildServices()
        {
            BuildManager = (IVsSolutionBuildManager2)Package.GetGlobalService(typeof(SVsSolutionBuildManager));
        }

        public IVsSolutionBuildManager2 BuildManager { get; }
    }
}
