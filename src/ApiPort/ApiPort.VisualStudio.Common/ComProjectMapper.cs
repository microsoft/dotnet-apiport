// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Contracts;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

using static Microsoft.Fx.Portability.Utils.FormattableStringHelper;

namespace ApiPortVS
{
    /// <summary>
    /// You need to switch to the UI thread before calling any COM interfaces (e.g. IVsHierarchy) using
    /// </summary>
    public class COMProjectMapper : IProjectMapper
    {
        /// <summary>
        /// Gets the VSHierarchy using COM calls to the package service.
        /// </summary>
        public async Task<IVsHierarchy> GetVsHierarchyAsync(Project project)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (Package.GetGlobalService(typeof(SVsSolution)) is IVsSolution solution)
            {
                solution.GetProjectOfUniqueName(project.FullName, out var hierarchy);

                return hierarchy;
            }

            return null;
        }

        public async Task<IVsCfg> GetVsProjectConfigurationAsync(Project project)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var hierarchy = await GetVsHierarchyAsync(project).ConfigureAwait(false);

            if (hierarchy == null)
            {
                return null;
            }

            hierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_BrowseObject, out var browseObject);

            var getConfigurationProvider = browseObject as IVsGetCfgProvider;

            if (getConfigurationProvider == null)
            {
                Trace.TraceError(ToCurrentCulture($"Could not retrieve {nameof(IVsGetCfgProvider)} from project: {project.Name}"));
                return null;
            }

            if (ErrorHandler.Failed(getConfigurationProvider.GetCfgProvider(out var provider)))
            {
                Trace.TraceError(ToCurrentCulture($"Could not retrieve {nameof(IVsCfgProvider)} from project: {project.Name}"));
                return null;
            }

            if (!(provider is IVsCfgProvider2))
            {
                Trace.TraceError(ToCurrentCulture($"IVsCfgProvider returned {provider.GetType()} is not of the right type. Expected: {nameof(IVsCfgProvider2)}"));
                return null;
            }

            var provider2 = (IVsCfgProvider2)provider;
            var activeConfiguration = project.ConfigurationManager.ActiveConfiguration;

            if (ErrorHandler.Failed(provider2.GetCfgOfName(activeConfiguration.ConfigurationName, activeConfiguration.PlatformName, out var configuration)))
            {
                Trace.TraceError(ToCurrentCulture($"Could not retrieve {nameof(IVsCfg)} from project: {project.Name}"));
                return null;
            }

            return configuration;
        }
    }
}
