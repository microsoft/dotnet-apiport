// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Net.Http;

namespace Microsoft.Fx.Portability.Azure
{
    public class ReversedIdTokenValidator : IReportTokenValidator
    {
        public bool RequestHasValidToken(HttpRequestMessage request)
        {
            var authHeader = request.Headers.Authorization;
            if (authHeader == null || !authHeader.Scheme.Equals("Bearer", StringComparison.Ordinal))
            {
                return false;
            }

            var token = authHeader.Parameter;
            var submissionId = request.RequestUri.Segments.Last();
            var chars = submissionId.ToCharArray();
            Array.Reverse(chars);
            var expectedToken = new string(chars);

            return token.Equals(expectedToken, StringComparison.Ordinal);
        }
    }
}
