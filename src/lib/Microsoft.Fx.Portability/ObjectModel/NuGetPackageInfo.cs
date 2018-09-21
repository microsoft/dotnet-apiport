// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Versioning;

namespace Microsoft.Fx.Portability.ObjectModel
{
    public class NuGetPackageInfo
    {
        private bool _hashComputed;
        private int _hashCode;

        public string PackageId { get; private set; }

        // Dictionary of Framework/version of this package that is supported on that framework
        public ImmutableDictionary<FrameworkName, string> SupportedVersions { get; private set; }

        // AssemblyInfo is optional; it is present only when the package Id is "guessed" from the assembly name
        public string AssemblyInfo { get; private set; }

        private static readonly HashSet<string> ImplicitlyReferencedPackages = new HashSet<string>(new[] { "Microsoft.NETCore.App", "NETStandard.Library" }, StringComparer.OrdinalIgnoreCase);

        public NuGetPackageInfo(string packageId, IDictionary<FrameworkName, string> supportedVersions, string assemblyInfo = null)
        {
            PackageId = packageId ?? throw new ArgumentNullException(nameof(packageId));
            if (string.IsNullOrWhiteSpace(PackageId))
            {
                throw new ArgumentException(nameof(packageId));
            }

            SupportedVersions = supportedVersions?.OrderBy(x => x.Key.FullName).ToImmutableDictionary() ?? ImmutableDictionary.Create<FrameworkName, string>();
            AssemblyInfo = assemblyInfo;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj is NuGetPackageInfo other)
            {
                return string.Equals(other.AssemblyInfo, AssemblyInfo, StringComparison.Ordinal)
                    && string.Equals(other.PackageId, PackageId, StringComparison.Ordinal)
                    && SupportedVersions.SequenceEqual(other.SupportedVersions, new SupportedVersionsComparer());
            }

            return false;
        }

        public override int GetHashCode()
        {
            if (!_hashComputed)
            {
                var hash = 17;
                hash = (hash * 23) + (AssemblyInfo ?? string.Empty).GetHashCode();
                hash = (hash * 23) + (PackageId ?? string.Empty).GetHashCode();
                _hashCode = hash;
                _hashComputed = true;
            }

            return _hashCode;
        }

        public static bool IsImplicitlyReferencedPackage(string packageId)
        {
            return ImplicitlyReferencedPackages.Contains(packageId);
        }

        private class SupportedVersionsComparer : IEqualityComparer<KeyValuePair<FrameworkName, string>>
        {
            public bool Equals(KeyValuePair<FrameworkName, string> x, KeyValuePair<FrameworkName, string> y)
            {
                return x.Key.Equals(y.Key) && string.Equals(x.Value, y.Value, StringComparison.Ordinal);
            }

            public int GetHashCode(KeyValuePair<FrameworkName, string> kvp)
            {
                var hash = 17;
                hash = (hash * 23) + kvp.Key.GetHashCode();
                hash = (hash * 23) + (kvp.Value ?? string.Empty).GetHashCode();
                return hash;
            }
        }
    }
}
