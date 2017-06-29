// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Autofac;
using Autofac.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.Analysis;
using Microsoft.Fx.Portability.Analyzer;
using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Proxy;
using Microsoft.Fx.Portability.Reporting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ApiPort
{
    internal static class DependencyBuilder
    {
        internal const string AutofacConfiguration = "autofac.json";
        internal const string ConfigurationFile = "config.json";
        internal const string DefaultOutputFormatInstanceName = "DefaultOutputFormat";

        public static IContainer Build(ICommandLineOptions options, ProductInformation productInformation)
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<TargetMapper>()
                .As<ITargetMapper>()
                .OnActivated(c => c.Instance.LoadFromConfig(options.TargetMapFile))
                .SingleInstance();

            builder.RegisterInstance<ProductInformation>(productInformation);
            builder.RegisterInstance<ICommandLineOptions>(options);

            builder.RegisterType<ConsoleCredentialProvider>()
                .As<ICredentialProvider>()
                .SingleInstance();
            builder.Register(context => {
                    var directory = Path.GetDirectoryName(typeof(Program).GetTypeInfo().Assembly.Location);

                    return new ProxyProvider(
                        directory,
                        ConfigurationFile,
                        context.Resolve<ICredentialProvider>());
                 })
                .As<IProxyProvider>()
                .SingleInstance();

#if DEBUG_SERVICE
            // For debug purposes, the FileOutputApiPortService helps as it serializes the request to json and opens it with the
            // default json handler. To use this service, uncomment the the next line and comment the one after that.
            container.RegisterType<FileOutputApiPortService>()
                .As<IApiPortService>()
                .SingleInstance();
#else
            builder.Register(context =>
                new ApiPortService(
                        context.Resolve<ICommandLineOptions>().ServiceEndpoint,
                        context.Resolve<ProductInformation>(),
                        context.Resolve<IProxyProvider>()))
                .As<IApiPortService>()
                .SingleInstance();
#endif

            builder.RegisterType<FileIgnoreAssemblyInfoList>()
                .As<IEnumerable<IgnoreAssemblyInfo>>()
                .SingleInstance();

            builder.RegisterType<ReflectionMetadataDependencyFinder>()
                .As<IDependencyFinder>()
                .SingleInstance();

            builder.RegisterType<DotNetFrameworkFilter>()
                .As<IDependencyFilter>()
                .SingleInstance();

            builder.RegisterType<ReportGenerator>()
                .As<IReportGenerator>()
                .SingleInstance();

            builder.RegisterType<ApiPortClient>()
                .SingleInstance();

            builder.RegisterType<ApiPortService>()
                .SingleInstance();

            builder.RegisterType<WindowsFileSystem>()
                .As<IFileSystem>()
                .SingleInstance();

            builder.RegisterType<ReportFileWriter>()
                .As<IFileWriter>()
                .SingleInstance();

            builder.RegisterType<RequestAnalyzer>()
                .As<IRequestAnalyzer>()
                .SingleInstance();

            builder.RegisterType<AnalysisEngine>()
                .As<IAnalysisEngine>()
                .SingleInstance();

            builder.RegisterType<ConsoleApiPort>()
                .SingleInstance();

            builder.RegisterAdapter<ICommandLineOptions, IApiPortOptions>((ctx, opts) =>
            {
                if (opts.OutputFormats?.Any() == true)
                {
                    return opts;
                }

                return new ReadWriteApiPortOptions(opts);
            })
            .SingleInstance();

            builder.RegisterType<DocIdSearchRepl>();

            builder.RegisterType<ApiPortServiceSearcher>()
                .As<ISearcher<string>>()
                .SingleInstance();

            if (Console.IsOutputRedirected)
            {
                builder.RegisterInstance<IProgressReporter>(new TextWriterProgressReporter(Console.Out));
            }
            else
            {
                builder.RegisterType<ConsoleProgressReporter>()
                    .As<IProgressReporter>()
                    .SingleInstance();
            }


            TryLoadOffline(builder);
            TryLoadAutofacConfiguration(builder);

            return builder.Build();
        }

        private static void TryLoadAutofacConfiguration(ContainerBuilder builder)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(GetApplicationDirectory())
                .AddJsonFile(AutofacConfiguration, optional: true)
                .Build();

            if (!configuration.GetChildren().Any())
            {
                return;
            }

            var module = new ConfigurationModule(configuration);

            builder.RegisterModule(module);
        }

        private static void TryLoadOffline(ContainerBuilder builder)
        {
            try
            {
                var offlineAssembly = new AssemblyName("Microsoft.Fx.Portability.Offline");
                var assembly = Assembly.Load(offlineAssembly);
                var type = assembly.GetType("Microsoft.Fx.Portability.OfflineDataModule");
                var offlineModule = type?.GetTypeInfo()
                    .GetConstructor(new[] { typeof(string) })
                    .Invoke(new[] { DefaultOutputFormatInstanceName }) as Autofac.Module;

                if (offlineModule != null)
                {
                    builder.RegisterModule(offlineModule);

                    TryLoadReportWriters(builder);
                }
            }
            catch (Exception) { }
        }

        private static void TryLoadReportWriters(ContainerBuilder builder)
        {
            foreach (var path in Directory.EnumerateFiles(GetApplicationDirectory(), "Microsoft.Fx.Portability.Reports.*.dll"))
            {
                try
                {
                    var name = new AssemblyName(Path.GetFileNameWithoutExtension(path));
                    var assembly = Assembly.Load(name);

                    builder.RegisterAssemblyTypes(assembly)
                        .AssignableTo<IReportWriter>()
                        .As<IReportWriter>()
                        .SingleInstance();
                }
                catch (Exception)
                {
                }
            }
        }

        private static string GetApplicationDirectory()
        {
            return Path.GetDirectoryName(typeof(DependencyBuilder).GetTypeInfo().Assembly.Location);
        }
    }
}
