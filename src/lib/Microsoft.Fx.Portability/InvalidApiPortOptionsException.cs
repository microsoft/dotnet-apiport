// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Fx.Portability
{
    /// <summary>
    /// Thrown when <see cref="IApiPortOptions"/> is invalid.
    /// </summary>
    public class InvalidApiPortOptionsException : PortabilityAnalyzerException
    {
        public InvalidApiPortOptionsException(string message)
            : base(message)
        { }
    }
}
