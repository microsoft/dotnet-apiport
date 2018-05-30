// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Fx.Portability.Azure;
using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reporting;
using Microsoft.WindowsAzure.Storage;
using PortabilityService.Functions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace PortabilityService.Functions
{
    public static class Report
    {
        private static readonly TimeSpan s_retryDelay = TimeSpan.FromSeconds(2);

        [FunctionName("report")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "report/{submissionId}")] HttpRequestMessage req,
            string submissionId,
            [Inject] IStorage storage,
            [Inject] IReportTokenValidator validator,
            [Inject] IEnumerable<IReportWriter> reportWriters,
            ILogger log)
        {
            if (!validator.RequestHasValidToken(req))
            {
                return req.CreateResponse(HttpStatusCode.Unauthorized);
            }

            var (analyzeRequest, analyzeResult) = await GetSubmissionDataAsync(submissionId, storage, log);
            if (analyzeRequest == null)
            {
                // the request may have been received but not yet stored => the client should poll again
                var res = req.CreateResponse(HttpStatusCode.NotFound);
                res.Headers.RetryAfter = new RetryConditionHeaderValue(s_retryDelay);

                return res;
            }
            if (analyzeResult == null)
            {
                // AnalyzeRequest received but AnalyzeResult unavailable => client should continue polling
                var res = req.CreateResponse(HttpStatusCode.Accepted);
                res.Headers.RetryAfter = new RetryConditionHeaderValue(s_retryDelay);

                return res;
            }

            var mediaType = req.Headers.Accept.SingleOrDefault();
            if (mediaType == null)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            var reportWriter = reportWriters
                .SingleOrDefault(writer => writer.Format.MimeType.Equals(mediaType.MediaType, StringComparison.OrdinalIgnoreCase));
            if (reportWriter == null)
            {
                return req.CreateResponse(HttpStatusCode.UnsupportedMediaType);
            }

            var generator = new ReportGenerator();
            analyzeResult.ReportingResult = generator.ComputeReport(analyzeRequest, analyzeResult);

            using (var stream = new MemoryStream())
            {
                reportWriter.WriteStream(stream, analyzeResult);

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Content = new ByteArrayContent(stream.ToArray());

                return response;
            }
        }

        private static async Task<(AnalyzeRequest, AnalyzeResult)> GetSubmissionDataAsync(string submissionId, IStorage storage, ILogger log)
        {
            AnalyzeRequest analyzeRequest = null;
            AnalyzeResult analyzeResult = null;
            try
            {
                analyzeRequest = await storage.RetrieveRequestAsync(submissionId);
                analyzeResult = await storage.RetrieveResultFromBlobAsync(submissionId);
            }
            catch (StorageException ex)
            {
                log.LogError("exception getting submission data {ex}", ex);
            }

            return (analyzeRequest, analyzeResult);
        }
    }
}
