// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;
using PortabilityService.Gateway.Tests.Helpers;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace PortabilityService.Gateway.Tests
{
    /// <summary>
    /// Verifies that the API gateway's health check endpoint is working
    /// </summary>
    public class HealthCheckTests
    {
        [Fact]
        public static async Task HealthCheckTest()
        {
            // Use a TestServer wrapper around the API gateway to
            // get a client that can easily call it from this test
            var testGateway = TestGateway.CreateTestApp();
            var httpClient = testGateway.CreateClient();

            // Invoke the endpoint
            var response = await httpClient.GetAsync(new Uri("/hc", UriKind.Relative));

            // Verify status code
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Verify contents
            var expectedContents = new { Status = "Healthy" };
            var actualContents = JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(), expectedContents);
            Assert.Equal(expectedContents.Status, actualContents.Status);
        }
    }
}
