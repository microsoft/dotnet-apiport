using Microsoft.Extensions.Logging;
using Microsoft.Fx.Portability.Azure;
using NSubstitute;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace PortabilityService.Functions.Tests
{
    public class TargetsTests
    {
        /// <summary>
        /// Smoke test - validates that embedded resource can be fetched and a value is returned from the endpoint
        /// </summary>
        [Fact]
        public async Task FetchesTargets()
        {
            var tokenValidator = Substitute.For<IReportTokenValidator>();
            tokenValidator.RequestHasValidToken(Arg.Any<HttpRequestMessage>()).Returns(false);

            using (var request = new HttpRequestMessage(HttpMethod.Get, string.Empty))
            {
                var response = await Targets.Run(request, Substitute.For<ILogger>());
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var contents = await response.Content.ReadAsStringAsync();

                Assert.Contains(".NET Core + Platform Extensions", contents, StringComparison.Ordinal);
            }
        }
    }
}
