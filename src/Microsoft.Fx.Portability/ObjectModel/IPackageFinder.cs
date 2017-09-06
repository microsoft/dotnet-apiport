// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.Versioning;

namespace Microsoft.Fx.Portability.ObjectModel
{
    public interface IPackageFinder
    {
        /// <summary>
        /// Retrieves the list of possible NuGet packages that contain a given assembly.
        /// </summary>
        /// <param name="assemblyInfo">assembly identity</param>
        /// <param name="targets">framework supporrted by package.</param>
        /// <param name="packages">dictionary of framework/list of packages (packages found that support the given framework)</param>
        /// <returns>
        /// Returns true if packages exist for that assembly (for any target), false if there are no packages
        /// If 'true' is returned, but no packages in the list, it means the package is not supported on the given framework.
        /// If 'false' is returned, it means we don't have any info about that assembly.
        /// </returns>
        bool TryFindPackage(string assemblyInfo, IEnumerable<FrameworkName> targets, out ImmutableDictionary<FrameworkName, IEnumerable<NuGetPackageId>> packages);
    }
}
