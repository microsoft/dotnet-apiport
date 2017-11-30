// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Analyze;
using ApiPortVS.Contracts;
using ApiPortVS.Models;
using ApiPortVS.Resources;
using ApiPortVS.Reporting;
using ApiPortVS.SourceMapping;
using ApiPortVS.ViewModels;
using ApiPortVS.Views;
using Autofac;
using EnvDTE;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.Analyzer;
using Microsoft.Fx.Portability.Proxy;
using Microsoft.Fx.Portability.Reporting;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.IO;

using static Microsoft.VisualStudio.VSConstants;

namespace ApiPortVS
{
    internal sealed class ServiceProvider : IDisposable, IServiceProvider
    {
        private static Guid OutputWindowGuid = new Guid(0xe2fc797f, 0x1dd3, 0x476c, 0x89, 0x17, 0x86, 0xcd, 0x31, 0x33, 0xc4, 0x69);
        private static readonly DirectoryInfo AssemblyDirectory = new FileInfo(typeof(ServiceProvider).Assembly.Location).Directory;
        private const string DefaultEndpoint = @"https://portability.dot.net/";

        private readonly IContainer _container;

        public ServiceProvider(ApiPortVSPackage serviceProvider)
        {
            var builder = new ContainerBuilder();

            // VS type registration
            // Registers all of the Visual Studio Package components.
            RegisterVisualStudioComponents(builder, serviceProvider);

            builder.RegisterType<VsBrowserReportViewer>()
                .As<IReportViewer>()
                .SingleInstance();
            builder.RegisterType<ToolbarListReportViewer>()
                .As<IReportViewer>()
                .InstancePerLifetimeScope();
            builder.RegisterType<ApiPortVsAnalyzer>()
                .As<IVsApiPortAnalyzer>()
                .InstancePerLifetimeScope();

            // Service registration
            builder.RegisterInstance(new ProductInformation("ApiPort_VS"))
                .AsSelf();
            builder.RegisterInstance(new AssemblyRedirectResolver(AssemblyDirectory))
                .AsSelf();
            builder.RegisterType<VisualStudioProxyProvider>()
                .As<IProxyProvider>()
                .SingleInstance();
            builder.RegisterType<ApiPortService>()
                .As<IApiPortService>()
                .WithParameter(TypedParameter.From(DefaultEndpoint))
                .SingleInstance();
            builder.RegisterType<ApiPortClient>()
                .AsSelf()
                .SingleInstance();
            builder.Register(_ => OptionsModel.Load())
                .As<OptionsModel>()
                .OnRelease(m => m.Save())
                .SingleInstance();
            builder.RegisterType<TargetMapper>()
                .As<ITargetMapper>()
                .OnActivated(h => h.Instance.LoadFromConfig())
                .InstancePerLifetimeScope();
            builder.RegisterType<WindowsFileSystem>()
                .As<IFileSystem>()
                .SingleInstance();

            // Register output services
            builder.RegisterType<ReportGenerator>()
                .As<IReportGenerator>()
                .SingleInstance();
            builder.RegisterType<OutputWindowWriter>()
                .AsSelf()
                .As<IOutputWindowWriter>()
                .As<TextWriter>()
                .SingleInstance();
            builder.RegisterType<TextWriterProgressReporter>()
                .As<IProgressReporter>()
                .SingleInstance();
            builder.RegisterType<ReportFileWriter>()
                .As<IFileWriter>()
                .SingleInstance();

            builder.RegisterInstance(AnalysisOutputToolWindowControl.Model)
                .As<OutputViewModel>()
                .SingleInstance();

            // Register menu handlers
            builder.RegisterType<AnalyzeMenu>()
                .AsSelf()
                .SingleInstance();
            builder.RegisterType<FileListAnalyzer>()
                .AsSelf()
                .InstancePerLifetimeScope();
            builder.RegisterType<ProjectAnalyzer>()
                .AsSelf()
                .InstancePerLifetimeScope();
            builder.RegisterType<COMProjectMapper>()
                .As<IProjectMapper>()
                .SingleInstance();

            // Register option pane services
            builder.RegisterType<OptionsPageControl>()
                .AsSelf()
                .InstancePerLifetimeScope();
            builder.RegisterType<OptionsViewModel>()
              .AsSelf()
              .InstancePerLifetimeScope();

            // Metadata manipulation registrations
            builder.RegisterType<CciDependencyFinder>()
                .As<IDependencyFinder>()
                .InstancePerLifetimeScope();
            builder.RegisterType<CciSourceLineMapper>()
                .As<ISourceLineMapper>()
                .InstancePerLifetimeScope();

            var dte = Package.GetGlobalService(typeof(DTE)) as DTE;
            var version = new Version(dte.Version);

            if (version.Major == 14)
            {
                builder.RegisterModule(new VS2015.ServiceProvider());
            }
            else
            {
                builder.RegisterModule(new VS2017.ServiceProvider());
            }

            _container = builder.Build();
        }

        public object GetService(Type serviceType)
        {
            return _container.Resolve(serviceType);
        }

        public void Dispose()
        {
            _container.Dispose();
        }

        private void RegisterVisualStudioComponents(ContainerBuilder builder, ApiPortVSPackage serviceProvider)
        {
            builder.RegisterInstance(serviceProvider)
                .As<IResultToolbar>()
                .As<IServiceProvider>();
            builder.RegisterType<Microsoft.VisualStudio.Shell.ErrorListProvider>()
                .AsSelf();
            builder.RegisterType<ErrorListProvider>()
                .As<IErrorListProvider>()
                .SingleInstance();
            builder.Register(_ => Package.GetGlobalService(typeof(SVsWebProxy)))
                .As<IVsWebProxy>();
            builder.Register(_ => Package.GetGlobalService(typeof(SVsWebBrowsingService)))
                .As<IVsWebBrowsingService>();
            builder.Register(_ => Package.GetGlobalService(typeof(SVsSolutionBuildManager)))
                .As<IVsSolutionBuildManager2>();
            builder.RegisterType<DefaultProjectBuilder>()
                .As<IProjectBuilder>();

            builder.RegisterAdapter<IServiceProvider, DTE>(provider => (DTE)provider.GetService(typeof(DTE)));
            builder.RegisterAdapter<IServiceProvider, IVsOutputWindowPane>(provider =>
            {
                var outputWindow = provider.GetService(typeof(SVsOutputWindow)) as IVsOutputWindow;

                if (outputWindow.GetPane(ref OutputWindowGuid, out IVsOutputWindowPane windowPane) == S_OK)
                {
                    return windowPane;
                }

                if (outputWindow.CreatePane(ref OutputWindowGuid, LocalizedStrings.PortabilityOutputTitle, 1, 0) == S_OK)
                {
                    if (outputWindow.GetPane(ref OutputWindowGuid, out windowPane) == S_OK)
                    {
                        return windowPane;
                    }
                }

                // If a custom window couldn't be opened, open the general purpose window
                return provider.GetService(typeof(SVsGeneralOutputWindowPane)) as IVsOutputWindowPane;
            }).SingleInstance();
        }
    }
}
