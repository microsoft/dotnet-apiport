// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.Fx.Portability.ObjectModel
{
    public interface IAnalyzeRequest
    {
        string Id { get; }
        string ApplicationName { get; }
        IDictionary<MemberInfo, ICollection<AssemblyInfo>> Dependencies { get; }
        AnalyzeRequestFlags RequestFlags { get; }
        IList<string> Targets { get; }
        ICollection<string> UnresolvedAssemblies { get; }
        ICollection<AssemblyInfo> UserAssemblies { get; }
        byte Version { get; }
    }
}
