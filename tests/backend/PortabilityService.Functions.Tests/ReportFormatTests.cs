// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Xunit;

namespace PortabilityService.Functions.Tests
{
    public class ReportFormatTests
    {
        [Theory]
        [InlineData("foo")]
        [InlineData(" ")]
        public void RespondsBadRequestForInvalidRouteArg(string arg)
        {
            var request = new HttpRequestMessage { Method = HttpMethod.Get };

            var response = ReportFormat.Run(request, arg);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task RespondsWithFormatsFromReportFunction()
        {
            var request = new HttpRequestMessage { Method = HttpMethod.Get };
            request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            var response = ReportFormat.Run(request, null);

            var contentStream = await response.Content.ReadAsStreamAsync();
            var formats = DataExtensions.Deserialize<IEnumerable<ResultFormatInformation>>(contentStream);
            Assert.All(formats, format => Report.ReportWriters.Single(writer => writer.Format.Equals(format)));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task RespondsWithOneFormatForDefaultArg()
        {
            var request = new HttpRequestMessage { Method = HttpMethod.Get };
            request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            var response = ReportFormat.Run(request, "default");

            var contentStream = await response.Content.ReadAsStreamAsync();
            Assert.True(response.TryGetContentValue(out ResultFormatInformation defaultFormat));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
