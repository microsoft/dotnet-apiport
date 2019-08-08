// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using System;
using System.Globalization;

namespace Microsoft.Fx.Portability.Analyzer
{
    internal class MemberDependency
    {
        private bool _hashComputed;
        private int _hashCode;

        private AssemblyReferenceInformation _definedInAssemblyIdentity;
        private string _memberDocId;
        private string _typeDocId;
        private bool _isPrimitive;

        /// <summary>
        /// Gets or sets the assembly that is calling the member.
        /// </summary>
        public AssemblyInfo CallingAssembly { get; set; }

        /// <summary>
        /// Gets or sets the assembly in which the member is defined.
        /// </summary>
        public AssemblyReferenceInformation DefinedInAssemblyIdentity
        {
            get
            {
                return _definedInAssemblyIdentity;
            }

            set
            {
                _definedInAssemblyIdentity = value;
                _hashComputed = false;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not the dependency is a primitive type.
        /// </summary>
        public bool IsPrimitive
        {
            get
            {
                return _isPrimitive;
            }

            set
            {
                _isPrimitive = value;
                _hashComputed = false;
            }
        }

        public string MemberDocId
        {
            get
            {
                return _memberDocId;
            }

            set
            {
                _memberDocId = value;
                _hashComputed = false;
            }
        }

        public string TypeDocId
        {
            get
            {
                return _typeDocId;
            }

            set
            {
                _typeDocId = value;
                _hashComputed = false;
            }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} [{1}]", MemberDocId, CallingAssembly.AssemblyIdentity);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is MemberDependency other))
            {
                return false;
            }

            return string.Equals(MemberDocId, other.MemberDocId, StringComparison.Ordinal) &&
                    DefinedInAssemblyIdentity == other.DefinedInAssemblyIdentity &&
                    CallingAssembly.Equals(other.CallingAssembly) &&
                    IsPrimitive == other.IsPrimitive;
        }

        public override int GetHashCode()
        {
            if (!_hashComputed)
            {
                _hashCode = ((DefinedInAssemblyIdentity?.ToString() ?? string.Empty) + (MemberDocId ?? string.Empty) + IsPrimitive.ToString((IFormatProvider)CultureInfo.CurrentUICulture)).GetHashCode() ^ CallingAssembly.GetHashCode();
                _hashComputed = true;
            }

            return _hashCode;
        }
    }
}
