// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.Versioning;

namespace Microsoft.Fx.Portability.ObjectModel
{
    public class OfflinePackageFinder : IPackageFinder
    {
        public bool TryFindPackage(string assemblyInfo, IEnumerable<FrameworkName> targets, out ImmutableList<NuGetPackageInfo> packages)
        {
            packages = ImmutableList<NuGetPackageInfo>.Empty;
            return false;
        }

        public bool TryFindSupportedVersions(string package, IEnumerable<FrameworkName> targets, out ImmutableDictionary<FrameworkName, string> versions)
        {
            versions = ImmutableDictionary<FrameworkName, string>.Empty;
            return false;
        }
    }
}