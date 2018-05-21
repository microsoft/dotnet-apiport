// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Fx.Portability.Analyzer;
using Microsoft.Fx.Portability.Azure.Storage;
using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.WindowsAzure.Storage;

namespace PortabilityService.AnalysisService
{
    public class Startup
    {
        private const string BlobStorageConnectionStringKeyName = "BlobStorageConnectionString";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            //TODO: replace with configuration service
            var connectionString = Configuration[BlobStorageConnectionStringKeyName];

            // Storage
            services.AddSingleton<IStorage>(new AzureStorage(CloudStorageAccount.Parse(connectionString)));

            //TODO: add a real RequestAnalyzer
            services.AddSingleton<IRequestAnalyzer>(new DemoRequestAnalyzer());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
