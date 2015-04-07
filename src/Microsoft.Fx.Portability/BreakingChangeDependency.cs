// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using System;

namespace Microsoft.Fx.Portability
{
    public class BreakingChangeDependency : IEquatable<BreakingChangeDependency>
    {
        public MemberInfo Member { get; set; }
        public BreakingChange Break { get; set; }
        public AssemblyInfo DependantAssembly{ get; set; }

        public bool Equals(BreakingChangeDependency other)
        {
            return (Member.Equals(other.Member) && (Break.CompareTo(other.Break) == 0) && DependantAssembly.Equals(other.DependantAssembly));
        }
    }
}
