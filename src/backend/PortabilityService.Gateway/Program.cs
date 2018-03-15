// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;

namespace PortabilityService.Gateway
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
                .UseApplicationInsights()
                .ConfigureAppConfiguration(ConfigureAppConfiguration)
                .ConfigureLogging(ConfigureLogging)
                .UseStartup<Startup>()
                .Build();

        /// <summary>
        /// Configures app configuration specific to the API Gateway service
        /// </summary>
        private static void ConfigureAppConfiguration(WebHostBuilderContext context, IConfigurationBuilder configBuilder)
        {
            // Add configuration from reroute config files.
            // Reroutes.json is required; environment-specific reroute configuration
            // (which will override the default) is optional.
            configBuilder.AddJsonFile("reroutes.json");
            configBuilder.AddJsonFile($"reroutes.{context.HostingEnvironment.EnvironmentName}.json", true);
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
