// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Fx.Portability.Proxy
{
    /// <summary>
    /// Looks for a proxy in the following order:
    /// 1. From a configuration file <see cref="ProxyConstants.ConfigurationFile"/>
    /// 2. From an environment variable <see cref="ProxyConstants.HttpProxy"/>
    /// 3. System provided proxy
    ///
    /// Looks for the one in the configuration file because the user would
    /// explicitly want to override their current proxy settings in order
    /// for the configuration value to be set.
    /// </summary>
    /// <remarks>
    /// Adapted from: https://github.com/NuGet/NuGet.Client/blob/dev/src/NuGet.Core/NuGet.Configuration/Proxy/ProxyCache.cs.
    /// </remarks>
    public class ProxyProvider : IProxyProvider
    {
        private readonly IConfigurationRoot _configuration;
        private readonly CredentialsWrapper _credentialsWrapper;
        private readonly ICredentialProvider _credentialProvider;

        public ProxyProvider(string configurationDirectory, string configurationFile, ICredentialProvider credentialProvider)
        {
            _configuration = new ConfigurationBuilder()
                 .SetBasePath(configurationDirectory)
                 .AddJsonFile(configurationFile, optional: true, reloadOnChange: true)
                 .Build();

            _credentialsWrapper = new CredentialsWrapper(CredentialCache.DefaultNetworkCredentials);
            _credentialProvider = credentialProvider;
        }

        public bool CanUpdateCredentials
        {
            get { return _credentialProvider != null; }
        }

        public IWebProxy GetProxy(Uri sourceUri)
        {
            // Check if the user has configured proxy details in settings or in the environment.
            var proxy = GetWebProxyFromConfiguration();

            if (proxy != null)
            {
                if (proxy.Credentials != null)
                {
                    _credentialsWrapper.UpdateCredentials(proxy.Credentials);
                }

                proxy.Credentials = _credentialsWrapper;
                return proxy;
            }

            return HttpClient.DefaultProxy;
        }

        public async Task<bool> TryUpdateCredentialsAsync(Uri uri, IWebProxy proxy, CredentialRequestType type, CancellationToken cancellationToken)
        {
            if (!CanUpdateCredentials)
            {
                return false;
            }

            var updatedCredentials = await _credentialProvider.GetCredentialsAsync(uri, proxy, type, cancellationToken);

            if (updatedCredentials != null)
            {
                _credentialsWrapper.UpdateCredentials(updatedCredentials);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Tries to create a proxy by reading values from <see cref="_configuration"/>.
        /// </summary>
        private IWebProxy GetWebProxyFromConfiguration()
        {
            string host;
            string isEnabledString = GetValueFromProxyConfiguration<string>(_configuration, ProxyConstants.IsEnabled);

            if (!bool.TryParse(isEnabledString, out var isEnabled) || !isEnabled)
            {
                return null;
            }

            // Try to get Proxy from config.json
            host = GetValueFromProxyConfiguration<string>(_configuration, ProxyConstants.Address);
            string username = GetValueFromProxyConfiguration<string>(_configuration, ProxyConstants.Username);
            string password = GetValueFromProxyConfiguration<string>(_configuration, ProxyConstants.Password);

            if (!string.IsNullOrEmpty(host))
            {
                var proxy = new ApiPort.Proxy.WebProxy(host);

                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    proxy.Credentials = new NetworkCredential(username, password);
                }

                return proxy;
            }
            else
            {
                return null;
            }
        }

        private static T GetValueFromProxyConfiguration<T>(IConfigurationRoot configuration, params string[] pathSegments)
            where T : class
        {
            var concatonatedWithProxy = new[] { ProxyConstants.ProxySection }.Concat(pathSegments);
            var key = ConfigurationPath.Combine(concatonatedWithProxy);

            return configuration.GetValue<T>(key);
        }

        /// <summary>
        /// Helper class that wraps NetworkCredentials so the proxy always
        /// has a reference to the same class.
        /// </summary>
        private class CredentialsWrapper : ICredentials
        {
            private ICredentials _currentCredentials;

            public CredentialsWrapper(NetworkCredential credentials)
            {
                _currentCredentials = credentials;
            }

            public NetworkCredential GetCredential(Uri uri, string authType)
            {
                return _currentCredentials?.GetCredential(uri, authType);
            }

            public void UpdateCredentials(ICredentials credential)
            {
                _currentCredentials = credential;
            }
        }
    }
}
