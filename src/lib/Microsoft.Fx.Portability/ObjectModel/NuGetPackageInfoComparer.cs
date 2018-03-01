// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.Fx.Portability.ObjectModel
{
    public class NuGetPackageInfoComparer : Comparer<NuGetPackageInfo>
    {
        public override int Compare(NuGetPackageInfo x, NuGetPackageInfo y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    return 0;
                }
                return -1;
            }
            else
            {
                if (y == null)
                {
                    return 1;
                }
                if (string.Equals(x.PackageId, y.PackageId, StringComparison.Ordinal))
                {
                    return string.Compare(x.AssemblyInfo, y.AssemblyInfo, StringComparison.Ordinal);
                }
                else
                {
                    return string.Compare(x.PackageId, y.PackageId, StringComparison.Ordinal);
                }
            }
        }
    }
}
