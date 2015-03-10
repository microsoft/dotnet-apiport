// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Resources;

namespace Microsoft.Fx.Portability
{
    public class UnknownReportFormatException : PortabilityAnalyzerException
    {
        public UnknownReportFormatException(string format)
            : base(string.Format(LocalizedStrings.UnknownResultFormat, format))
        { }
    }
}