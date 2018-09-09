// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Resources;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Fx.Portability.Reporting.ObjectModel
{
    public class MissingTypeInfo : MissingInfo
    {
        private readonly HashSet<AssemblyInfo> _usedInAssemblies;
        private bool _isMissing;

        public int UsageCount { get { return _usedInAssemblies.Count; } }

        public IEnumerable<AssemblyInfo> UsedIn { get { return _usedInAssemblies; } }

        public IEnumerable<string> TargetStatus { get; set; }

        public IEnumerable<Version> TargetVersionStatus { get; set; }

        public bool IsMissing { get { return _isMissing; } }

        public HashSet<MissingMemberInfo> MissingMembers;

        public string TypeName { get; set; }

        public void AddMissingMember(MissingMemberInfo mmi, AssemblyInfo sourceAssembly)
        {
            var x = MissingMembers.FirstOrDefault(m => m.MemberName == mmi.MemberName);
            if (x != null)
            {
                x.IncrementUsage(sourceAssembly);
            }
            else
            {
                MissingMembers.Add(mmi);
            }
        }

        public void MarkAsMissing()
        {
            _isMissing = true;
        }

        public void IncrementUsage(AssemblyInfo sourceAssembly)
        {
            _usedInAssemblies.Add(sourceAssembly);
        }

        public MissingTypeInfo(AssemblyInfo sourceAssembly, string docId, List<Version> targetStatus, string recommendedChanges)
            : base(docId)
        {
            int pos = DocId.IndexOf("T:", StringComparison.Ordinal);
            if (pos == -1)
                throw new ArgumentException(LocalizedStrings.MemberShouldBeDefinedOnTypeException, nameof(docId));

            TypeName = DocId.Substring(pos);
            MissingMembers = new HashSet<MissingMemberInfo>();
            TargetStatus = targetStatus?.Select(GenerateTargetStatusMessage).ToList() ?? Enumerable.Empty<string>();
            TargetVersionStatus = new List<Version>(targetStatus ?? Enumerable.Empty<Version>());

            RecommendedChanges = recommendedChanges;
            _usedInAssemblies = new HashSet<AssemblyInfo>();

            if (sourceAssembly != null)
            {
                _usedInAssemblies.Add(sourceAssembly);
            }
        }

        public override int GetHashCode()
        {
            return TypeName.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is MissingTypeInfo other
                && StringComparer.Ordinal.Equals(other.TypeName, TypeName);
        }
    }
}
