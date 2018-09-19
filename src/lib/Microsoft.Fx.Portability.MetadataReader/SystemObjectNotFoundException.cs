// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Analyzer.Resources;

namespace Microsoft.Fx.Portability.Analyzer
{
    /// <summary>
    /// Exception thrown when assembly containing <see cref="object"/>
    /// cannot be found.
    /// </summary>
    public class SystemObjectNotFoundException : PortabilityAnalyzerException
    {
        public SystemObjectNotFoundException()
            : base(LocalizedStrings.MissingSystemObjectAssembly)
        {
        }
    }
}
