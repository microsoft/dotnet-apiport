// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using System.Collections.Generic;

namespace Microsoft.Fx.Portability.Analyzer
{
    public interface IDependencyInfo
    {
        IDictionary<MemberInfo, ICollection<AssemblyInfo>> Dependencies { get; }

        IEnumerable<string> AssembliesWithErrors { get; }

        IDictionary<string, ICollection<string>> UnresolvedAssemblies { get; }

        IEnumerable<AssemblyInfo> UserAssemblies { get; }
    }
}
