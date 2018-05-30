// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.Reporting;
using PortabilityService.Functions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace PortabilityService.Functions
{
    public static class ReportFormat
    {
        [FunctionName("ReportFormat")]
        public static HttpResponseMessage Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "reportformat/{arg:alpha?}")] HttpRequestMessage req,
            [Inject] IEnumerable<IReportWriter> reportWriters,
            [Inject] ResultFormatInformation defaultFormat,
            string arg)
        {
            var formats = reportWriters.Select(writer => writer.Format);
            if (arg == null)
            {
                return req.CreateResponse(HttpStatusCode.OK, formats);
            }

            return "default".Equals(arg, StringComparison.Ordinal)
                ? req.CreateResponse(HttpStatusCode.OK, defaultFormat)
                : req.CreateResponse(HttpStatusCode.BadRequest);
        }
    }
}
