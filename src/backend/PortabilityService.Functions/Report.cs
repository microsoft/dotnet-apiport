// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace PortabilityService.Functions
{
    public static class Report
    {
        [FunctionName("report")]
        public static HttpResponseMessage Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "report/{submissionId}")] HttpRequestMessage req,
            string submissionId,
            TraceWriter log)
        {
            if (!ValidAccessKey(req))
            {
                return req.CreateResponse(HttpStatusCode.Unauthorized);
            }

            // simulate report generation taking some time
            if (new Random().Next(10) < 4)
            {
                // TODO this should return 202 only when the analyze request has been received; 
                // if it has not, an error e.g. 404 is more appropriate
                var response = req.CreateResponse(HttpStatusCode.Accepted);
                response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(2d));

                return response;
            }

            switch (req.Headers.Accept.ToString())
            {
                case MediaType.Json:
                    return JsonReport(req);
                case MediaType.Excel:
                    return ExcelReport(req);
                default:
                    return req.CreateResponse(HttpStatusCode.UnsupportedMediaType);
            }
        }

        public static bool ValidAccessKey(HttpRequestMessage request)
        {
            // TODO generate and persist a new unique key in Analyze, validate it here
            var authHeader = request.Headers.Authorization;
            if (authHeader == null || !authHeader.Scheme.Equals("Bearer", StringComparison.Ordinal))
            {
                return false;
            }

            var token = authHeader.Parameter;
            var submissionId = request.RequestUri.Segments.Last();
            var chars = submissionId.ToCharArray();
            Array.Reverse(chars);
            var expectedToken = new string(chars);

            return token.Equals(expectedToken, StringComparison.Ordinal);
        }

        public static HttpResponseMessage JsonReport(HttpRequestMessage request)
        {
            // TODO retrieve a real report generated somewhere else
            var response = request.CreateResponse(HttpStatusCode.OK);
            using (var stream = typeof(Report).Assembly.GetManifestResourceStream("apiport-demo.dll.json"))
            using (var sr = new StreamReader(stream))
            {
                var json = sr.ReadToEnd();
                var content = new StringContent(json, System.Text.Encoding.UTF8, MediaType.Json);
                response.Content = content;
            }

            return response;
        }

        private static HttpResponseMessage ExcelReport(HttpRequestMessage request)
        {
            // TODO retrieve a real report generated somewhere else
            var response = request.CreateResponse(HttpStatusCode.OK);
            using (var stream = typeof(Report).Assembly.GetManifestResourceStream("apiport-demo.dll.xlsx"))
            {
                var bytes = new byte[stream.Length];
                stream.Read(bytes, 0, (int)stream.Length);

                var content = new ByteArrayContent(bytes);
                response.Content = content;
                response.Content.Headers.Add("Content-Type", MediaType.Excel);
            }

            return response;
        }
    }
}
