// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PortabilityService.ConfigurationProvider;
using Serilog;

namespace PortabilityService.AnalysisEngine
{
    public class Program
    {
        const string APP_INSIGHTS_KEY_KEY = "ApplicationInsights:InstrumentationKey";

        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(ConfigureAppConfiguration)
                .ConfigureLogging(ConfigureLogging)
                .UseStartup<Startup>()
                .Build();

        /// <summary>
        /// Configures app configuration
        /// </summary>
        private static void ConfigureAppConfiguration(WebHostBuilderContext context, IConfigurationBuilder configBuilder)
        {
            configBuilder.AddPortabilityServiceConfiguration();
        }

        /// <summary>
        /// Configure logging providers
        /// </summary>
        private static void ConfigureLogging(WebHostBuilderContext context, ILoggingBuilder loggingBuilder)
        {
            var env = context.HostingEnvironment;
            var config = context.Configuration;

            // Clear default providers
            loggingBuilder.ClearProviders();

            // Create Serilog configuration from app configuration
            var serilogLoggerConfiguration = new LoggerConfiguration()
                .ReadFrom.Configuration(config);

            if (!env.IsDevelopment())
            {
                // In non-dev environments, add an App Insights sink
                serilogLoggerConfiguration = serilogLoggerConfiguration
                    .WriteTo.ApplicationInsightsTraces(config[APP_INSIGHTS_KEY_KEY]);
            }

            // Add the Serilog logging provider
            loggingBuilder.AddSerilog(serilogLoggerConfiguration.CreateLogger());
        }
    }
}
