// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using Microsoft.Fx.Portability.Resources;

namespace Microsoft.Fx.Portability
{
    public class RequestTooLargeException : PortabilityAnalyzerException
    {
        public RequestTooLargeException(long contentLengthInBytes)
            : base(string.Format(CultureInfo.CurrentCulture, LocalizedStrings.RequestTooLargeMessage, contentLengthInBytes))
        {
        }
    }
}
