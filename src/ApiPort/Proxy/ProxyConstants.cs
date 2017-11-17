// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Fx.Portability.Proxy
{
    /// <summary>
    /// Constants used to fetch proxy settings.
    /// </summary>
    internal class ProxyConstants
    {
        /// Configuration keys for config.json
        public const string ProxySection = "proxy";
        public const string Username = nameof(Username);
        public const string Address = nameof(Address);
        public const string Password = nameof(Password);
        public const string IsEnabled = nameof(IsEnabled);

        /// <summary>
        /// Environment variable key to specify the value to use as the HTTP proxy
        /// for all connections.For example, HTTP_PROXY="http://proxy.mycompany.com:8080/"
        /// </summary>
        /// <remarks>
        /// https://msdn.microsoft.com/en-us/library/hh272656(v=vs.120).aspx
        /// </remarks>
        public const string HttpProxy = "http_proxy";

        /// <summary>
        /// Environment variable key to determine hosts that should bypass the
        /// proxy. For example, NO_PROXY="localhost,.mycompany.com,192.168.0.10:80"
        /// </summary>
        /// <remarks>
        /// https://msdn.microsoft.com/en-us/library/hh272656(v=vs.120).aspx
        /// </remarks>
        public const string NoProxy = "no_proxy";
    }
}
