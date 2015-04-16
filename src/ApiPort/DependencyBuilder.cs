// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.Analyzer;
using Microsoft.Fx.Portability.Reporting;
using Microsoft.Practices.Unity;
using System;

namespace ApiPort
{
    internal static class DependencyBuilder
    {
        public static IUnityContainer Build(ICommandLineOptions options, ProductInformation productInformation)
        {
            var container = new UnityContainer();

            var targetMapper = new TargetMapper();
            targetMapper.LoadFromConfig();

            container.RegisterInstance(options);
            container.RegisterInstance<ITargetMapper>(targetMapper);
            container.RegisterInstance<IApiPortService>(new ApiPortService(options.ServiceEndpoint, productInformation));
            container.RegisterType<IDependencyFinder, ReflectionMetadataDependencyFinder>(new ContainerControlledLifetimeManager());
            container.RegisterType<IReportGenerator, ReportGenerator>(new ContainerControlledLifetimeManager());
            container.RegisterType<ApiPortService>(new ContainerControlledLifetimeManager());
            container.RegisterType<IFileSystem, WindowsFileSystem>(new ContainerControlledLifetimeManager());
            container.RegisterType<IFileWriter, ReportFileWriter>(new ContainerControlledLifetimeManager());

            if (Console.IsOutputRedirected)
            {
                container.RegisterInstance<IProgressReporter>(new TextWriterProgressReporter(Console.Out));
            }
            else
            {
                container.RegisterType<IProgressReporter, ConsoleProgressReporter>(new ContainerControlledLifetimeManager());
            }

            return container;
        }
    }
}
