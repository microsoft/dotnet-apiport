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
        private readonly HashSet<AssemblyInfo> _usedInAssemblies;

        public IEnumerable<AssemblyInfo> UsedIn
        {
            get { return _usedInAssemblies.ToList().AsReadOnly(); }
        }

        public int Uses
        {
            get { return _usedInAssemblies.Count; }
        }

        public string MemberName { get; set; }

        public IEnumerable<string> TargetStatus { get; set; }

        public IEnumerable<Version> TargetVersionStatus { get; set; }

        public void IncrementUsage(AssemblyInfo sourceAssembly)
        {
            _usedInAssemblies.Add(sourceAssembly);
        }

        public MissingMemberInfo(AssemblyInfo sourceAssembly, string docId, List<Version> targetStatus, string recommendedChanges)
            : base(docId)
        {
            RecommendedChanges = recommendedChanges;
            MemberName = docId;
            TargetStatus = targetStatus?.Select(GenerateTargetStatusMessage).ToList() ?? Enumerable.Empty<string>();
            TargetVersionStatus = new List<Version>(targetStatus ?? Enumerable.Empty<Version>());

            _usedInAssemblies = new HashSet<AssemblyInfo>();

            if (sourceAssembly != null)
            {
                _usedInAssemblies.Add(sourceAssembly);
            }
        }
    }
}
