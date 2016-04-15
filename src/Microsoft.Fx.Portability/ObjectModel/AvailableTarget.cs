// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Fx.Portability.ObjectModel
{
    public class AvailableTarget
    {
        public AvailableTarget()
        {
            ExpandedTargets = Enumerable.Empty<string>();
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public Version Version { get; set; }
        public IEnumerable<string> ExpandedTargets { get; set; }
        public bool IsSet { get; set; }
    }
}
