// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using System;

namespace Microsoft.Fx.Portability.Analyzer
{
    internal class MemberDependency
    {
        private bool _hashComputed;
        private int _hashCode;

        private string _definedInAssemblyIdentity;
        private string _memberDocId;
        private string _typeDocId;

        /// <summary>
        /// This represents the assembly that is calling the member
        /// </summary>
        public AssemblyInfo CallingAssembly { get; set; }

        /// <summary>
        /// This represents the assembly in which the member is defined
        /// </summary>
        public string DefinedInAssemblyIdentity
        {
            get { return _definedInAssemblyIdentity; }
            set
            {
                _definedInAssemblyIdentity = value;
                _hashComputed = false;
            }
        }

        public string MemberDocId
        {
            get { return _memberDocId; }
            set
            {
                _memberDocId = value;
                _hashComputed = false;
            }
        }

        public string TypeDocId
        {
            get { return _typeDocId; }
            set
            {
                _typeDocId = value;
                _hashComputed = false;
            }
        }

        public override string ToString()
        {
            return string.Format("{0} [{1}]", MemberDocId, CallingAssembly.AssemblyIdentity);
        }

        public override bool Equals(object obj)
        {
            MemberDependency other = obj as MemberDependency;
            if (other == null)
                return false;

            return StringComparer.Ordinal.Equals(MemberDocId, other.MemberDocId) &&
                    StringComparer.Ordinal.Equals(DefinedInAssemblyIdentity, other.DefinedInAssemblyIdentity) &&
                    CallingAssembly.Equals(other.CallingAssembly);
        }

        public override int GetHashCode()
        {
            if (!_hashComputed)
            {
                _hashCode = ((DefinedInAssemblyIdentity ?? string.Empty) + (MemberDocId ?? string.Empty)).GetHashCode() ^ CallingAssembly.GetHashCode();
                _hashComputed = true;
            }
            return _hashCode;
        }
    }
}
