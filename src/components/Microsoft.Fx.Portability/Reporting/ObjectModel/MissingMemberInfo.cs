// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Fx.Portability.Reporting.ObjectModel
{
    public class MissingMemberInfo : MissingInfo
    {
        public IEnumerable<AssemblyInfo> UsedIn { get { return _usedInAssemblies; } }

        private HashSet<AssemblyInfo> _usedInAssemblies;

        public int Uses { get { return _usedInAssemblies.Count; } }
        public string MemberName { get; set; }
        public IEnumerable<String> TargetStatus { get; set; }
        public IEnumerable<Version> TargetVersionStatus { get; set; }
        public void IncrementUsage(AssemblyInfo sourceAssembly)
        {
            _usedInAssemblies.Add(sourceAssembly);
        }

        public MissingMemberInfo(AssemblyInfo sourceAssembly, string DocId, List<Version> targetStatus, string recommendedChanges)
            : base(DocId)
        {
            RecommendedChanges = recommendedChanges;
            MemberName = DocId;
            TargetStatus = targetStatus.Select(GenerateTargetStatusMessage).ToList();
            TargetVersionStatus = new List<Version>(targetStatus);

            _usedInAssemblies = new HashSet<AssemblyInfo>();

            if (sourceAssembly != null)
            {
                _usedInAssemblies.Add(sourceAssembly);
            }
        }
    }
}
