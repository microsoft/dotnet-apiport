// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Versioning;

namespace Microsoft.Fx.Portability.Reporting.ObjectModel
{
    [DataContract]
    public class ReportingResult
    {
        private HashSet<MissingTypeInfo> _missingTypes = new HashSet<MissingTypeInfo>();
        private Dictionary<string, ICollection<string>> _unresolvedUserAssemblies = new Dictionary<string, ICollection<string>>();
        private HashSet<string> _assembliesWithError = new HashSet<string>();
        private IList<FrameworkName> _targets;
        private List<AssemblyUsageInfo> _perAssemblyUsage;
        private Dictionary<AssemblyInfo, string> _assemblyNameMap;

        public IList<FrameworkName> Targets { get { return _targets; } }
        public string SubmissionId { get; private set; }
        public ServiceHeaders Headers { get; private set; }

        public ReportingResult(IList<FrameworkName> targets, string submissionId, ServiceHeaders headers)
        {
            _targets = targets;
            SubmissionId = submissionId;
            Headers = headers;
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

        public void AddMissingDependency(AssemblyInfo SourceAssembly, string docId, string typeDocId, List<Version> targetStatus, string recommendedChanges)
        {
            var typeInfo = new MissingTypeInfo(SourceAssembly, typeDocId ?? docId, targetStatus, recommendedChanges);

            // If we already have an entry for this type, get it.
            if (_missingTypes.Any(mt => mt.TypeName == typeInfo.TypeName))
            {
                typeInfo = _missingTypes.First(mt => mt.TypeName == typeInfo.TypeName);
                typeInfo.IncrementUsage(SourceAssembly);
            }
            else
            {
                _missingTypes.Add(typeInfo);
            }


            // If we did not receive a member entry, it means the entire type is missing -- flag it accordingly
            if (docId.StartsWith("M:", System.StringComparison.OrdinalIgnoreCase) ||
                docId.StartsWith("F:", System.StringComparison.OrdinalIgnoreCase) ||
                docId.StartsWith("P:", System.StringComparison.OrdinalIgnoreCase))
            {
                MissingMemberInfo memberInfo = new MissingMemberInfo(SourceAssembly, docId, targetStatus, recommendedChanges);
                typeInfo.AddMissingMember(memberInfo, SourceAssembly);
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