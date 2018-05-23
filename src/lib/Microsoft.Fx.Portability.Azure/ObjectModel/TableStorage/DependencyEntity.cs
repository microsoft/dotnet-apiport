// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;

namespace Microsoft.Fx.Portability.Azure.ObjectModel.TableStorage
{
    public class DependencyEntity : BaseEntity
    {
        private bool _hashComputed;
        private int _hashCode;

        private string _docId { get; set; }
        private string _assemblyInfo { get; set; }

        public string DocId
        {
            get { return _docId; }
            set
            {
                _docId = value;
                _hashComputed = false;
            }
        }
        public string AssemblyInfo
        {
            get { return _assemblyInfo; }
            set
            {
                _assemblyInfo = value;
                _hashComputed = false;
            }
        }
        public DependencyEntity()
        {

        }

        public DependencyEntity(MemberInfo member, AssemblyInfo assembly)
        {
            DocId = member.MemberDocId + (member.TypeDocId == null ? string.Empty : (":" + member.TypeDocId));
            AssemblyInfo = assembly.AssemblyIdentity + ", fileVer: " + assembly.FileVersion;

            PartitionKey = ComputeHashCode(DocId);
            RowKey = ComputeHashCode(AssemblyInfo);
        }

        public override int GetHashCode()
        {
            if (!_hashComputed)
            {
                _hashCode = (DocId + AssemblyInfo).GetHashCode();
                _hashComputed = true;
            }
            return _hashCode;
        }

        public override bool Equals(object obj)
        {
            DependencyEntity other = obj as DependencyEntity;
            return other != null && (other.DocId == DocId && other.AssemblyInfo == AssemblyInfo);
        }
    }
}
