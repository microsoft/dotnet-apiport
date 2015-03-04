// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Fx.Portability
{
    public enum BreakingChangeImpact
    {
        Minor,
        Major,
        Edge,
        Transparent,
        RetargetingMinor,
        RetargetingMajor,
        Unknown,
        EdgeRetargeting,
        MinorRetargeting
    }
}
