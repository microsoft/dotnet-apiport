// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Resources;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Microsoft.Fx.Portability.ObjectModel
{
    public sealed class AssemblyInfo : IComparable
    {
        // BUG: There is the potential for a race in the management of this. The complexity is also unlikely to be worth the small perf gain
        private bool _hashComputed;
        private int _hashCode;

        private string _assemblyIdentity;
        private string _targetFrameworkVersion;
        private string _fileVersion;
        private string _name;

        public string AssemblyIdentity
        {
            get
            {
                return _assemblyIdentity;
            }

            set
            {
                _assemblyIdentity = value;
                _hashComputed = false;
            }
        }

        public string GetAssemblyName()
        {
            if (_name is null)
            {
                _name = new AssemblyName(AssemblyIdentity).Name;
            }

            return _name;
        }

        /// <summary>
        /// Gets or sets the assembly location.
        /// </summary>
        /// <remarks>Do not serialize location and send it to service.</remarks>
        [JsonIgnore]
        public string Location { get; set; }

        public string FileVersion
        {
            get
            {
                return _fileVersion;
            }

            set
            {
                _fileVersion = value;

                // NOTE: file version doesn't participate in GetHashCode(), so this isn't necessary
                _hashComputed = false;
            }
        }

        public string TargetFrameworkMoniker
        {
            get
            {
                return _targetFrameworkVersion;
            }

            set
            {
                _targetFrameworkVersion = value;
                _hashComputed = false;
            }
        }

        public IList<AssemblyReferenceInformation> AssemblyReferences { get; set; }

        public bool IsExplicitlySpecified { get; set; } = true;

        public string BinHash { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is not AssemblyInfo other)
            {
                return false;
            }

            return StringComparer.Ordinal.Equals(other.AssemblyIdentity, AssemblyIdentity)
                && StringComparer.Ordinal.Equals(other.TargetFrameworkMoniker, TargetFrameworkMoniker);
        }

        public override int GetHashCode()
        {
            if (!_hashComputed)
            {
                _hashCode = (AssemblyIdentity ?? string.Empty + (TargetFrameworkMoniker ?? string.Empty)).GetHashCode();
                _hashComputed = true;
            }

            return _hashCode;
        }

        public override string ToString()
        {
            return AssemblyIdentity;
        }

        public string GetFullAssemblyIdentity()
        {
            return string.Format(CultureInfo.InvariantCulture, LocalizedStrings.FullAssemblyIdentity, AssemblyIdentity, FileVersion);
        }

        public int CompareTo(object obj)
        {
            var obj2 = obj as AssemblyInfo;

            // BUG: This implementation of CompareTo is inconsistent with the type's equality
            // BUG: obj2 may be null at this point, which will NRE below
            return string.Compare(AssemblyIdentity, obj2.AssemblyIdentity, StringComparison.Ordinal);
        }
    }
}
