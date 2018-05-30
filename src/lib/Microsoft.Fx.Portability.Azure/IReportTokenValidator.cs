// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net.Http;

namespace Microsoft.Fx.Portability.Azure
{
    public interface IReportTokenValidator
    {
        bool RequestHasValidToken(HttpRequestMessage req);
    }
}
