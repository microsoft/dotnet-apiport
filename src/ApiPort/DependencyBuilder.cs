// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Autofac;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.Analysis;
using Microsoft.Fx.Portability.Analyzer;
using Microsoft.Fx.Portability.ObjectModel;
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
        private const string DefaultOutputFormatInstanceName = "DefaultOutputFormat";

        public static IContainer Build(ICommandLineOptions options, ProductInformation productInformation)
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<TargetMapper>()
                .As<ITargetMapper>()
                .OnActivated(c => c.Instance.LoadFromConfig(options.TargetMapFile))
                .SingleInstance();

            builder.RegisterInstance<ICommandLineOptions>(options);

#if DEBUG_SERVICE
            // For debug purposes, the FileOutputApiPortService helps as it serializes the request to json and opens it with the
            // default json handler. To use this service, uncomment the the next line and comment the one after that.
            container.RegisterType<FileOutputApiPortService>()
                .As<IApiPortService>()
                .SingleInstance();
#else
            builder.RegisterInstance<IApiPortService>(new ApiPortService(options.ServiceEndpoint, productInformation));
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

                return new ReadWriteApiPortOptions(opts)
                {
                    OutputFormats = new[] { ctx.ResolveNamed<string>(DefaultOutputFormatInstanceName) }
                };
            })
            .SingleInstance();

            builder.RegisterType<DocIdSearchRepl>();

            builder.RegisterType<ApiPortServiceSearcher>()
                .As<ISearcher<string>>()
                .SingleInstance();

            // Register the default output format name
            builder.RegisterInstance("Excel")
                .Named<string>(DefaultOutputFormatInstanceName);

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

            return builder.Build();
        }

        private static void TryLoadOffline(ContainerBuilder builder)
        {
            try
            {
                var assembly = Assembly.Load("Microsoft.Fx.Portability.Offline");
                var type = assembly.GetType("Microsoft.Fx.Portability.OfflineDataModule");
                var offlineModule = type?.GetTypeInfo()
                    .GetConstructor(new[] { typeof(string) })
                    .Invoke(new[] { DefaultOutputFormatInstanceName }) as Autofac.Module;

                if (offlineModule != null)
                {
                    builder.RegisterModule(offlineModule);
                }
            }
            catch (Exception) { }
        }

        private static string GetApplicationDirectory()
        {
            return Path.GetDirectoryName(typeof(DependencyBuilder).GetTypeInfo().Assembly.Location);
        }
    }
}
