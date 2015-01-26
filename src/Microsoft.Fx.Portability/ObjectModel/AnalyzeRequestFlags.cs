// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.Fx.Portability.ObjectModel
{
    [Flags]
    public enum AnalyzeRequestFlags
    {
        None = 0x0,
        NoTelemetry = 0x1
    }
}
