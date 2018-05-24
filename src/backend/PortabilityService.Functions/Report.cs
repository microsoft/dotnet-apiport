// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reporting;
using Microsoft.Fx.Portability.Reports;
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
        [FunctionName("report")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "report/{submissionId}")] HttpRequestMessage req,
            string submissionId,
            [Inject] IStorage storage,
            ILogger log)
        {
            if (!ValidAccessKey(req))
            {
                return req.CreateResponse(HttpStatusCode.Unauthorized);
            }

            var (analyzeRequest, analyzeResult) = await GetSubmissionDataAsync(submissionId, storage, log);
            if (analyzeRequest == null)
            {
                // the request may have been received but not yet stored => the client should poll again
                var res = req.CreateResponse(HttpStatusCode.NotFound);
                res.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(4d));
            }
            if (analyzeResult == null)
            {
                // AnalyzeRequest received but AnalyzeResult not available => client should continue polling
                var res = req.CreateResponse(HttpStatusCode.Accepted);
                res.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(2d));

                return res;
            }

            var response = req.CreateResponse();
            try
            {
                response.Content = GetReportContent(req.Headers.Accept.SingleOrDefault(), analyzeRequest, analyzeResult);
                response.StatusCode = HttpStatusCode.OK;
            }
            catch (UnsupportedMediaTypeException ex)
            {
                log.LogError("unknown format {MediaType} requested", ex.MediaType);
                response.StatusCode = HttpStatusCode.UnsupportedMediaType;
            }

            return response;
        }

        public static async Task<(AnalyzeRequest, AnalyzeResult)> GetSubmissionDataAsync(string submissionId, IStorage storage, ILogger log)
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

        private static readonly ITargetMapper s_targetMapper = new TargetMapper();
        public static IReadOnlyCollection<IReportWriter> ReportWriters { get; } = new IReportWriter[]
        {
            new ExcelReportWriter(s_targetMapper),
            // TODO HtmlReportWriter fails, possibly because RazorEngine can't write temp files to disk
            //new HtmlReportWriter(s_targetMapper),
            new JsonReportWriter()
        };

        public static ByteArrayContent GetReportContent(MediaTypeWithQualityHeaderValue mediaType, AnalyzeRequest analyzeRequest, AnalyzeResult analyzeResult)
        {
            var reportWriter = ReportWriters
                .SingleOrDefault(writer => writer.Format.MimeType.Equals(mediaType.MediaType, StringComparison.OrdinalIgnoreCase));
            if (mediaType == null || reportWriter == null)
            {
                throw new UnsupportedMediaTypeException("no appropriate report writer found", mediaType);
            }

            var generator = new ReportGenerator();
            analyzeResult.ReportingResult = generator.ComputeReport(analyzeRequest, analyzeResult);

            using (var stream = new MemoryStream())
            {
                reportWriter.WriteStream(stream, analyzeResult);

                return new ByteArrayContent(stream.ToArray());
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
    }
}
