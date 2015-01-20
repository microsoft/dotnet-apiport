// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Resources;
using System;

namespace Microsoft.Fx.Portability.ObjectModel
{
    public sealed class AssemblyInfo
    {
        private bool _hashComputed;
        private int _hashCode;

        private string _assemblyIdentity;
        private string _targetFrameworkVersion;
        private string _fileVersion;

        public string AssemblyIdentity
        {
            get { return _assemblyIdentity; }
            set
            {
                _assemblyIdentity = value;
                _hashComputed = false;
            }
        }

        public string FileVersion
        {
            get { return _fileVersion; }
            set
            {
                _fileVersion = value;
                _hashComputed = false;
            }
        }

        public string TargetFrameworkMoniker
        {
            get { return _targetFrameworkVersion; }
            set
            {
                _targetFrameworkVersion = value;
                _hashComputed = false;
            }
        }

        public override bool Equals(object obj)
        {
            AssemblyInfo other = obj as AssemblyInfo;
            if (other == null)
                return false;

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
            return string.Format(LocalizedStrings.FullAssemblyIdentity, AssemblyIdentity, FileVersion);
        }
    }
}
