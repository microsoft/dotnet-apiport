// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Fx.Portability.ObjectModel
{
    public class IgnoreAssemblyInfo : IEquatable<IgnoreAssemblyInfo>
    {
        public string AssemblyIdentity { get; set; }

        public IEnumerable<string> TargetsIgnored { get; set; }

        // If specific targets to ignore for aren't specified, then assume the assembly should be ignored everywhere
        public bool IgnoreForAllTargets { get { return TargetsIgnored == null || TargetsIgnored.Count() == 0; } }

        public bool Equals(IgnoreAssemblyInfo other)
        {
            if (other == null)
            {
                return false;
            }

            if (!AssemblyIdentity.Equals(other.AssemblyIdentity, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (TargetsIgnored == null || TargetsIgnored.Count() == 0)
            {
                if (other.TargetsIgnored != null && other.TargetsIgnored.Count() > 0)
                {
                    return false;
                }
            }
            else
            {
                // We could just look to see that all of the targets ignored by either object are ignored by the other, but this way
                // will prevent duplicates which is desirable since the merge method should make it easy for user to not have duplicate targets
                if (TargetsIgnored.Count() != (other.TargetsIgnored == null ? 0 : other.TargetsIgnored.Count()))
                {
                    return false;
                }

                var sortedTargetsEnum = TargetsIgnored.OrderBy(s => s).GetEnumerator();
                var sortedOtherTargetsEnum = other.TargetsIgnored.OrderBy(s => s).GetEnumerator();
                while (sortedTargetsEnum.MoveNext() && sortedOtherTargetsEnum.MoveNext())
                {
                    if (!sortedTargetsEnum.Current.Equals(sortedOtherTargetsEnum.Current, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }

    public class IgnoreAssemblyInfoComparer : IEqualityComparer<IgnoreAssemblyInfo>
    {
        public bool Equals(IgnoreAssemblyInfo x, IgnoreAssemblyInfo y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(IgnoreAssemblyInfo x)
        {
            int ret = x.AssemblyIdentity.GetHashCode();
            foreach (string ver in x.TargetsIgnored)
            {
                ret ^= ver.GetHashCode();
            }

            return ret;
        }
    }
}
