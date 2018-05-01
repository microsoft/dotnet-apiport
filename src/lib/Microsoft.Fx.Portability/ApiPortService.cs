// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Proxy;
using Microsoft.Fx.Portability.Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
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
            internal const string ResultFormat = "/api/reportformat";
            internal const string DefaultResultFormat = "/api/reportformat/default";
        }

        private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(5);
        private readonly IProgressReporter _progressReporter;
        private readonly HttpClient _client;

        public ApiPortService(string endpoint, ProductInformation info, IProgressReporter reporter, IProxyProvider proxyProvider = null)
            : this(endpoint, BuildMessageHandler(endpoint, proxyProvider), info, reporter)
        { }

        public ApiPortService(string endpoint, HttpMessageHandler httpMessageHandler, ProductInformation info, IProgressReporter reporter)
        {
            _progressReporter = reporter;
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new ArgumentOutOfRangeException(nameof(endpoint), endpoint, LocalizedStrings.MustBeValidEndpoint);
            }

            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            _client = new HttpClient(httpMessageHandler, true)
            {
                BaseAddress = new Uri(endpoint),
                Timeout = Timeout
            };

            _client.DefaultRequestHeaders.AcceptLanguage.TryParseAdd(CultureInfo.CurrentCulture.ToString());
            _client.DefaultRequestHeaders.Add("Client-Type", info.Name);
            _client.DefaultRequestHeaders.Add("Client-Version", info.Version);
        }

        public Task<ApiInformation> GetApiInformationAsync(string docId)
        {
            throw new NotImplementedException();
        }

        public async Task<ResultFormatInformation> GetDefaultResultFormatAsync()
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, Endpoints.DefaultResultFormat))
            {
                var bytes = await SendAsync(request);

                return bytes.Deserialize<ResultFormatInformation>();
            }
        }

        public async Task<IEnumerable<ResultFormatInformation>> GetResultFormatsAsync()
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, Endpoints.ResultFormat))
            {
                var bytes = await SendAsync(request);

                return bytes.Deserialize<IEnumerable<ResultFormatInformation>>();
            }
        }

        public async Task<IEnumerable<AvailableTarget>> GetTargetsAsync()
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, Endpoints.Targets))
            {
                var bytes = await SendAsync(request);

                return bytes.Deserialize<IEnumerable<AvailableTarget>>();
            }
        }

        public Task<IReadOnlyCollection<ApiInformation>> QueryDocIdsAsync(IEnumerable<string> docIds)
        {
            throw new NotImplementedException();
        }

        public async Task<AnalyzeResponse> RequestAnalysisAsync(AnalyzeRequest analyzeRequest)
        {
            var content = analyzeRequest.SerializeAndCompress();

            using (var request = new HttpRequestMessage(HttpMethod.Post, Endpoints.Analyze))
            {
                request.Content = new ByteArrayContent(content);
                request.Content.Headers.ContentEncoding.Add("gzip");
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                request.Headers.Add("Accept", "application/json");

                var response = await SendAsync(request);

                return response.Deserialize<AnalyzeResponse>();
            }
        }

        public async Task<ReportingResultWithFormat> GetReportingResultAsync(AnalyzeResponse analyzeResponse, ResultFormatInformation format)
        {
            var deadline = DateTime.Now + Timeout;
            while (DateTime.Now < deadline)
            {
                var (retryDelta, reportingResult) = await GetReportAsync(analyzeResponse, format);
                if (reportingResult.HasValue)
                {
                    return reportingResult.Value;
                }

                if (!retryDelta.HasValue)
                {
                    throw new PortabilityAnalyzerException(LocalizedStrings.HttpExceptionMessage);
                }

                await Task.Delay(retryDelta.Value);
            }

            throw new TimeoutException(LocalizedStrings.TimedOut);
        }

        private async Task<(TimeSpan? retryDelta, ReportingResultWithFormat? result)> GetReportAsync(AnalyzeResponse analyzeResponse, ResultFormatInformation format)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, analyzeResponse.ResultUrl.ToString()))
            {
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(format.MimeType));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", analyzeResponse.ResultAuthToken);

                using (var response = await _client.SendAsync(request))
                {
                    ReportEndpointDeprecation(response);

                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.Accepted:
                            var retryDelta = response.Headers.RetryAfter.Delta;
                            return (retryDelta, null);
                        case HttpStatusCode.OK:
                            var data = await response.Content.ReadAsByteArrayAsync();
                            var result = new ReportingResultWithFormat
                            {
                                Data = data,
                                Format = format.DisplayName
                            };
                            return (null, result);
                    }

                    await ThrowErrorCodeExceptionAsync(request, response);
                    
                    // the compiler doesn't know ThrowErrorCodeExceptionAsync always throws
                    throw new PortabilityAnalyzerException(string.Format(CultureInfo.CurrentCulture, LocalizedStrings.UnknownErrorCodeMessage, response.StatusCode));
                }
            }
        }

        private async Task<byte[]> SendAsync(HttpRequestMessage request)
        {
            using (var response = await _client.SendAsync(request))
            {
                ReportEndpointDeprecation(response);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsByteArrayAsync();
                }
                else
                {
                    await ThrowErrorCodeExceptionAsync(request, response);

                    // the compiler doesn't know ThrowErrorCodeExceptionAsync always throws
                    throw new PortabilityAnalyzerException(string.Format(CultureInfo.CurrentCulture, LocalizedStrings.UnknownErrorCodeMessage, response.StatusCode));
                }
            }
        }

        private void ReportEndpointDeprecation(HttpResponseMessage response)
        {
            if (response.Headers.TryGetValues(typeof(EndpointStatus).Name, out var headers) &&
                Enum.TryParse(headers.Single(), out EndpointStatus status) &&
                status == EndpointStatus.Deprecated)
            {
                _progressReporter.ReportIssue(LocalizedStrings.ServerEndpointDeprecated);
            }
        }

        private static async Task ThrowErrorCodeExceptionAsync(HttpRequestMessage request, HttpResponseMessage response)
        {
            switch (response.StatusCode)
            {
                case HttpStatusCode.BadRequest:
                    await ProcessBadRequestAsync(response);
                    break; // ProcessBadRequestAsync always throws but the compiler does not detect it
                case HttpStatusCode.MovedPermanently:
                    throw new MovedPermanentlyException();
                case HttpStatusCode.NotFound:
                    // Estimated maximum allowed content from the portability service in bytes
                    const long estimatedMaximumAllowedContentLength = 31457280;

                    var contentLength = request.Content?.Headers?.ContentLength;

                    if (contentLength.HasValue && contentLength.Value >= estimatedMaximumAllowedContentLength)
                    {
                        throw new RequestTooLargeException(contentLength.Value);
                    }
                    else
                    {
                        throw new NotFoundException(request.Method, request.RequestUri);
                    }
                case HttpStatusCode.Unauthorized:
                    throw new UnauthorizedEndpointException();
                case HttpStatusCode.ProxyAuthenticationRequired:
                    throw new ProxyAuthenticationRequiredException(request.RequestUri);
            }

            throw new PortabilityAnalyzerException(string.Format(CultureInfo.CurrentCulture, LocalizedStrings.UnknownErrorCodeMessage, response.StatusCode));
        }

        private static async Task ProcessBadRequestAsync(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            if (string.Equals(response.ReasonPhrase, typeof(UnknownTargetException).Name, StringComparison.Ordinal))
            {
                throw new UnknownTargetException(content);
            }

            throw new PortabilityAnalyzerException(LocalizedStrings.BadRequestMessage);
        }

        public Task<IReadOnlyCollection<ApiDefinition>> SearchFxApiAsync(string query, int? top = null)
        {
            throw new NotImplementedException();
        }

        private static HttpMessageHandler BuildMessageHandler(string endpoint, IProxyProvider proxyProvider)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new ArgumentOutOfRangeException(nameof(endpoint), endpoint, LocalizedStrings.MustBeValidEndpoint);
            }

            // Create the URI directly from a string (rather than using a hard-coded scheme or port) because 
            // even though production use of ApiPort should always use HTTPS, developers using a non-production
            // portability service URL (via the -e command line parameter) may need to specify a different 
            // scheme or port.
            var uri = new Uri(endpoint);

            var clientHandler = new HttpClientHandler
            {
                Proxy = proxyProvider?.GetProxy(uri),
                AutomaticDecompression = (DecompressionMethods.GZip | DecompressionMethods.Deflate)
            };

#if FEATURE_SERVICE_POINT_MANAGER
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
#else
            clientHandler.SslProtocols = SslProtocols.Tls12;
#endif
            if (clientHandler.Proxy == null)
            {
                return clientHandler;
            }

            return new ProxyAuthenticationHandler(clientHandler, proxyProvider);
        }

        public void Dispose() => _client.Dispose();
    }
}
