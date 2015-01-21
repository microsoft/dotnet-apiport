// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;
using System.Runtime.Versioning;

namespace Microsoft.Fx.Portability.ObjectModel
{
    public class TargetInfo
    {
        public FrameworkName DisplayName { get; set; }

        public bool IsReleased { get; set; }

        [JsonIgnore]
        public string AreaPath { get; set; }

        public override string ToString()
        {
            return DisplayName.ToString();
        }
    }
}
