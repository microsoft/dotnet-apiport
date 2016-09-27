// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Autofac;
using Microsoft.Fx.Portability.Analysis;
using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reporting;
using Microsoft.Fx.Portability.Reports;
using System.Collections.Generic;

namespace Microsoft.Fx.Portability
{
    public class OfflineDataModule : Module
    {
        private readonly string _defaultOutputFormatName;

        public OfflineDataModule(string name)
        {
            _defaultOutputFormatName = name;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<OfflineApiPortService>()
                .As<IApiPortService>()
                .SingleInstance();

            builder.RegisterType<OfflineApiCatalogLookup>()
                .As<IApiCatalogLookup>()
                .SingleInstance();

            builder.RegisterType<OfflineBreakingChanges>()
                .As<IEnumerable<BreakingChange>>()
                .SingleInstance();

            builder.RegisterType<OfflineApiRecommendations>()
                .As<IApiRecommendations>()
                .SingleInstance();

            builder.RegisterType<JsonReportWriter>()
                .As<IReportWriter>()
                .SingleInstance();

#if FEATURE_HTML_WRITER
            // Currently, the HTML writer has dependencies that do not work on .NET Core
            builder.RegisterType<HtmlReportWriter>()
                .As<IReportWriter>()
                .SingleInstance();
#endif

            builder.RegisterType<TargetNameParser>()
                .WithParameter(TypedParameter.From(".NET Framework"))
                .As<ITargetNameParser>()
                .SingleInstance();

            builder.Register(ctx =>
            {
                return ctx.Resolve<IReportWriter>().Format.DisplayName;
            })
            .Named<string>(_defaultOutputFormatName);
        }
    }
}
