// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Microsoft.Fx.Portability.ObjectModel
{
    public sealed class MemberInfo : IComparable
    {
        private bool _hashComputed;
        private int _hashCode;

        private string _targetName;
        private string _memberDocId;
        private string _typeDocId;

        /// <summary>
        /// Gets or sets the assembly in which the member is defined.
        /// </summary>
        public string DefinedInAssemblyIdentity
        {
            get
            {
                return _targetName;
            }

            set
            {
                _targetName = value;
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

        public string RecommendedChanges { get; set; }

        public string SourceCompatibleChange { get; set; }

        public List<Version> TargetStatus { get; set; }

        [JsonIgnore]
        public bool IsSupportedAcrossTargets { get; set; }

        public override string ToString()
        {
            return MemberDocId;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is MemberInfo other))
            {
                return false;
            }

            return StringComparer.Ordinal.Equals(MemberDocId, other.MemberDocId) &&
                    StringComparer.Ordinal.Equals(DefinedInAssemblyIdentity, other.DefinedInAssemblyIdentity);
        }

        public override int GetHashCode()
        {
            if (!_hashComputed)
            {
                _hashCode = ((DefinedInAssemblyIdentity ?? string.Empty) + (MemberDocId ?? string.Empty)).GetHashCode();
                _hashComputed = true;
            }

            return _hashCode;
        }

        public int CompareTo(object obj)
        {
            var obj2 = obj as MemberInfo;

            return string.Compare(MemberDocId, obj2.MemberDocId, StringComparison.Ordinal);
        }
    }
}
