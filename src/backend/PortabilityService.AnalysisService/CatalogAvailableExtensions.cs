// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Fx.Portability.Cache;
using System.Net;
using System.Text;

namespace PortabilityService.AnalysisService
{
    internal static class CatalogAvailableExtensions
    {
        public static IApplicationBuilder EnsureCatalogIsAvailable(this IApplicationBuilder app)
        {
            return app.Use((context, next) =>
            {
                var index = context.RequestServices.GetService<IObjectCache<CatalogIndex>>();

                if (index.Value.Catalog is null)
                {
                    var text = "Catalog not available";
                    var bytes = Encoding.UTF8.GetBytes(text);
                    context.Response.StatusCode = (int)HttpStatusCode.FailedDependency;
                    return context.Response.Body.WriteAsync(bytes).AsTask();
                }
                else
                {
                    return next();
                }
            });
        }
    }
}
