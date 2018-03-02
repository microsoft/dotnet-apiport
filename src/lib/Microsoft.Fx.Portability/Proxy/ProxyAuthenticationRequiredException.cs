// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Resources;
using System;
using System.Globalization;
using System.Net;

namespace Microsoft.Fx.Portability
{
    /// <summary>
    /// Thrown when trying to call the <see cref="Endpoint"/> resulted in <see cref="HttpStatusCode.ProxyAuthenticationRequired"/>
    /// </summary>
    public class ProxyAuthenticationRequiredException : Exception
    {
        /// <summary>
        /// Endpoint that was accessed when thrown.
        /// </summary>
        public Uri Endpoint { get; }

        public ProxyAuthenticationRequiredException(Uri uri)
            : this(uri, null, null)
        { }

        public ProxyAuthenticationRequiredException(Uri uri, string message)
            : this(uri, message, null)
        { }

        public ProxyAuthenticationRequiredException(Uri uri, string message, Exception innerException)
            : base(message, innerException)
        {
            Endpoint = uri;
        }

        public override string ToString()
        {
            var formatted = string.Format(CultureInfo.CurrentCulture, LocalizedStrings.ProxyAuthenticationRequiredMessage, Endpoint);

            return formatted + Environment.NewLine + base.ToString();
        }
    }
}
