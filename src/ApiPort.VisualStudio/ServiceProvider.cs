// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Analyze;
using ApiPortVS.Contracts;
using ApiPortVS.Models;
using ApiPortVS.Reporting;
using ApiPortVS.SourceMapping;
using ApiPortVS.ViewModels;
using ApiPortVS.Views;
using Autofac;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.Analyzer;
using Microsoft.Fx.Portability.Reporting;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.IO;
using System.Reflection;

namespace ApiPortVS
{
    internal sealed class ServiceProvider : IDisposable, IServiceProvider
    {
        private const string DefaultEndpoint = @"https://portability.dot.net/";
        private const string AppConfig = "app.config";

        private static readonly string s_appConfigFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), AppConfig);

        private readonly IContainer _container;

        public ServiceProvider(IServiceProvider serviceProvider)
        {
            var builder = new ContainerBuilder();

            // VS type registration
            builder.RegisterType<ErrorListProvider>()
                .AsSelf()
                .SingleInstance();
            builder.RegisterInstance<IServiceProvider>(serviceProvider)
                .As<IServiceProvider>();
            builder.Register(_ => Package.GetGlobalService(typeof(SVsWebBrowsingService)))
                .As<IVsWebBrowsingService>();
            builder.RegisterType<VsBrowserReportViewer>()
                .As<IReportViewer>()
                .SingleInstance();
            builder.Register(x => new AssemblyRedirects(s_appConfigFilePath))
                .AsSelf()
                .SingleInstance()
                .AutoActivate();

            // Service registration
            builder.RegisterInstance(new ProductInformation("ApiPort_VS"))
                .AsSelf();
            builder.RegisterType<ApiPortService>().
                As<IApiPortService>().
                WithParameter(TypedParameter.From<string>(DefaultEndpoint))
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
            builder.Register(CreateOutputTextWriter)
                .As<TextWriter>()
                .SingleInstance();
            builder.RegisterType<TextWriterProgressReporter>()
                .As<IProgressReporter>()
                .SingleInstance();
            builder.RegisterType<ReportFileWriter>()
                .As<IFileWriter>()
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

        private TextWriter CreateOutputTextWriter(IComponentContext arg)
        {
            var serviceProvider = arg.Resolve<IServiceProvider>();
            var windowPane = serviceProvider.GetService(typeof(SVsGeneralOutputWindowPane)) as IVsOutputWindowPane;

            return windowPane == null ? TextWriter.Null : new OutputWindowWriter(windowPane);
        }
    }
}
