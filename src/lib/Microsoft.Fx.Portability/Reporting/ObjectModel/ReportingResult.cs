// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;

namespace Microsoft.Fx.Portability.Reporting.ObjectModel
{
    public class ReportingResult
    {
        private readonly HashSet<MissingTypeInfo> _missingTypes = new HashSet<MissingTypeInfo>();
        private readonly Dictionary<Tuple<string, string>, MemberInfo> _types;
        private readonly Dictionary<string, ICollection<string>> _unresolvedUserAssemblies = new Dictionary<string, ICollection<string>>();
        private readonly HashSet<string> _assembliesWithError = new HashSet<string>();
        private List<AssemblyUsageInfo> _perAssemblyUsage;
        private Dictionary<AssemblyInfo, string> _assemblyNameMap;

        public AnalyzeRequestFlags RequestFlags { get; }

        public IList<FrameworkName> Targets { get; }

        public string SubmissionId { get;  }

        public IList<NuGetPackageInfo> NuGetPackages { get; set; }

        public ReportingResult(IList<FrameworkName> targets, IEnumerable<MemberInfo> types, string submissionId, AnalyzeRequestFlags requestFlags)
        {
            Targets = targets;
            RequestFlags = requestFlags;
            SubmissionId = submissionId;
            _types = types.ToDictionary(key => Tuple.Create(key.DefinedInAssemblyIdentity, key.MemberDocId), value => value);
        }

        public IEnumerable<string> GetAssembliesWithError()
        {
            return _assembliesWithError.ToList();
        }

        public IEnumerable<MissingTypeInfo> GetMissingTypes()
        {
            return _missingTypes.ToList();
        }

        public IDictionary<string, ICollection<string>> GetUnresolvedAssemblies()
        {
            return _unresolvedUserAssemblies;
        }

        public IEnumerable<AssemblyUsageInfo> GetAssemblyUsageInfo()
        {
            return _perAssemblyUsage != null ? _perAssemblyUsage.ToList() : Enumerable.Empty<AssemblyUsageInfo>();
        }

        public void AddMissingDependency(AssemblyInfo sourceAssembly, MemberInfo missingDependency, string recommendedChanges)
        {
            MissingTypeInfo typeInfo;
            try
            {
                var type = _types[Tuple.Create(missingDependency.DefinedInAssemblyIdentity, missingDependency.TypeDocId ?? missingDependency.MemberDocId)];
                typeInfo = new MissingTypeInfo(sourceAssembly, type.MemberDocId, type.TargetStatus, type.RecommendedChanges);
            }
            catch (KeyNotFoundException)
            {
                typeInfo = new MissingTypeInfo(sourceAssembly, missingDependency.TypeDocId ?? missingDependency.MemberDocId, missingDependency.TargetStatus, recommendedChanges);
            }

            // If we already have an entry for this type, get it.
            if (_missingTypes.Any(mt => mt.TypeName == typeInfo.TypeName))
            {
                typeInfo = _missingTypes.First(mt => mt.TypeName == typeInfo.TypeName);
                typeInfo.IncrementUsage(sourceAssembly);
            }
            else
            {
                _missingTypes.Add(typeInfo);
            }

            // If we did not receive a member entry, it means the entire type is missing -- flag it accordingly
            if (missingDependency.MemberDocId.StartsWith("M:", StringComparison.OrdinalIgnoreCase) ||
                missingDependency.MemberDocId.StartsWith("F:", StringComparison.OrdinalIgnoreCase) ||
                missingDependency.MemberDocId.StartsWith("P:", StringComparison.OrdinalIgnoreCase))
            {
                var memberInfo = new MissingMemberInfo(sourceAssembly, missingDependency.MemberDocId, missingDependency.TargetStatus, recommendedChanges);
                typeInfo.AddMissingMember(memberInfo, sourceAssembly);
            }
            else
            {
                typeInfo.MarkAsMissing();
            }
        }

        public void SetAssemblyUsageInfo(List<AssemblyUsageInfo> list)
        {
            _perAssemblyUsage = list;
        }

        public void AddUnresolvedUserAssembly(string assembly, IEnumerable<string> usedIn)
        {
            _unresolvedUserAssemblies.Add(assembly, usedIn.ToList());
        }

        public void AddAssemblyWithError(string error)
        {
            _assembliesWithError.Add(error);
        }

        public string GetNameForAssemblyInfo(AssemblyInfo assembly)
        {
            return _assemblyNameMap == null ? string.Empty : _assemblyNameMap[assembly];
        }

        public void SetAssemblyNameMap(Dictionary<AssemblyInfo, string> assemblyNameMap)
        {
            _assemblyNameMap = assemblyNameMap;
        }
    }
}
