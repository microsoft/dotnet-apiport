// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PortabilityService.Gateway.Tests.Helpers;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

// API Gateway tests need should not run in parallel since they depend on
// a WebHost listening on a particular port. Sharing the web host between tests
// does not work well because it would not be clear which requests it received from
// which test cases.
//
// An alternative would be to change the port the test WebHost is listening on for each
// test case and modify reroutes.json on a per-testcase basis to reroute to the proper port,
// but that would require non-trivial changes to how we setup the test API gateway and doesn't
// seem worthwhile given the small number of tests that will run sequentially in this assembly
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace PortabilityService.Gateway.Tests
{
    /// <summary>
    /// Tests to verify that requests are properly rerouted according to configuration in reroutes.json
    /// and that our custom QoS and header-forwarding policies are applied
    /// </summary>
    public class RerouteTests
    {
        static HttpClient GetTestClient() => TestGateway.CreateTestApp().CreateClient();
        static DownstreamListener GetDownstreamListener() => new DownstreamListener(new[] { "/GoodPath" }, 4321);

        // Test that positive-case reroutes work, as expected, 
        // with or without QoS options in the reroute configuration
        [Theory]
        [InlineData("/PathExists")]
        [InlineData("/PathExistsQoS")]
        [InlineData("/PathExistsEmptyQoS")]
        public static async Task RerouteBasedOnGatewayConfiguration(string path)
        {
            using (var downstreamListener = GetDownstreamListener())
            using (var testClient = GetTestClient())
            {
                var response = await testClient.GetAsync(new Uri(path, UriKind.Relative));

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Single(downstreamListener.Requests);
            }
        }

        // Test that Content-Encoding headers are properly forwarded 
        [Fact]
        public static async Task RerouteForwardsContentEncodingHeader()
        {
            using (var downstreamListener = GetDownstreamListener())
            using (var testClient = GetTestClient())
            {
                // Create a sample request with a Content-Encoding header
                var request = new HttpRequestMessage(HttpMethod.Get, new Uri("/PathExists", UriKind.Relative));
                request.Content = new ByteArrayContent(new byte[] { 1, 2, 3, 4 });
                request.Content.Headers.ContentEncoding.Add("gzip");

                var response = await testClient.SendAsync(request);

                // Verify that the request (including header) was received
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Single(downstreamListener.Requests);
                Assert.Equal("gzip", downstreamListener.Requests.FirstOrDefault()?.Headers["Content-Encoding"]);
            }
        }

        // Test that routes with timeout options correctly timeout
        [Fact]
        public static async Task RerouteTimesOutAccordingToQoSConfig()
        {
            using (var downstreamListener = GetDownstreamListener())
            using (var testClient = GetTestClient())
            {
                var wait = 500;
                var response = await testClient.GetAsync(new Uri($"/PathExistsButIsSlow?slow={wait}", UriKind.Relative));

                // Verify that the API gateway timed-out the request
                Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode); ;

                // Wait for the request to complete and confirm that it did go to the right endpoint (eventually)
                await Task.Delay(wait);
                Assert.Single(downstreamListener.Requests);
            }
        }

        // Test that QoS circuit breakers kick in, as expected
        [Fact]
        public static async Task RerouteFailsFastIfBreakerOpen()
        {
            using (var downstreamListener = GetDownstreamListener())
            using (var testClient = GetTestClient())
            {
                var response1 = await testClient.GetAsync(new Uri("/PathDoesNotExist", UriKind.Relative));
                var response2 = await testClient.GetAsync(new Uri("/PathDoesNotExist", UriKind.Relative));
                var response3 = await testClient.GetAsync(new Uri("/PathDoesNotExist", UriKind.Relative));
                var response4 = await testClient.GetAsync(new Uri("/PathExistsQoS", UriKind.Relative));

                // Verify that the first two requests return 404
                Assert.Equal(HttpStatusCode.NotFound, response1.StatusCode);
                Assert.Equal(HttpStatusCode.NotFound, response2.StatusCode);

                // Verify that after two failures the circuit breaker opens and returns 'ServiceUnavailable'
                Assert.Equal(HttpStatusCode.ServiceUnavailable, response3.StatusCode);

                // Verify that the open breaker does not affect other paths (with separate circuit breakers)
                Assert.Equal(HttpStatusCode.OK, response4.StatusCode);
            }
        }
        
    }
}
