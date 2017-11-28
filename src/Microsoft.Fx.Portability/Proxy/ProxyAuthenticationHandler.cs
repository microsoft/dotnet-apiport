// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Fx.Portability.Proxy
{
    /// <summary>
    /// DelegatingHandler that coordinates passing in credentials and fetching
    /// new credentials from user when authorization is required.
    /// </summary>
    /// <remarks>
    /// Taken from: https://github.com/NuGet/NuGet.Client/blob/dev/src/NuGet.Core/NuGet.Protocol.Core.v3/HttpSource/ProxyAuthenticationHandler.cs
    /// </remarks>
    public class ProxyAuthenticationHandler : DelegatingHandler
    {
        public static readonly int MaxAttempts = 3;
        private const string BasicAuthenticationType = "Basic";

        private readonly HttpClientHandler _clientHandler;
        private int _authRetries = 0;
        private readonly IProxyProvider _proxyProvider;

        public ProxyAuthenticationHandler(HttpClientHandler httpClientHandler, IProxyProvider proxyProvider)
            : base(httpClientHandler)
        {
            if (httpClientHandler == null)
            {
                throw new ArgumentNullException(nameof(httpClientHandler));
            }
            if (proxyProvider == null)
            {
                throw new ArgumentNullException(nameof(proxyProvider));
            }

            _clientHandler = httpClientHandler;
            _proxyProvider = proxyProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            int maxAttempts = 0;
            HttpResponseMessage response = default(HttpResponseMessage);

            while (maxAttempts < MaxAttempts)
            {
                try
                {
                    response = await base.SendAsync(request, cancellationToken);

                    if (response.StatusCode != HttpStatusCode.ProxyAuthenticationRequired)
                    {
                        return response;
                    }

                    if (_clientHandler.Proxy == null || !_proxyProvider.CanUpdateCredentials)
                    {
                        return response;
                    }

                    if (!await AcquireCredentialsAsync(request.RequestUri, cancellationToken))
                    {
                        return response;
                    }
                }
                catch (Exception ex)
                when (ProxyAuthenticationRequired(ex) && _clientHandler.Proxy != null && _proxyProvider.CanUpdateCredentials)
                {
                    if (!await AcquireCredentialsAsync(request.RequestUri, cancellationToken))
                    {
                        throw;
                    }
                }

                maxAttempts++;
            }

            return response;
        }

        private async Task<bool> AcquireCredentialsAsync(Uri requestUri, CancellationToken cancellationToken)
        {
            // Limit the number of retries
            _authRetries++;
            if (_authRetries >= MaxAttempts)
            {
                // user prompting no more
                return false;
            }

            var proxyAddress = _clientHandler.Proxy.GetProxy(requestUri);

            // prompt user for proxy credentials.
            // use the user provided credential to send the request again if it was successful.
            return await _proxyProvider.TryUpdateCredentialsAsync(proxyAddress, _clientHandler.Proxy, CredentialRequestType.Proxy, cancellationToken);
        }

#if FEATURE_NETCORE
        // Returns true if the cause of the exception is proxy authentication failure
        private static bool ProxyAuthenticationRequired(Exception ex)
        {
            return true;
        }
#else
        // Returns true if the cause of the exception is proxy authentication failure
        private static bool ProxyAuthenticationRequired(Exception ex)
        {
            if (ex is ProxyAuthenticationRequiredException)
            {
                return true;
            }

            var response = ExtractResponse(ex);
            return response?.StatusCode == HttpStatusCode.ProxyAuthenticationRequired;
        }

        private static HttpWebResponse ExtractResponse(Exception ex)
        {
            var webException = ex.InnerException as WebException;
            var response = webException?.Response as HttpWebResponse;
            return response;
        }
#endif
    }
}
