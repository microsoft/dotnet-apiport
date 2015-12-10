// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Runtime.Versioning;

namespace Microsoft.Fx.Portability
{
    public interface ITargetMapper
    {
        string GetAlias(string targetName);
        ICollection<string> GetNames(string aliasName);
        ICollection<string> Aliases { get; }
        IEnumerable<string> GetTargetNames(IEnumerable<FrameworkName> targets, bool alwaysIncludeVersion = false);
    }
}