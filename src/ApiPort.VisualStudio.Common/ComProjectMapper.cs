// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Contracts;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using VisualStudio = Microsoft.VisualStudio.Shell;

namespace ApiPortVS
{
    /// <summary>
    /// You need to switch to the UI thread before calling any COM interfaces (e.g. IVsHierarchy) using
    /// </summary>
    public class COMProjectMapper : IProjectMapper
    {
        private readonly IVSThreadingService _threadingService;

        public COMProjectMapper(IVSThreadingService threadingService)
        {
            _threadingService = threadingService;
        }

        /// <summary>
        /// Gets the VSHierarchy using COM calls to the package service.
        /// </summary>
        public async Task<IVsHierarchy> GetVsHierarchyAsync(Project project)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            await _threadingService.SwitchToMainThreadAsync();

            var solution = VisualStudio.Package.GetGlobalService(typeof(SVsSolution)) as IVsSolution;
            if (solution == null)
            {
                return null;
            }

            IVsHierarchy hierarchy;
            solution.GetProjectOfUniqueName(project.FullName, out hierarchy);

            return hierarchy;
        }

        public async Task<IVsCfg> GetVsProjectConfigurationAsync(Project project)
        {
            object browseObject = null;

            var hierarchy = await GetVsHierarchyAsync(project).ConfigureAwait(false);

            if (hierarchy == null)
            {
                return null;
            }

            await _threadingService.SwitchToMainThreadAsync();

            hierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_BrowseObject, out browseObject);

            var getConfigurationProvider = browseObject as IVsGetCfgProvider;

            if (getConfigurationProvider == null)
            {
                Trace.TraceError($"Could not retrieve {nameof(IVsGetCfgProvider)} from project: {project.Name}");
                return null;
            }

            if (ErrorHandler.Failed(getConfigurationProvider.GetCfgProvider(out IVsCfgProvider provider)))
            {
                Trace.TraceError($"Could not retrieve {nameof(IVsCfgProvider)} from project: {project.Name}");
                return null;
            }
            if (!(provider is IVsCfgProvider2))
            {
                Trace.TraceError($"IVsCfgProvider returned {provider.GetType()} is not of the right type. Expected: {nameof(IVsCfgProvider2)}");
                return null;
            }

            var provider2 = (IVsCfgProvider2)provider;
            var activeConfiguration = project.ConfigurationManager.ActiveConfiguration;

            if (ErrorHandler.Failed(provider2.GetCfgOfName(activeConfiguration.ConfigurationName, activeConfiguration.PlatformName, out IVsCfg configuration)))
            {
                Trace.TraceError($"Could not retrieve {nameof(IVsCfg)} from project: {project.Name}");
                return null;
            }

            return configuration;
        }
    }
}
