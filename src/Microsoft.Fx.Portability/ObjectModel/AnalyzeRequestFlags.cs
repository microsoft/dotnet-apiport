// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.Fx.Portability.ObjectModel
{
    [Flags]
    public enum AnalyzeRequestFlags
    {
        None = 0,
        NoTelemetry = 1,
        ShowNonPortableApis = 1 << 1,
        ShowBreakingChanges = 1 << 2,
        NoDefaultIgnoreFile = 1 << 3,
        ShowRetargettingIssues = 1 << 4,
    }
}
