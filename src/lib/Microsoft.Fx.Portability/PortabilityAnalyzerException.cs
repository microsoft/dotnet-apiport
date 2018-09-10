// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.Fx.Portability
{
    public class PortabilityAnalyzerException : Exception
    {
        public PortabilityAnalyzerException(string message)
            : base(message)
        {
        }

        public PortabilityAnalyzerException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
