// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Autofac;
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
    internal static partial class DependencyBuilder
    {
        internal const string AutofacConfiguration = "autofac.json";
        internal const string ConfigurationFile = "config.json";
        internal const string DefaultOutputFormatInstanceName = "DefaultOutputFormat";

#if FEATURE_WPF
        public static IContainer Build(ProductInformation productInformation)
#else
        public static IContainer Build(ICommandLineOptions options, ProductInformation productInformation)
#endif
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<TargetMapper>()
                .As<ITargetMapper>()
#if !FEATURE_WPF
                .OnActivated(c => c.Instance.LoadFromConfig(options.TargetMapFile))
#endif
                .SingleInstance();

            builder.RegisterInstance<ProductInformation>(productInformation);
#if !FEATURE_WPF
            builder.RegisterInstance<ICommandLineOptions>(options);

            builder.RegisterType<ConsoleCredentialProvider>()
                .As<ICredentialProvider>()
                .SingleInstance();

            builder.Register(context =>
            {
                var directory = Path.GetDirectoryName(typeof(Program).GetTypeInfo().Assembly.Location);

                return new ProxyProvider(
                    directory,
                    ConfigurationFile,
                    context.Resolve<ICredentialProvider>());
            })
            .As<IProxyProvider>()
            .SingleInstance();
#endif
#if !FEATURE_WPF
            if (options.Command == AppCommand.DumpAnalysis)
            {
                builder.RegisterType<FileOutputApiPortService>()
                    .As<IApiPortService>()
                    .SingleInstance();
            }
            else
            {
                builder.Register(context =>
                    new ApiPortService(
                            context.Resolve<ICommandLineOptions>().ServiceEndpoint,
                            context.Resolve<ProductInformation>(),
                            context.Resolve<IProxyProvider>()))
                    .As<IApiPortService>()
                    .SingleInstance();
            }
#else
            builder.Register(context =>
                    new ApiPortService(
                        "https://portability.dot.net",
                         context.Resolve<ProductInformation>()))
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

            builder.RegisterType<SystemObjectFinder>()
                .SingleInstance();
#if FEATURE_WPF
            builder.RegisterType<WPFApiPortClient>()
                .SingleInstance();
#else
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
#endif

            RegisterOfflineModule(builder);

            return builder.Build();
        }

        /// <summary>
        /// Used to register offline mode in the offline build.
        /// </summary>
        static partial void RegisterOfflineModule(ContainerBuilder builder);
    }
}
