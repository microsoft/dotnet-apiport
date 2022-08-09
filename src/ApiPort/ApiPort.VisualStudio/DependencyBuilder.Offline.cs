// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Autofac;

using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.Analysis;
using Microsoft.Fx.Portability.Analyzer;
using Microsoft.Fx.Portability.Reporting;

using System;
using System.IO;
using System.Reflection;

namespace ApiPort
{
    internal static class DependencyBuilder
    {
        internal const string DefaultOutputFormatInstanceName = "DefaultOutputFormat";

        public static void RegisterOfflineModule(this ContainerBuilder builder)
        {
            builder.RegisterType<DependencyOrderer>()
                .As<IDependencyOrderer>()
                .SingleInstance();

            builder.RegisterType<RequestAnalyzer>()
                .As<IRequestAnalyzer>()
                .SingleInstance();

            builder.RegisterType<AnalysisEngine>()
                .As<IAnalysisEngine>()
                .SingleInstance();

            builder.RegisterModule(new OfflineDataModule(DefaultOutputFormatInstanceName));
            LoadReportWriters(builder);
        }

        private static void LoadReportWriters(ContainerBuilder builder)
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
