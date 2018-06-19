// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Fx.Portability.Cache;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PortabilityService.AnalysisService
{
    internal static class CatalogAvailableExtensions
    {
        public static IApplicationBuilder EnsureCatalogIsAvailable(this IApplicationBuilder app)
        {
            return app.UseMiddleware<EnsureCatalogAvailableMiddleware>();
        }

        private class EnsureCatalogAvailableMiddleware
        {
            private readonly RequestDelegate _next;
            private readonly ILogger<EnsureCatalogAvailableMiddleware> _log;
            private readonly IObjectCache<CatalogIndex> _cache;

            public EnsureCatalogAvailableMiddleware(RequestDelegate next, ILogger<EnsureCatalogAvailableMiddleware> log, IObjectCache<CatalogIndex> cache)
            {
                _next = next;
                _log = log;
                _cache = cache;
            }

            public Task InvokeAsync(HttpContext context)
            {
                if (_cache.Value.Catalog is null)
                {
                    return WriteOutput(context.Response, "Catalog is not available");
                }
                else
                {
                    return _next(context);
                }
            }

            private Task WriteOutput(HttpResponse response, string message)
            {
                _log.LogWarning(message);
                var bytes = Encoding.UTF8.GetBytes(message);
                response.StatusCode = (int)HttpStatusCode.FailedDependency;
                return response.Body.WriteAsync(bytes).AsTask();
            }
        }
    }
}
