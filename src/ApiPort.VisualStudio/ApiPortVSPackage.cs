// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Views;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
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
    [ProvideOptionPage(typeof(OptionsPage), ".NET Portability Analyzer", "General", 110, 113, true)]
    [ProvideToolWindow(typeof(AnalysisOutputToolWindow))]
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

                CommandID anazlyMenuCommandID = new CommandID(Guids.AnalyzeMenuItemCmdSet, (int)PkgCmdIDList.CmdIdAnalyzeMenuItem);
                MenuCommand menuItem = new MenuCommand(menuInitializer.AnalyzeMenuItemCallback, anazlyMenuCommandID);
                mcs.AddCommand(menuItem);

                CommandID analyzeMenuOptionsCommandID = new CommandID(Guids.AnalyzeMenuItemCmdSet, (int)PkgCmdIDList.CmdIdAnalyzeOptionsMenuItem);
                MenuCommand analyzeMenuOptionsItem = new MenuCommand(ShowOptionsPage, analyzeMenuOptionsCommandID);
                mcs.AddCommand(analyzeMenuOptionsItem);

                CommandID analyzeMenuToolbarCommandID = new CommandID(Guids.AnalyzeMenuItemCmdSet, (int)PkgCmdIDList.CmdIdAnalyzeToolbarMenuItem);
                MenuCommand analyzeMenuToolbarItem = new MenuCommand(ShowToolbar, analyzeMenuToolbarCommandID);
                mcs.AddCommand(analyzeMenuToolbarItem);

                CommandID projectContextMenuCmdId = new CommandID(Guids.ProjectContextMenuItemCmdSet, (int)PkgCmdIDList.CmdIdProjectContextMenuItem);
                OleMenuCommand contextMenuItem = new OleMenuCommand(menuInitializer.ContextMenuItemCallback, projectContextMenuCmdId);
                contextMenuItem.BeforeQueryStatus += menuInitializer.ProjectContextMenuItemBeforeQueryStatus;
                mcs.AddCommand(contextMenuItem);

                CommandID projectContextMenuOptionsCmdId = new CommandID(Guids.ProjectContextMenuItemCmdSet, (int)PkgCmdIDList.CmdIdProjectContextOptionsMenuItem);
                OleMenuCommand contextMenuOptionsItem = new OleMenuCommand(ShowOptionsPage, projectContextMenuOptionsCmdId);
                contextMenuOptionsItem.BeforeQueryStatus += menuInitializer.ProjectContextMenuItemBeforeQueryStatus;
                mcs.AddCommand(contextMenuOptionsItem);
            }
        }

        private void ShowToolbar(object sender, EventArgs e)
        {
            ToolWindowPane window = FindToolWindow(typeof(AnalysisOutputToolWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException("Cannot create tool window");
            }

            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        private void ShowOptionsPage(object sender, EventArgs e)
        {
            ShowOptionPage(typeof(OptionsPage));
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