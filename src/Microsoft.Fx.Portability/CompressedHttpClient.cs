// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Reporting.ObjectModel;
using Microsoft.Fx.Portability.Resources;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Microsoft.Fx.Portability
{
    internal class CompressedHttpClient : HttpClient
    {
        /// <param name="productName">Product name that will be displayed in the User Agent string of requests</param>
        /// <param name="productVersion">Product version that will be displayed in the User Agent string of requests</param>
        public CompressedHttpClient(ProductInformation info)
            : base(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate })
        {
            DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            DefaultRequestHeaders.AcceptLanguage.TryParseAdd(CultureInfo.CurrentCulture.ToString());

            DefaultRequestHeaders.Add("Client-Type", info.Name);
            DefaultRequestHeaders.Add("Client-Version", info.Version);
        }

        public async Task<ServiceResponse<TResponse>> CallAsync<TResponse>(HttpMethod method, string requestUri)
        {
            var request = new HttpRequestMessage(method, requestUri);

            return await CallInternalAsync<TResponse>(request);
        }

        public async Task<ServiceResponse<byte[]>> CallAsync(HttpMethod method, string requestUri, ResultFormatInformation format)
        {
            var request = new HttpRequestMessage(method, requestUri);

            return await CallInternalAsync(request, format);
        }

        public async Task<ServiceResponse<byte[]>> CallAsync<TRequest>(HttpMethod method, string requestUri, TRequest requestData, ResultFormatInformation format)
        {
#if SILVERLIGHT
            var content = requestData.Serialize();
#else
            var content = requestData.Serialize().Compress();
#endif

            var request = new HttpRequestMessage(method, requestUri)
            {
                Content = new ByteArrayContent(content)
            };

#if !SILVERLIGHT
            request.Content.Headers.ContentEncoding.Add("gzip");
#endif

            return await CallInternalAsync(request, format);
        }

        public async Task<ServiceResponse<TResponse>> CallAsync<TRequest, TResponse>(HttpMethod method, string requestUri, TRequest requestData)
        {
#if SILVERLIGHT
            var content = requestData.Serialize();
#else
            var content = requestData.Serialize().Compress();
#endif

            var request = new HttpRequestMessage(method, requestUri)
            {
                Content = new ByteArrayContent(content)
            };

#if !SILVERLIGHT
            request.Content.Headers.ContentEncoding.Add("gzip");
#endif

            return await CallInternalAsync<TResponse>(request);
        }

        private async Task ProcessBadRequestAsync(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            if (string.Equals(response.ReasonPhrase, typeof(UnknownTargetException).Name, StringComparison.Ordinal))
            {
                throw new UnknownTargetException(content);
            }

#if !SILVERLIGHT
            Trace.TraceError(string.Format("Unknown HttpStatusCode.BadRequest: {0} [{1}]", response.ReasonPhrase, content));
#endif

            throw new PortabilityAnalyzerException(LocalizedStrings.UnknownBadRequestMessage);
        }

        private async Task<ServiceResponse<TResponse>> CallInternalAsync<TResponse>(HttpRequestMessage request)
        {
            var response = await CallInternalAsync(request, ResultFormat.Json.ToResultFormatInformation());
            var result = response.Response.Deserialize<TResponse>();

            return new ServiceResponse<TResponse>(result, response.Headers);
        }

        private async Task<ServiceResponse<byte[]>> CallInternalAsync(HttpRequestMessage request, ResultFormatInformation format)
        {
            try
            {
                if (request.Content != null)
                {
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                }

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(format.MimeType));

                HttpResponseMessage response = await SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsByteArrayAsync();

                    return new ServiceResponse<byte[]>(data, response);
                }
                else
                {
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.BadRequest:
                            await ProcessBadRequestAsync(response);
                            break; // ProcessBadRequestAsync always throws but the compiler does not detect it
                        case HttpStatusCode.MovedPermanently:
                            throw new MovedPermanentlyException();
                        case HttpStatusCode.NotFound:
                            throw new NotFoundException();
                        case HttpStatusCode.Unauthorized:
                            throw new UnauthorizedEndpointException();
                    }

                    throw new PortabilityAnalyzerException(string.Format(CultureInfo.CurrentCulture, LocalizedStrings.UnknownErrorCodeMessage, response.StatusCode));
                }
            }
            catch (HttpRequestException e)
            {
                throw new PortabilityAnalyzerException(LocalizedStrings.HttpExceptionMessage, e);
            }
        }
    }
}
