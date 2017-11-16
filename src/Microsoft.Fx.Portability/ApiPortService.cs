// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
            internal const string DefaultResultFormat = "/api/resultformat/default";
        }

        const string UriScheme = "https";

        private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(10);
        private readonly CompressedHttpClient _client;

        public ApiPortService(string endpoint, ProductInformation info, IProxyProvider proxyProvider)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new ArgumentOutOfRangeException(nameof(endpoint), endpoint, "Must be a valid endpoint");
            }
            if (proxyProvider == null)
            {
                throw new ArgumentNullException(nameof(proxyProvider));
            }

            var uri = new UriBuilder(UriScheme, endpoint).Uri;
            var proxy = proxyProvider.GetProxy(uri);
            
            // replace the handler with the proxy aware handler
            var clientHandler = new HttpClientHandler
            {
#if FEATURE_CLIENT_HANDLER_SET_SSL_PROTOCOLS
                SslProtocols = CompressedHttpClient.SupportedSSLProtocols,
#endif
                Proxy = proxy,
                AutomaticDecompression = (DecompressionMethods.GZip | DecompressionMethods.Deflate)
            };

            // HTTP handler pipeline can be injected here, around the client handler
            HttpMessageHandler messageHandler = clientHandler;

            if (proxy != null)
            {
                messageHandler = new ProxyAuthenticationHandler(clientHandler, proxyProvider);
            }

            _client = new CompressedHttpClient(info, messageHandler)
            {
                BaseAddress = new UriBuilder(UriScheme, endpoint).Uri,
                Timeout = Timeout
            };
        }

        public ApiPortService(string endpoint, ProductInformation info)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new ArgumentOutOfRangeException(nameof(endpoint), endpoint, "Must be a valid endpoint");
            }

            _client = new CompressedHttpClient(info)
            {
                BaseAddress = new UriBuilder(UriScheme, endpoint).Uri,
                Timeout = Timeout
            };
        }

        public ApiPortService(string endpoint, HttpMessageHandler httpMessageHandler, ProductInformation info)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new ArgumentOutOfRangeException(nameof(endpoint), endpoint, "Must be a valid endpoint");
            }

            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            _client = new CompressedHttpClient(info, httpMessageHandler)
            {
                BaseAddress = new UriBuilder(UriScheme, endpoint).Uri,
                Timeout = Timeout
            };
        }

        public async Task<ServiceResponse<AnalyzeResponse>> SendAnalysisAsync(AnalyzeRequest a)
        {
            return await _client.CallAsync<AnalyzeRequest, AnalyzeResponse>(HttpMethod.Post, Endpoints.Analyze, a);
        }

        public async Task<ServiceResponse<IEnumerable<ReportingResultWithFormat>>> SendAnalysisAsync(AnalyzeRequest a, IEnumerable<string> format)
        {
            var formatInformation = await GetResultFormatsAsync(format);

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

        public async Task<ServiceResponse<ReportingResultWithFormat>> GetAnalysisAsync(string submissionId, string format)
        {
            var formatInformation = await GetResultFormatsAsync(string.IsNullOrWhiteSpace(format) ? null : new[] { format });
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

        public async Task<ServiceResponse<ResultFormatInformation>> GetDefaultResultFormatAsync()
        {
            return await _client.CallAsync<ResultFormatInformation>(HttpMethod.Get, Endpoints.DefaultResultFormat);
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        private async Task<IEnumerable<ResultFormatInformation>> GetResultFormatsAsync(IEnumerable<string> formats)
        {
            if (!formats?.Any() ?? true)   //no "resultFormat" string option provider by user
            {
                var defaultFormat = await GetDefaultResultFormatAsync();
                return new[] { defaultFormat.Response };
            }
            else
            {
                var requestedFormats = new HashSet<string>(formats, StringComparer.OrdinalIgnoreCase);
                var resultFormats = await GetResultFormatsAsync();
                var formatInformation = resultFormats.Response
                    .Where(r => requestedFormats.Contains(r.DisplayName));

                var unknownFormats = requestedFormats
                    .Except(formatInformation.Select(f => f.DisplayName), StringComparer.OrdinalIgnoreCase);   

                if (unknownFormats.Any())
                {
                    throw new UnknownReportFormatException(unknownFormats);
                }

                return formatInformation;
            }
        }
    }
}
