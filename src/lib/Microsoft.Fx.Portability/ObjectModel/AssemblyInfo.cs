// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Resources;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Fx.Portability.ObjectModel
{
    public sealed class AssemblyInfo : IComparable
    {
        private bool _hashComputed;
        private int _hashCode;

        private string _assemblyIdentity;
        private string _targetFrameworkVersion;
        private string _fileVersion;

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

        public override bool Equals(object obj)
        {
            if (!(obj is AssemblyInfo other))
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

            return string.Compare(AssemblyIdentity, obj2.AssemblyIdentity, StringComparison.Ordinal);
        }
    }
}
