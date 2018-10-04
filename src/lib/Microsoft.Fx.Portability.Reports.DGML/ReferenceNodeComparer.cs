// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Collections.Generic;

namespace Microsoft.Fx.Portability.Reports.DGML
{
    internal class ReferenceNodeComparer : IEqualityComparer<ReferenceNode>
    {
        public static ReferenceNodeComparer Instance { get; } = new ReferenceNodeComparer();

        public bool Equals(ReferenceNode x, ReferenceNode y)
        {
            return x.Assembly == y.Assembly;
        }

        public int GetHashCode(ReferenceNode obj)
        {
            return obj.Assembly.GetHashCode();
        }
    }
}
