using System;
using System.Net;

namespace Microsoft.Fx.Portability.Proxy
{
    /// <summary>
    /// Coordinates the proxy settings and credentials
    /// </summary>
    public interface IProxyProvider
    {
        /// <summary>
        /// The credential provider to fetch credentials
        /// </summary>
        ICredentialProvider CredentialProvider { get; }

        /// <summary>
        /// The resolved proxy based on the destination Uri.
        /// </summary>
        IWebProxy GetProxy(Uri sourceUri);

        /// <summary>
        /// Updates the existing credentials in the proxy.
        /// </summary>
        void UpdateProxyCredentials(NetworkCredential credentials);
    }
}
