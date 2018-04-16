// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PortabilityService.ConfigurationService
{
    public class Program
    {
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
            if (context.HostingEnvironment.IsDevelopment())
            {
                loggingBuilder.AddDebug();
                loggingBuilder.AddConsole();
            }
            else
            {
                // TODO: Add Application insights here
            }
        }
    }
}
