// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;

namespace Microsoft.Fx.Portability.Reports
{
    public sealed class TargetSupportedIn
    {
        public readonly Version SupportedIn;
        
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes",
            Justification = "This is a false positive. FrameworkName is an immutable class.")]
        public readonly FrameworkName Target;

        public TargetSupportedIn(FrameworkName target, Version supportedIn)
        {
            Target = target;
            SupportedIn = supportedIn;
        }
    }
}
