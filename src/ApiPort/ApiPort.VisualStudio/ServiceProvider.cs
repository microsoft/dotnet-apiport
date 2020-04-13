// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Analyze;
using ApiPortVS.Contracts;
using ApiPortVS.Models;
using ApiPortVS.Reporting;
using ApiPortVS.Resources;
using ApiPortVS.SourceMapping;
using ApiPortVS.ViewModels;
using ApiPortVS.Views;
using Autofac;
using EnvDTE;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.Analyzer;
using Microsoft.Fx.Portability.Proxy;
using Microsoft.Fx.Portability.Reporting;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using static Microsoft.VisualStudio.VSConstants;

using Tasks = System.Threading.Tasks;

namespace ApiPortVS
{
    internal sealed class ServiceProvider : IDisposable, IAsyncServiceProvider, IServiceProvider
    {
        private const string DefaultEndpoint = @"https://portability.dot.net/";
        private static readonly DirectoryInfo AssemblyDirectory = new FileInfo(typeof(ServiceProvider).Assembly.Location).Directory;
        private static Guid _outputWindowGuid = new Guid(0xe2fc797f, 0x1dd3, 0x476c, 0x89, 0x17, 0x86, 0xcd, 0x31, 0x33, 0xc4, 0x69);

        private readonly IContainer _container;

        private ServiceProvider(IContainer container)
        {
            _container = container;
        }

        public static async Task<ServiceProvider> CreateAsync(ApiPortVSPackage serviceProvider)
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<VsBrowserReportViewer>()
                .As<IReportViewer>()
                .SingleInstance();
            builder.RegisterType<ToolbarListReportViewer>()
                .As<IReportViewer>()
                .InstancePerLifetimeScope();
            builder.RegisterType<ApiPortVsAnalyzer>()
                .As<IVsApiPortAnalyzer>()
                .InstancePerLifetimeScope();
            builder.RegisterType<DependencyOrderer>()
                .As<IDependencyOrderer>();

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
            builder.RegisterType<StatusBarProgressReporter>()
                .As<IProgressReporter>()
                .SingleInstance();
            builder.RegisterType<ReportFileWriter>()
                .As<IFileWriter>()
                .SingleInstance();

            builder.Register(GetOutputViewModel)
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

            // VS type registration
            // Registers all of the Visual Studio Package components.
            await RegisterVisualStudioComponentsAsync(builder, serviceProvider);

            return new ServiceProvider(builder.Build());
        }

        public void Dispose() => _container.Dispose();

        private static async Tasks.Task RegisterVisualStudioComponentsAsync(ContainerBuilder builder, ApiPortVSPackage serviceProvider)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            builder.RegisterInstance(serviceProvider)
                .As<IResultToolbar>()
                .As<IServiceProvider>();
            builder.RegisterType<Microsoft.VisualStudio.Shell.ErrorListProvider>()
                .AsSelf();
            builder.RegisterType<ErrorListProvider>()
                .As<IErrorListProvider>()
                .SingleInstance();

            builder.RegisterComInstance<SVsWebProxy, IVsWebProxy>();
            builder.RegisterComInstance<SVsWebBrowsingService, IVsWebBrowsingService>();
            builder.RegisterComInstance<SVsStatusbar, IVsStatusbar>();

            builder.RegisterCom<DTE>(await serviceProvider.GetServiceAsync(typeof(DTE)));

            var componentModel = await serviceProvider.GetServiceAsync(typeof(SComponentModel)) as IComponentModel;

            builder.RegisterInstance(componentModel.GetService<IProjectBuilder>());
            builder.RegisterInstance(componentModel.GetService<IProjectMapper>());

            var outputWindow = await serviceProvider.GetServiceAsync(typeof(SVsOutputWindow));
            builder.RegisterCom<IVsOutputWindowPane>(BuildPane((IVsOutputWindow)outputWindow));
        }

        public static IVsOutputWindowPane BuildPane(IVsOutputWindow outputWindow)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (outputWindow.GetPane(ref _outputWindowGuid, out var windowPane) == S_OK)
            {
                return windowPane;
            }

            if (outputWindow.CreatePane(ref _outputWindowGuid, LocalizedStrings.PortabilityOutputTitle, 1, 0) == S_OK)
            {
                if (outputWindow.GetPane(ref _outputWindowGuid, out windowPane) == S_OK)
                {
                    return windowPane;
                }
            }

            throw new InvalidOperationException("Could not create pane");
        }

        private static OutputViewModel GetOutputViewModel(IComponentContext context)
        {
            var viewModel = context.Resolve<OptionsViewModel>();
            var directory = new DirectoryInfo(viewModel.OutputDirectory);

            if (!directory.Exists)
            {
                return new OutputViewModel();
            }

            var validExtensions = new HashSet<string>(viewModel.Formats.Select(x => x.FileExtension).Distinct());

            var validReports = directory.EnumerateFiles().Where(x => validExtensions.Contains(x.Extension));

            // If there are no report file extensions we support,
            // or if there are no matching reports, return a new view model
            if (!validExtensions.Any() || !validReports.Any())
            {
                return new OutputViewModel();
            }

            return new OutputViewModel(validReports.Select(x => x.FullName));
        }

        Task<object> IAsyncServiceProvider.GetServiceAsync(Type serviceType) => System.Threading.Tasks.Task.FromResult<object>(_container.Resolve(serviceType));

        public object GetService(Type serviceType) => _container.Resolve(serviceType);
    }
}
