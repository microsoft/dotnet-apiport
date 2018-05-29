// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.Reporting;
using NSubstitute;
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
            var reportWriters = Substitute.For<IEnumerable<IReportWriter>>();
            var defaultFormat = Substitute.For<ResultFormatInformation>();

            using (var request = new HttpRequestMessage { Method = HttpMethod.Get })
            {
                var response = ReportFormat.Run(request, reportWriters, defaultFormat, arg);

                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            }
        }

        [Fact]
        public async Task RespondsWithFormatsFromReportFunction()
        {
            var reportWriters = Substitute.For<IEnumerable<IReportWriter>>();
            var defaultFormat = Substitute.For<ResultFormatInformation>();

            using (var request = new HttpRequestMessage { Method = HttpMethod.Get })
            {
                request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

                var response = ReportFormat.Run(request, reportWriters, defaultFormat, null);

                var contentStream = await response.Content.ReadAsStreamAsync();
                var formats = DataExtensions.Deserialize<IEnumerable<ResultFormatInformation>>(contentStream);
                Assert.All(formats, format => reportWriters.Single(writer => writer.Format.Equals(format)));
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }

        [Fact]
        public void RespondsWithDefaultFormatForDefaultArg()
        {
            var defaultFormat = new ResultFormatInformation
            {
                DisplayName = "foo",
                FileExtension = "bar",
                MimeType = "baz/quux"
            };
            var reportWriters = Substitute.For<IEnumerable<IReportWriter>>();

            using (var request = new HttpRequestMessage { Method = HttpMethod.Get })
            {
                request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

                var response = ReportFormat.Run(request, reportWriters, defaultFormat, "default");

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(response.TryGetContentValue(out ResultFormatInformation actual));
                Assert.Equal(defaultFormat, actual);
            }
        }
    }
}
