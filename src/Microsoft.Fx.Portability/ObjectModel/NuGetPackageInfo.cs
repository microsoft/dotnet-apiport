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

        public string AssemblyInfo { get; private set; }
        public FrameworkName Target { get; private set; }
        public ImmutableList<NuGetPackageId> SupportedPackages { get; private set; }

        public NuGetPackageInfo(string assemblyInfo, FrameworkName target, IEnumerable<NuGetPackageId> supportedPackages)
        {
            AssemblyInfo = assemblyInfo ?? throw new ArgumentNullException(nameof(assemblyInfo));
            Target = target ?? throw new ArgumentNullException(nameof(target));
            SupportedPackages = supportedPackages?.OrderBy(x => x.PackageId).ToImmutableList() ?? ImmutableList.Create<NuGetPackageId>();
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
                    && Target.Equals(other.Target)
                    && SupportedPackages.SequenceEqual(other.SupportedPackages);
            }
            return false;
        }

        public override int GetHashCode()
        {
            if (!_hashComputed)
            {
                _hashCode = (AssemblyInfo ?? string.Empty + Target?.FullName ?? string.Empty).GetHashCode();
                _hashComputed = true;
            }
            return _hashCode;
        }
    }
}
