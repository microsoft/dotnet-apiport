using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Fx.Portability.Resources;
using Microsoft.Fx.Portability.ObjectModel;

namespace Microsoft.Fx.Portability.Reporting.ObjectModel
{
    public class MissingTypeInfo : MissingInfo
    {
        private bool _isMissing;
        private HashSet<AssemblyInfo> _usedInAssemblies;

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
            int pos = DocId.IndexOf("T:");
            if (pos == -1)
                throw new ArgumentException(LocalizedStrings.MemberShouldBeDefinedOnTypeException, "docId");

            TypeName = DocId.Substring(pos);
            MissingMembers = new HashSet<MissingMemberInfo>();
            TargetStatus = targetStatus.Select(GenerateTargetStatusMessage).ToList();
            TargetVersionStatus = new List<Version>(targetStatus);

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
            MissingTypeInfo other = obj as MissingTypeInfo;

            return other != null && StringComparer.Ordinal.Equals(other.TypeName, TypeName);
        }
    }
}
