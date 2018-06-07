// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace ApiPort.Proxy
{
    /// <summary>
    /// Internal implementation of <see cref="IWebProxy"/> mirroring default desktop one.
    /// </summary>
    /// <remarks>Remove when WebProxy is brought to .NET Standard 2.0.</remarks>
    public class WebProxy : IWebProxy
    {
        private IReadOnlyList<string> _bypassList = new string[] { };
        private Regex[] _regExBypassList; // can be null

        public WebProxy(string proxyAddress)
        {
            if (proxyAddress == null)
            {
                throw new ArgumentNullException(nameof(proxyAddress));
            }

            ProxyAddress = CreateProxyUri(proxyAddress);
        }

        public WebProxy(Uri proxyAddress)
        {
            ProxyAddress = proxyAddress ?? throw new ArgumentNullException(nameof(proxyAddress));
        }

        public Uri ProxyAddress { get; }

        public ICredentials Credentials { get; set; }

        public IReadOnlyList<string> BypassList
        {
            get
            {
                return _bypassList;
            }
            set
            {
                _bypassList = value ?? new string[] { };
                UpdateRegExList();
            }
        }

        public Uri GetProxy(Uri destination) => ProxyAddress;

        public bool IsBypassed(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            if (_regExBypassList != null && _regExBypassList.Length > 0)
            {
                var normalizedUri = uri.Scheme + "://" + uri.Host + ((!uri.IsDefaultPort) ? (":" + uri.Port) : "");
                return _regExBypassList.Any(r => r.IsMatch(normalizedUri));
            }

            return false;
        }

        private void UpdateRegExList()
        {
            _regExBypassList = _bypassList?
                .Select(x => WildcardToRegex(x))
                .Select(x => new Regex(x, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                .ToArray();
        }

        private static string WildcardToRegex(string pattern)
        {
            string Replace(string content, string oldValue, string newValue)
            {
#if NETCOREAPP2_1
                return content.Replace(oldValue, newValue, StringComparison.Ordinal);
#else
                return content.Replace(oldValue, newValue);
#endif
            };

            return Replace(
                    Replace(Regex.Escape(pattern), @"\*", ".*?"),
                    @"\?",
                    ".");
        }

        private static Uri CreateProxyUri(string address)
        {
            if (address == null)
            {
                return null;
            }

            if (address.IndexOf("://", StringComparison.Ordinal) == -1)
            {
                address = "http://" + address;
            }

            return new Uri(address);
        }
    }
}
