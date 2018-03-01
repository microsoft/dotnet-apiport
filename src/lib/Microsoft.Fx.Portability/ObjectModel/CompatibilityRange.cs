// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.Fx.Portability.ObjectModel
{
#if FEATURE_SERIALIZABLE
    [Serializable]
#endif
    public class CompatibilityRange
    {
        public string FrameworkName { get; set; }
        public Version StartVersion { get; set; }
        public Version EndVersion { get; set; }
        public Version LatestVersion { get; set; }
        public Version DefaultVersion { get; set; }
        public bool IsRetargeting { get; set; }
    }
}
