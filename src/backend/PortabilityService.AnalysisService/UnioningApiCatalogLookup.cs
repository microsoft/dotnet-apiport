// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PortabilityService.AnalysisService;
using System;
using System.Runtime.Versioning;

namespace Microsoft.Fx.Portability.ObjectModel
{
    public class UnioningApiCatalogLookup : CloudApiCatalogLookup2
    {
        private static readonly FrameworkName AspNetCore1_0 = new FrameworkName("ASP.NET Core", Version.Parse("1.0"));
        private static readonly FrameworkName NetCore1_0 = new FrameworkName(".NET Core", Version.Parse("1.0"));
        private static readonly FrameworkName NetCore1_1 = new FrameworkName(NetCore1_0.Identifier, Version.Parse("1.1"));

        private readonly IServiceSettings _settings;

        public UnioningApiCatalogLookup(DotNetCatalog catalog, IServiceSettings settings)
            : base(catalog)
        {
            _settings = settings;
        }

        public override bool IsMemberInTarget(string docId, FrameworkName targetName, out Version introducedVersion)
        {
            bool memberInTarget = base.IsMemberInTarget(docId, targetName, out introducedVersion);
            if (!memberInTarget && _settings.UnionAspNetWithNetCore)
            {
                // An ASP.NET Core 1.0 app is a .NET Core 1.0 app, but their targets are disjoint in the catalog.
                // To produce better reports, we check here whether APIs the catalog considers unavailable to
                // one are available to the other.
                if (targetName == AspNetCore1_0)
                {
                    return base.IsMemberInTarget(docId, NetCore1_0, out introducedVersion);
                }
                else if (targetName == NetCore1_1 || targetName == NetCore1_0)
                {
                    return base.IsMemberInTarget(docId, AspNetCore1_0, out introducedVersion);
                }
            }

            return memberInTarget;
        }
    }
}
