// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Resources;
using System;
using System.Net.Http;

namespace Microsoft.Fx.Portability
{
    public class NotFoundException : PortabilityAnalyzerException
    {
        public NotFoundException(HttpMethod method, Uri requestUri)
            : base(string.Format(LocalizedStrings.NotFoundException, method, requestUri))
        { }
    }
}
