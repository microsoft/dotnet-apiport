using Microsoft.Fx.Portability.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Microsoft.Fx.Portability.ObjectModel
{
    [DataContract]
    public sealed class MemberInfoV1
    {
        private bool _hashComputed;
        private int _hashCode;

        private string _targetName;
        private string _memberDocId;
        private string _typeDocId;

        public MemberInfoV1() { }

        public MemberInfoV1(MemberInfo memberInfo)
        {
            TargetStatus = memberInfo.TargetStatus == null ? null : memberInfo.TargetStatus.Select(GetTargetStatusMessage).ToList();

            DefinedInAssemblyIdentity = memberInfo.DefinedInAssemblyIdentity;
            MemberDocId = memberInfo.MemberDocId;
            TypeDocId = memberInfo.TypeDocId;
            RecommendedChanges = memberInfo.RecommendedChanges;
        }

        private static string GetTargetStatusMessage(Version v)
        {
            if (v == null)
            {
                return LocalizedStrings.NotSupported;
            }
            else
            {
                return String.Format(LocalizedStrings.SupportedOn, v);
            }
        }

        /// <summary>
        /// This represents the assembly in which the member is defined
        /// </summary>
        [DataMember]
        public string DefinedInAssemblyIdentity
        {
            get { return _targetName; }
            set
            {
                _targetName = value;
                _hashComputed = false;
            }
        }

        [DataMember]
        public string MemberDocId
        {
            get { return _memberDocId; }
            set
            {
                _memberDocId = value;
                _hashComputed = false;
            }
        }

        [DataMember]
        public string TypeDocId
        {
            get { return _typeDocId; }
            set
            {
                _typeDocId = value;
                _hashComputed = false;
            }
        }

        [DataMember]
        public string RecommendedChanges { get; set; }

        [DataMember]
        public List<string> TargetStatus { get; set; }

        public override string ToString()
        {
            return MemberDocId;
        }

        public override bool Equals(object obj)
        {
            MemberInfo other = obj as MemberInfo;
            if (other == null)
                return false;

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
    }
}
