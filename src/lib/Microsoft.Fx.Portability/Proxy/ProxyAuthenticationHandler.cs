// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Resources;
using System;
using System.Globalization;
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
        private const string BasicAuthenticationType = "Basic";
        public static readonly int MaxAttempts = 3;

        private readonly HttpClientHandler _clientHandler;
        private readonly IProxyProvider _proxyProvider;

        public ProxyAuthenticationHandler(HttpClientHandler httpClientHandler, IProxyProvider proxyProvider)
            : base(httpClientHandler)
        {
            _clientHandler = httpClientHandler ?? throw new ArgumentNullException(nameof(httpClientHandler));
            _proxyProvider = proxyProvider ?? throw new ArgumentNullException(nameof(proxyProvider));
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            int _attempts = 0;

            while (_attempts < MaxAttempts)
            {
                try
                {
                    return await base.SendAsync(request, cancellationToken);
                }
                catch (Exception ex) when (ProxyAuthenticationRequired(ex))
                {
                    var proxyAddress = _clientHandler.Proxy.GetProxy(request.RequestUri);

                    // prompt user for proxy credentials.
                    // use the user provided credential to send the request again if it was successful.
                    if (!await _proxyProvider.TryUpdateCredentialsAsync(proxyAddress, _clientHandler.Proxy, CredentialRequestType.Proxy, cancellationToken).ConfigureAwait(false))
                    {
                        throw;
                    }
                }

                _attempts++;
            }

            throw new PortabilityAnalyzerException(string.Format(CultureInfo.CurrentCulture, LocalizedStrings.UnknownErrorCodeMessage, HttpStatusCode.BadRequest));
        }

        // Returns true if the cause of the exception is proxy authentication failure
        private bool ProxyAuthenticationRequired(Exception ex)
        {
            if (!_proxyProvider.CanUpdateCredentials)
            {
                return false;
            }

            if (ex is ProxyAuthenticationRequiredException)
            {
                return true;
            }

#if FEATURE_WEBEXCEPTION
            if (ex.InnerException is WebException webException && webException.Response is HttpWebResponse response)
            {
                return response?.StatusCode == HttpStatusCode.ProxyAuthenticationRequired;
            }
#endif

            return false;
        }
    }
}
