// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Views;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ApiPortVS
{
    [Guid(Guids.ApiPortVSPkgString)]
    [InstalledProductRegistration("#110", "#112", "1.1.10808.0", IconResourceID = 400)] // Help->About info
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string)] // load when a solution is opened
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideOptionPage(typeof(OptionsPage), ".NET Portability Analyzer", "Target Platforms", 0, 0, true)] // TODO how to localize?
    public class ApiPortVSPackage : Package
    {
        private static ServiceProvider s_serviceProvider;

        private readonly AssemblyRedirects _assemblyRedirects;

        internal static IServiceProvider LocalServiceProvider { get { return s_serviceProvider; } }

        public ApiPortVSPackage() : base()
        {
            s_serviceProvider = new ServiceProvider(this);
            _assemblyRedirects = s_serviceProvider.GetService(typeof(AssemblyRedirects)) as AssemblyRedirects;

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        protected override void Dispose(bool disposing)
        {
            s_serviceProvider.Dispose();

            base.Dispose(disposing);
        }

        // Called after constructor when package is sited
        protected override void Initialize()
        {
            base.Initialize();

            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                var menuInitializer = LocalServiceProvider.GetService(typeof(AnalyzeMenu)) as AnalyzeMenu;

                CommandID menuCommandID = new CommandID(Guids.analyzeMenuItemCmdSet, (int)PkgCmdIDList.CmdIdAnalyzeMenuItem);
                MenuCommand menuItem = new MenuCommand(menuInitializer.AnalyzeMenuItemCallback, menuCommandID);
                mcs.AddCommand(menuItem);

                CommandID contextMenuCmdId = new CommandID(Guids.projectContextMenuItemCmdSet, (int)PkgCmdIDList.CmdIdProjectContextMenuItem);
                OleMenuCommand contextMenuItem = new OleMenuCommand(menuInitializer.ContextMenuItemCallback, contextMenuCmdId);
                contextMenuItem.BeforeQueryStatus += menuInitializer.ContextMenuItem_BeforeQueryStatus;
                mcs.AddCommand(contextMenuItem);
            }
        }

        /// <summary>
        /// Programmatically provides binding redirects for assemblies that cannot be resolved.
        /// </summary>
        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return _assemblyRedirects?.ResolveAssembly(args.Name, args.RequestingAssembly);
        }
    }
}