// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.Versioning;

namespace Microsoft.Fx.Portability.Reports
{
    public sealed class TargetSupportedIn
    {
        public Version SupportedIn { get; }

        public FrameworkName Target { get; }

        public TargetSupportedIn(FrameworkName target, Version supportedIn)
        {
            Target = target;
            SupportedIn = supportedIn;
        }
    }
}
