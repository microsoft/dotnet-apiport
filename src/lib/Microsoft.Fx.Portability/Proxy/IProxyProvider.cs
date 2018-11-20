// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Fx.Portability.Proxy
{
    /// <summary>
    /// Coordinates the proxy settings and credentials
    /// </summary>
    public interface IProxyProvider
    {
        /// <summary>
        /// Gets a value indicating whether it can get updated credentials (if
        /// the existing ones for the proxy are not sufficient).
        /// </summary>
        bool CanUpdateCredentials { get; }

        /// <summary>
        /// The resolved proxy based on the destination Uri.
        /// </summary>
        IWebProxy GetProxy(Uri sourceUri);

        /// <summary>
        /// True if it was possible to update the credentials and false otherwise.
        /// </summary>
        Task<bool> TryUpdateCredentialsAsync(Uri uri, IWebProxy proxy, CredentialRequestType type, CancellationToken cancellationToken);
    }
}
