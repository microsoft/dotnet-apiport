// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using PortabilityService.Gateway.Middleware;
using PortabilityService.Gateway.Reliability;
using System.Net.Http;

namespace PortabilityService.Gateway
{
    public class Startup
    {
        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            Configuration = configuration;
            LoggerFactory = loggerFactory;
        }

        public IConfiguration Configuration { get; }
        public ILoggerFactory LoggerFactory { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Register a ResiliencePoliciesFactory and then use it to create
            // and register the Policy[] to be used
            services.AddTransient<ResiliencePoliciesFactory>();
            services.AddSingleton(sp =>
                sp.GetRequiredService<ResiliencePoliciesFactory>().CreatePolicies());

            services.AddScoped<DelegatingHandler, HandlerWithPolicies>();

            services.AddOcelot(Configuration.GetSection("Gateway"))
                    .AddCacheManager(c =>
                    {
                        c.WithDictionaryHandle();
                    })
                    .AddDelegatingHandler(() =>
                    {
                        // Ocelot comes with built-in QoS options, but they're not very customizable,
                        // so this adds custom Polly policy to retry, circuit break, and timeout.

                        // Doesn't get dependencies from DI due to
                        // https://github.com/ThreeMammals/Ocelot/issues/259
                        var policiesFactory = new ResiliencePoliciesFactory(LoggerFactory.CreateLogger<ResiliencePoliciesFactory>(), Configuration);
                        return new HandlerWithPolicies(policiesFactory.CreatePolicies());
                    });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            var logger = LoggerFactory.CreateLogger<Startup>();

            if (env.IsDevelopment())
            {
                logger.LogInformation("Enabling developer exception page middleware");
                app.UseDeveloperExceptionPage();
            }

            var ocelotConfiguration = new OcelotPipelineConfiguration
            {
                // Work around https://github.com/ThreeMammals/Ocelot/issues/263
                PreQueryStringBuilderMiddleware = ContentHeaderForwardingMiddleware.ForwardContentHeaders
            };

            // Ocelot must be the last middleware in the pipeline
            app.UseOcelot(ocelotConfiguration).Wait();

            logger.LogInformation("Middleware pipeline configured");
        }
    }
}
