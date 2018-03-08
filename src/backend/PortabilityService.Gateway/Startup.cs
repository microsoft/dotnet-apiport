// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOcelot(Configuration.GetSection("Gateway"))
                    .AddCacheManager(c =>
                    {
                        c.WithDictionaryHandle();
                    });

            // Ocelot comes with built-in QoS options, but they're not very customizable,
            // so this adds custom Polly policy to retry, circuit break, and timeout.
            // Works around https://github.com/ThreeMammals/Ocelot/issues/264
            var qosProviderService = services.FirstOrDefault(d => d.ServiceType == typeof(IQoSProviderFactory));
            services.Remove(qosProviderService);

            services.AddSingleton<PortabilityServiceQoSProviderFactory>();
            services.AddSingleton<IQoSProviderFactory, PortabilityServiceQoSProviderFactory>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

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
