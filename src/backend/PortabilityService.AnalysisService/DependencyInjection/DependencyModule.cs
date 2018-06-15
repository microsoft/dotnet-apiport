// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.Analysis;
using Microsoft.Fx.Portability.Analyzer;
using Microsoft.Fx.Portability.Azure.Storage;
using Microsoft.Fx.Portability.Cache;
using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reporting;
using Microsoft.WindowsAzure.Storage;
using System.Threading;

namespace PortabilityService.AnalysisService
{
    public class DependencyModule : Module
    {
        private const string BlobStorageConnectionStringKeyName = "BlobStorageConnection";

        public DependencyModule(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(CreateSettings).As<IServiceSettings>().SingleInstance();

            builder.Register(arg =>
            {
                var settings = arg.Resolve<IServiceSettings>();

                return new CatalogInBlobIndexCache(settings, CancellationToken.None);
            })
            .As<IObjectCache<CatalogIndex>>()
            .As<IObjectCache>()
            .SingleInstance()
            .OnActivated(o => o.Instance.Start())
            .AutoActivate();

            // TODO (yumeng): replace UnioningApiCatalogLookup with a concrete type implementing IApiCatalogLookup
            // and interacting with the planned catelog service.
            builder.RegisterAdapter<IObjectCache<CatalogIndex>, IApiCatalogLookup>(cache => cache.Value.Catalog)
                .InstancePerLifetimeScope();

            builder.RegisterAdapter<IObjectCache<CatalogIndex>, ISearcher<string>>(cache => cache.Value.Index)
                .InstancePerLifetimeScope()
                .ExternallyOwned();

            builder.Register(CreateTargetNameParser).As<ITargetNameParser>().InstancePerLifetimeScope();
            builder.Register(CreateTargetMapper).As<ITargetMapper>().InstancePerLifetimeScope();
            builder.RegisterType<RequestAnalyzer>().As<IRequestAnalyzer>().InstancePerLifetimeScope();

            builder.RegisterType<AnalysisEngine>().As<IAnalysisEngine>().InstancePerLifetimeScope();
            builder.RegisterType<ReportGenerator>().As<IReportGenerator>().SingleInstance();
            builder.RegisterType<CloudPackageFinder>().As<IPackageFinder>().SingleInstance();

            builder.RegisterModule<ApiPortDataModule>();

            builder.Register(CreateStorage).As<IStorage>().SingleInstance();
        }

        private AzureStorage CreateStorage(IComponentContext arg)
        {
            var connectionString = Configuration.GetConnectionString(BlobStorageConnectionStringKeyName);
            return new AzureStorage(CloudStorageAccount.Parse(connectionString));
        }

        private AnalysisServiceSettings CreateSettings(IComponentContext arg)
        {
            return new AnalysisServiceSettings(Configuration);
        }

        private TargetMapper CreateTargetMapper(IComponentContext arg)
        {
            var mapper = new TargetMapper();
            var settings = arg.Resolve<IServiceSettings>();

            mapper.ParseAliasString(settings.TargetGroups);

            return mapper;
        }

        private CloudTargetNameParser CreateTargetNameParser(IComponentContext arg)
        {
            var catalog = arg.Resolve<IApiCatalogLookup>();
            var settings = arg.Resolve<IServiceSettings>();

            return new CloudTargetNameParser(catalog, settings);
        }
    }
}
