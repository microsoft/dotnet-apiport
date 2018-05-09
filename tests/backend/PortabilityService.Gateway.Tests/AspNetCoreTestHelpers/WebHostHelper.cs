// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace PortabilityService.Gateway.Tests.Helpers
{
    /// <summary>
    /// Helper for generating a TestHost-run version of the API Gateway
    /// for end-to-end integration-style tests
    /// </summary>
    internal static class TestGateway
    {
        internal static TestServer CreateTestApp()
        {
            var webHostBuilder = CreateTestWebHost();

            // Set the content root so that correct config files (reroutes.json) are loaded
            webHostBuilder.UseContentRoot(Path.GetDirectoryName(typeof(TestGateway).Assembly.Location));

            return new TestServer(webHostBuilder);
        }

        private static IWebHostBuilder CreateTestWebHost() =>
            WebHost.CreateDefaultBuilder()
                .ConfigureAppConfiguration((WebHostBuilderContext context, IConfigurationBuilder configBuilder) => configBuilder.AddJsonFile("reroutes.json"))
                .UseStartup<Startup>();
    }
}
