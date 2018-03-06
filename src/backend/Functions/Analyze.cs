// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.ObjectModel;
using Newtonsoft.Json;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Functions
{
    public static class Analyze
    {
        [FunctionName("analyze")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
            TraceWriter log)
        {
            var analyzeRequest = await DeserializeRequest(req.Content);
            if (analyzeRequest == null)
            {
                log.Error("invalid request");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            var submissionId = Guid.NewGuid().ToString();
            log.Info($"created submission id {submissionId}");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(submissionId);

            return response;
        }

        public static async Task<AnalyzeRequest> DeserializeRequest(HttpContent content)
        {
            try
            {
                var body = await DecompressContent(content);
                return JsonConvert.DeserializeObject<AnalyzeRequest>(body, DataExtensions.JsonSettings);
            }
            catch
            {
                return null;
            }
        }

        public static async Task<string> DecompressContent(HttpContent content)
        {
            var contentStream = await content.ReadAsStreamAsync();
            using (var gzstream = new GZipStream(contentStream, CompressionMode.Decompress))
            using (var reader = new StreamReader(gzstream))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}
