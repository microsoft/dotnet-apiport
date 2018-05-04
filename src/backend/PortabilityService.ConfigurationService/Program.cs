// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;

namespace PortabilityService.ConfigurationService
{
    public class Program
    {
        const string APP_INSIGHTS_KEY_KEY = "ApplicationInsights:InstrumentationKey";

        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            var host = new WebHostBuilder()
               .UseKestrel()
               .ConfigureAppConfiguration(ConfigureAppConfiguration)
               .ConfigureLogging(ConfigureLogging)
               .UseContentRoot(Directory.GetCurrentDirectory())
               .UseIISIntegration()
               .UseStartup<Startup>()
               .Build();

            host.Run();

            return host;
        }

        /// <summary>
        /// Sets up the applications IConfiguration object
        /// </summary>
        private static void ConfigureAppConfiguration(WebHostBuilderContext context, IConfigurationBuilder configBuilder)
        {
            var env = context.HostingEnvironment;

            configBuilder
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddJsonFile("PortabilityServiceConfigurationSettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"PortabilityServiceConfigurationSettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
        }

        /// <summary>
        /// Setup application logging
        /// </summary>
        private static void ConfigureLogging(WebHostBuilderContext context, ILoggingBuilder loggingBuilder)
        {
            var env = context.HostingEnvironment;
            var config = context.Configuration;

            // Clear default providers
            loggingBuilder.ClearProviders();

            // Create Serilog configuration from app configuration
            var serilogLoggerConfiguration = new LoggerConfiguration()
                .ReadFrom.Configuration(config.GetSection("PortabilityServiceSettings"));

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
