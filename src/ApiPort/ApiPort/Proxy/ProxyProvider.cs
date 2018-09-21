// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Net;
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
    /// Adapted from: https://github.com/NuGet/NuGet.Client/blob/dev/src/NuGet.Core/NuGet.Configuration/Proxy/ProxyCache.cs
    /// </remarks>
    public class ProxyProvider : IProxyProvider
    {
#if FEATURE_SYSTEM_PROXY
        /// <summary>
        /// Capture the default System Proxy so that it can be re-used.  Cannot rely on
        /// WebRequest.DefaultWebProxy since someone can modify the DefaultWebProxy
        /// property and we can't tell if it was modified and if we are still using System Proxy Settings or not.
        /// One limitation of this method is that it does not look at the config file to get the defined proxy
        /// settings.
        /// </summary>
        private static readonly IWebProxy _originalSystemProxy = WebRequest.GetSystemWebProxy();
#endif
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
            IWebProxy proxy = GetUserConfiguredProxy();

            if (proxy != null)
            {
                if (proxy.Credentials != null)
                {
                    _credentialsWrapper.UpdateCredentials(proxy.Credentials);
                }

                proxy.Credentials = _credentialsWrapper;
                return proxy;
            }

#if FEATURE_SYSTEM_PROXY
            if (IsSystemProxySet(sourceUri))
            {
                proxy = GetSystemProxy(sourceUri);

                if (proxy.Credentials != null)
                {
                    _credentialsWrapper.UpdateCredentials(proxy.Credentials);
                }

                proxy.Credentials = _credentialsWrapper;

                return proxy;
            }
#endif

            return null;
        }

        /// <summary>
        /// Tries to get user configured proxy in the following order:
        /// 1. Environment (configured in http_proxy, no proxy)
        /// 2. Configuration file passed in.
        /// </summary>
        /// <returns>A web proxy or null if none was found.</returns>
        private IWebProxy GetUserConfiguredProxy()
        {
            var proxy = GetWebProxyFromEnvironment();

            if (proxy != null)
            {
                return proxy;
            }

            return GetWebProxyFromConfiguration();
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
        /// Gets the proxy from environment variables.
        /// </summary>
        private static IWebProxy GetWebProxyFromEnvironment()
        {
            // Try reading from the environment variable http_proxy. This would be specified as http://<username>:<password>@proxy.com
            var host = Environment.GetEnvironmentVariable(ProxyConstants.HttpProxy);
            if (!string.IsNullOrEmpty(host)
                && Uri.TryCreate(host, UriKind.Absolute, out var uri))
            {
                var webProxy = new ApiPort.Proxy.WebProxy(uri.GetComponents(UriComponents.HttpRequestUrl, UriFormat.SafeUnescaped));

                if (!string.IsNullOrEmpty(uri.UserInfo))
                {
                    var credentials = uri.UserInfo.Split(':');

                    if (credentials.Length > 1)
                    {
                        webProxy.Credentials = new NetworkCredential(
                            userName: credentials[0],
                            password: credentials[1]);
                    }
                }

                var noProxy = Environment.GetEnvironmentVariable(ProxyConstants.NoProxy);
                if (!string.IsNullOrEmpty(noProxy))
                {
                    // split comma-separated list of domains
                    webProxy.BypassList = noProxy.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                }

                return webProxy;
            }
            else
            {
                return null;
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

#if FEATURE_SYSTEM_PROXY
        private static ApiPort.Proxy.WebProxy GetSystemProxy(Uri uri)
        {
            // WebRequest.DefaultWebProxy seems to be more capable in terms of getting the default
            // proxy settings instead of the WebRequest.GetSystemProxy()
            var proxyUri = _originalSystemProxy.GetProxy(uri);
            return new ApiPort.Proxy.WebProxy(proxyUri);
        }

        /// <summary>
        /// Return true or false if connecting through a proxy server
        /// </summary>
        private static bool IsSystemProxySet(Uri uri)
        {
            // The reason for not calling the GetSystemProxy is because the object
            // that will be returned is no longer going to be the proxy that is set by the settings
            // on the users machine only the Address is going to be the same.
            // Not sure why the .NET team did not want to expose all of the useful settings like
            // ByPass list and other settings that we can't get because of it.
            // Anyway the reason why we need the DefaultWebProxy is to see if the uri that we are
            // getting the proxy for to should be bypassed or not. If it should be bypassed then
            // return that we don't need a proxy and we should try to connect directly.
            var proxy = WebRequest.DefaultWebProxy;
            if (proxy != null)
            {
                var proxyUri = proxy.GetProxy(uri);
                if (proxyUri != null)
                {
                    var proxyAddress = new Uri(proxyUri.AbsoluteUri);
                    if (string.Equals(proxyAddress.AbsoluteUri, uri.AbsoluteUri, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                    return !proxy.IsBypassed(uri);
                }
            }

            return false;
        }
#endif

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
