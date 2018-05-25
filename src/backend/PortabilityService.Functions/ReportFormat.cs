// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Fx.Portability;
using System;
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
            string arg)
        {
            if (arg == null)
            {
                return req.CreateResponse(HttpStatusCode.OK, Formats);
            }

            return "default".Equals(arg, StringComparison.Ordinal)
                ? req.CreateResponse(HttpStatusCode.OK, Formats[0]) // TODO return default format as configured somewhere
                : req.CreateResponse(HttpStatusCode.BadRequest);
        }

        private static ResultFormatInformation[] Formats { get; } = Report.ReportWriters
            .Select(writer => writer.Format)
            .ToArray();
    }
}
