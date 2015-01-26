// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reporting.ObjectModel;
using System;
using System.Collections.Generic;
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
        }

        private readonly CompressedHttpClient _client;
        private readonly string _endpoint;

        public ApiPortService(string endpoint, ProductInformation info)
        {
            _endpoint = endpoint;

            _client = new CompressedHttpClient(info)
            {
                Timeout = TimeSpan.FromMinutes(10)
            };
        }

        public async Task<ServiceResponse<AnalyzeResponse>> SendAnalysisAsync(AnalyzeRequest a)
        {
            string sendAnalysis = _endpoint + Endpoints.Analyze;

            return await _client.CallAsync<AnalyzeRequest, AnalyzeResponse>(HttpMethod.Post, sendAnalysis, a);
        }

        public async Task<ServiceResponse<byte[]>> SendAnalysisAsync(AnalyzeRequest a, ResultFormat format)
        {
            string sendAnalysis = _endpoint + Endpoints.Analyze;

            return await _client.CallAsync(HttpMethod.Post, sendAnalysis, a, format);
        }

        public async Task<ServiceResponse<IEnumerable<AvailableTarget>>> GetTargetsAsync()
        {
            string targetsUrl = _endpoint + Endpoints.Targets;

            return await _client.CallAsync<IEnumerable<AvailableTarget>>(HttpMethod.Get, targetsUrl);
        }

        public async Task<ServiceResponse<UsageDataCollection>> GetUsageDataAsync(int? skip = null, int? top = null, UsageDataFilter? filter = null, IEnumerable<string> targets = null)
        {
            var usedApiUrl = UrlBuilder.Create(_endpoint + Endpoints.UsedApi)
                .AddQuery("skip", skip)
                .AddQuery("top", top)
                .AddQuery("filter", filter)
                .AddQueryList("targets", targets)
                .Url;

            return await _client.CallAsync<UsageDataCollection>(HttpMethod.Get, usedApiUrl);
        }

        public async Task<ServiceResponse<AnalyzeResponse>> GetAnalysisAsync(string submissionId)
        {
            var submissionUrl = UrlBuilder.Create(_endpoint + Endpoints.Analyze).AddPath(submissionId).Url;

            return await _client.CallAsync<AnalyzeResponse>(HttpMethod.Get, submissionUrl);
        }

        public async Task<ServiceResponse<byte[]>> GetAnalysisAsync(string submissionId, ResultFormat format)
        {
            var submissionUrl = UrlBuilder.Create(_endpoint + Endpoints.Analyze).AddPath(submissionId).Url;

            return await _client.CallAsync(HttpMethod.Get, submissionUrl, format);
        }

        public async Task<ServiceResponse<ApiInformation>> GetApiInformationAsync(string docId)
        {
            string sendAnalysis = UrlBuilder
                .Create(_endpoint + Endpoints.FxApi)
                .AddQuery("docId", docId)
                .Url;

            return await _client.CallAsync<ApiInformation>(HttpMethod.Get, sendAnalysis);
        }

        public async Task<ServiceResponse<IReadOnlyCollection<ApiDefinition>>> SearchFxApiAsync(string query, int? top = null)
        {
            var url = UrlBuilder
                .Create(_endpoint + Endpoints.FxApiSearch)
                .AddQuery("q", query)
                .AddQuery("top", top);

            return await _client.CallAsync<IReadOnlyCollection<ApiDefinition>>(HttpMethod.Get, url.Url);
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
