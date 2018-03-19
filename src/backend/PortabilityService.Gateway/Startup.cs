// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Requester.QoS;
using PortabilityService.Gateway.Middleware;
using PortabilityService.Gateway.Reliability;
using System.Linq;

namespace PortabilityService.Gateway
{
    public class Startup
    {
        const string HEALTH_CHECK_PATH = "/hc";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Register services required by Ocelot (API gateway library)
            services.AddOcelot(Configuration.GetSection("Gateway"))
                    .AddCacheManager(c =>
                    {
                        // Include in-memory caching so to reduce downstream traffic
                        c.WithDictionaryHandle();
                    });

            // Ocelot comes with built-in QoS options, but they're not very customizable,
            // so this adds custom Polly policy to retry, circuit break, and timeout.
            // Works around https://github.com/ThreeMammals/Ocelot/issues/264
            // Once that issue is resolved, the custom IQoSProviderFactory can be 
            // removed (and replaced by reroute-specific delegating handlers)
            var qosProviderService = services.FirstOrDefault(d => d.ServiceType == typeof(IQoSProviderFactory));
            services.Remove(qosProviderService);

            services.AddSingleton<IQoSProviderFactory, PortabilityServiceQoSProviderFactory>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Add a single non-reroute endpoint that orchestrators can use to know if 
            // this service is up and ready
            app.Use(async (context, next) =>
            {
                // If the request path matches the health check path,
                // return a healthy acknowledgement
                if (context.Request.Path == HEALTH_CHECK_PATH)
                {
                    context.Response.Headers.Add("Content-Type", "application/json");
                    await context.Response.WriteAsync("{\"status\":\"Healthy\"}");
                    return;
                }

                // Otherwise, proceed with Ocelot's rerouting middleware
                await next();
            });

            var ocelotConfiguration = new OcelotPipelineConfiguration
            {
                // Work around https://github.com/ThreeMammals/Ocelot/issues/263
                PreQueryStringBuilderMiddleware = ContentHeaderForwardingMiddleware.ForwardContentHeaders
            };

            // Ocelot must be the last middleware in the pipeline
            app.UseOcelot(ocelotConfiguration).Wait();
        }
    }
}
