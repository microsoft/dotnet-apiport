// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Fx.Portability
{
    public sealed class ApiPortService : IDisposable, IApiPortService
    {
        internal static class Endpoints
        {
            internal const string Analyze = "/api/analyze";
            internal const string Targets = "/api/target";
            internal const string UsedApi = "/api/usage";
            internal const string FxApi = "/api/fxapi";
            internal const string FxApiSearch = "/api/fxapi/search";
            internal const string ResultFormat = "/api/resultformat";
        }

        private readonly CompressedHttpClient _client;

        public ApiPortService(string endpoint, ProductInformation info)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new ArgumentOutOfRangeException(nameof(endpoint), endpoint, "Must be a valid endpoint");
            }

            _client = new CompressedHttpClient(info)
            {
                BaseAddress = new Uri(endpoint),
                Timeout = TimeSpan.FromMinutes(10)
            };
        }

        public async Task<ServiceResponse<AnalyzeResponse>> SendAnalysisAsync(AnalyzeRequest a)
        {
            return await _client.CallAsync<AnalyzeRequest, AnalyzeResponse>(HttpMethod.Post, Endpoints.Analyze, a);
        }

        public async Task<ServiceResponse<IEnumerable<ReportingResultWithFormat>>> SendAnalysisAsync(AnalyzeRequest a, IEnumerable<string> formats)
        {
            var requests = formats.Select(format => new
            {
                Task = SendAnalysisAsync(a, format),
                Format = format
            }).ToList();

            await Task.WhenAll(requests.Select(r => r.Task));

            var headers = requests[0].Task.Result.Headers;
            var result = requests.Select(r => new ReportingResultWithFormat
            {
                Data = r.Task.Result.Response,
                Format = r.Format
            }).ToList() as IEnumerable<ReportingResultWithFormat>;

            return ServiceResponse.Create(result, headers);
        }

        public async Task<ServiceResponse<byte[]>> SendAnalysisAsync(AnalyzeRequest a, string format)
        {
            var formatInformation = await GetResultFormat(format);

            return await _client.CallAsync(HttpMethod.Post, Endpoints.Analyze, a, formatInformation);
        }

        public async Task<ServiceResponse<IEnumerable<AvailableTarget>>> GetTargetsAsync()
        {
            return await _client.CallAsync<IEnumerable<AvailableTarget>>(HttpMethod.Get, Endpoints.Targets);
        }

        public async Task<ServiceResponse<UsageDataCollection>> GetUsageDataAsync(int? skip = null, int? top = null, UsageDataFilter? filter = null, IEnumerable<string> targets = null)
        {
            var usedApiUrl = UrlBuilder.Create(Endpoints.UsedApi)
                .AddQuery("skip", skip)
                .AddQuery("top", top)
                .AddQuery("filter", filter)
                .AddQueryList("targets", targets)
                .Url;

            return await _client.CallAsync<UsageDataCollection>(HttpMethod.Get, usedApiUrl);
        }

        public async Task<ServiceResponse<AnalyzeResponse>> GetAnalysisAsync(string submissionId)
        {
            var submissionUrl = UrlBuilder.Create(Endpoints.Analyze).AddPath(submissionId).Url;

            return await _client.CallAsync<AnalyzeResponse>(HttpMethod.Get, submissionUrl);
        }

        public async Task<ServiceResponse<byte[]>> GetAnalysisAsync(string submissionId, string format)
        {
            var formatInformation = await GetResultFormat(format);
            var submissionUrl = UrlBuilder.Create(Endpoints.Analyze).AddPath(submissionId).Url;

            return await _client.CallAsync(HttpMethod.Get, submissionUrl, formatInformation);
        }

        public async Task<ServiceResponse<ApiInformation>> GetApiInformationAsync(string docId)
        {
            string sendAnalysis = UrlBuilder
                .Create(Endpoints.FxApi)
                .AddQuery("docId", docId)
                .Url;

            return await _client.CallAsync<ApiInformation>(HttpMethod.Get, sendAnalysis);
        }

        public async Task<ServiceResponse<IReadOnlyCollection<ApiDefinition>>> SearchFxApiAsync(string query, int? top = null)
        {
            var url = UrlBuilder
                .Create(Endpoints.FxApiSearch)
                .AddQuery("q", query)
                .AddQuery("top", top);

            return await _client.CallAsync<IReadOnlyCollection<ApiDefinition>>(HttpMethod.Get, url.Url);
        }

        /// <summary>
        /// Returns a list of valid DocIds from the PortabilityService
        /// </summary>
        /// <param name="docIds">Enumerable of DocIds</param>
        public async Task<ServiceResponse<IReadOnlyCollection<ApiInformation>>> QueryDocIdsAsync(IEnumerable<string> docIds)
        {
            return await _client.CallAsync<IEnumerable<string>,
                IReadOnlyCollection<ApiInformation>>(HttpMethod.Post, Endpoints.FxApi, docIds);
        }

        public async Task<ServiceResponse<IEnumerable<ResultFormatInformation>>> GetResultFormatsAsync()
        {
            return await _client.CallAsync<IEnumerable<ResultFormatInformation>>(HttpMethod.Get, Endpoints.ResultFormat);
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        private async Task<ResultFormatInformation> GetResultFormat(string format)
        {
            var formats = await GetResultFormatsAsync();
            var formatInformation = formats.Response.FirstOrDefault(r => string.Equals(r.DisplayName, format, StringComparison.OrdinalIgnoreCase));

            if (formatInformation == null)
            {
                throw new UnknownReportFormatException(format);
            }

            return formatInformation;
        }
    }
}
