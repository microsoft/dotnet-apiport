// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.Analysis;
using Microsoft.Fx.Portability.Analyzer;
using Microsoft.Fx.Portability.Azure.ObjectModel;
using Microsoft.Fx.Portability.Cache;
using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reporting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PortabilityService.AnalysisService
{
    public class DependencyModule : Module
    {
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

                return new ObjectInBlobCache<FxApiUsageData>(settings.StorageAccount, "data", "fxmember.json.gz", settings.UpdateFrequency, CancellationToken.None);
            })
                .As<IObjectCache<FxApiUsageData>>()
                .As<IObjectCache>()
                .SingleInstance()
                .OnActivated(o => o.Instance.Start())
                .AutoActivate();

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

            builder.RegisterAdapter<IObjectCache<CatalogIndex>, IApiCatalogLookup>(cache => cache.Value.Catalog)
                .InstancePerRequest();

            builder.RegisterAdapter<IObjectCache<CatalogIndex>, ISearcher<string>>(cache => cache.Value.Index)
                .InstancePerRequest()
                .ExternallyOwned();

            builder.RegisterAdapter<IObjectCache<FxApiUsageData>, FxApiUsageData>(cache => cache.Value)
                .InstancePerRequest();

            builder.Register(CreateTargetNameParser).As<ITargetNameParser>().InstancePerRequest();
            builder.Register(CreateTargetMapper).As<ITargetMapper>().InstancePerRequest();
            builder.RegisterType<RequestAnalyzer>().As<IRequestAnalyzer>().InstancePerRequest();

            builder.RegisterType<AnalysisEngine>().As<IAnalysisEngine>();
            builder.RegisterType<ReportGenerator>().As<IReportGenerator>().SingleInstance();
        }

        private object CreateSettings(IComponentContext arg)
        {
            return new AnalysisServiceSettings(Configuration);
        }

        private ITargetMapper CreateTargetMapper(IComponentContext arg)
        {
            var mapper = new TargetMapper();
            var settings = arg.Resolve<IServiceSettings>();

            mapper.ParseAliasString(settings.TargetGroups);

            return mapper;
        }

        private ITargetNameParser CreateTargetNameParser(IComponentContext arg)
        {
            var catalog = arg.Resolve<IApiCatalogLookup>();
            var settings = arg.Resolve<IServiceSettings>();

            return new CloudTargetNameParser(catalog, settings);
        }
    }
}
