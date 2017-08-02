// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Resources;
using System;
using System.Globalization;
using System.Net.Http;

namespace Microsoft.Fx.Portability
{
    public class NotFoundException : PortabilityAnalyzerException
    {
        public NotFoundException(HttpMethod method, Uri requestUri)
            : base(string.Format(CultureInfo.CurrentCulture, LocalizedStrings.NotFoundException, method, requestUri))
        { }
    }
}
