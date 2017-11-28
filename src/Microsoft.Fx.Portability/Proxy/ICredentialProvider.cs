// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Fx.Portability.Proxy
{
    /// <summary>
    /// Provides NetworkCredentials for a given address and proxy.
    /// </summary>
    public interface ICredentialProvider
    {
        /// <summary>
        /// Fetches credentials for the given uri and proxy.
        /// </summary>
        Task<NetworkCredential> GetCredentialsAsync(Uri uri, IWebProxy proxy, CredentialRequestType type, CancellationToken cancellationToken);
    }
}
