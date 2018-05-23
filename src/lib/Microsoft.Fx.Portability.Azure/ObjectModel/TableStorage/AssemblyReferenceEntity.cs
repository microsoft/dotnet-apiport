// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Fx.Portability.Azure.ObjectModel.TableStorage
{
    public class AssemblyReferenceEntity : BaseEntity
    {
        private bool _hashComputed;
        private int _hashCode;

        private string _sourceAssemblyInfo;
        private string _targetAssemblyInfo;

        public string SourceAssemblyInfo
        {
            get { return _sourceAssemblyInfo; }
            set
            {
                _sourceAssemblyInfo = value;
                _hashComputed = false;
            }
        }
        public string TargetAssemblyInfo
        {
            get { return _targetAssemblyInfo; }
            set
            {
                _targetAssemblyInfo = value;
                _hashComputed = false;
            }
        }

        public AssemblyReferenceEntity(string sourceAssemblyIdentity, string targetAssemblyIdentity)
        {
            SourceAssemblyInfo = sourceAssemblyIdentity;
            TargetAssemblyInfo = targetAssemblyIdentity;

            PartitionKey = ComputeHashCode(SourceAssemblyInfo);
            RowKey = ComputeHashCode(TargetAssemblyInfo);
        }

        public override int GetHashCode()
        {
            if (!_hashComputed)
            {
                _hashCode = (SourceAssemblyInfo + TargetAssemblyInfo).GetHashCode();
                _hashComputed = true;
            }
            return _hashCode;
        }

        public override bool Equals(object obj)
        {
            AssemblyReferenceEntity other = obj as AssemblyReferenceEntity;
            return other != null && (other.SourceAssemblyInfo == SourceAssemblyInfo && other.TargetAssemblyInfo == TargetAssemblyInfo);
        }
    }
}
