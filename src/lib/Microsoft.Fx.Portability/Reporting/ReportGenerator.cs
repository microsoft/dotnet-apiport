// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reporting.ObjectModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;

namespace Microsoft.Fx.Portability.Reporting
{
    public class ReportGenerator : IReportGenerator
    {
        public ReportingResult ComputeReport(
            IList<FrameworkName> targets,
            string submissionId,
            AnalyzeRequestFlags requestFlags,
            IDictionary<MemberInfo, ICollection<AssemblyInfo>> allDependencies,
            IList<MemberInfo> missingDependencies,
            IDictionary<string, ICollection<string>> unresolvedAssemblies,
            IList<string> unresolvedUserAssemblies,
            IEnumerable<string> assembliesWithErrors,
            IList<NuGetPackageInfo> nugetPackages)
        {
            var types = allDependencies.Keys.Where(dep => dep.TypeDocId == null);
            ReportingResult result = new ReportingResult(targets, types, submissionId, requestFlags);

            missingDependencies
                .AsParallel()
                .ForAll((Action<MemberInfo>)((item) =>
                {
                    // the calling assemblies are in Finder...
                    if (allDependencies == null)
                    {
                        lock (result)
                        {
                            result.AddMissingDependency(null, item, item.RecommendedChanges);
                        }
                    }
                    else
                    {
                        if (!allDependencies.TryGetValue(item, out var calledIn))
                        {
                            return;
                        }

                        foreach (var callingAsm in calledIn)
                        {
                            lock (result)
                            {
                                result.AddMissingDependency(callingAsm, item, item.RecommendedChanges);
                            }
                        }
                    }
                }));

            if (assembliesWithErrors != null)
            {
                foreach (var error in assembliesWithErrors)
                {
                    result.AddAssemblyWithError(error);
                }
            }

            foreach (var unresolvedAssembly in unresolvedUserAssemblies)
            {
                result.AddUnresolvedUserAssembly(unresolvedAssembly, unresolvedAssemblies == null ? Enumerable.Empty<string>() : unresolvedAssemblies[unresolvedAssembly]);
            }

            // Compute per assembly report
            if (allDependencies != null)
            {
                var perAssemblyUsage = ComputePerAssemblyUsage(targets, missingDependencies, allDependencies);
                result.SetAssemblyUsageInfo(perAssemblyUsage);

                // Compute the map of assemblyInfo to name
                var assemblyNameMap = ComputeAssemblyNames(perAssemblyUsage);
                result.SetAssemblyNameMap(assemblyNameMap);
            }

            result.NuGetPackages = nugetPackages;
            return result;
        }

        private static List<AssemblyUsageInfo> ComputePerAssemblyUsage(
            IList<FrameworkName> targets,
            IList<MemberInfo> missingDependencies,
            IDictionary<MemberInfo, ICollection<AssemblyInfo>> allDependencies)
        {
            Dictionary<MemberInfo, MemberInfo> missingDeps = missingDependencies.ToDictionary(key => key, value => value);

            var perAsmUsage = new ConcurrentDictionary<AssemblyInfo, AssemblyUsageInfo>();

            allDependencies.Keys
                .AsParallel()
                .ForAll((Action<MemberInfo>)(memberInfo =>
                {
                    // This is declared here to minimize allocations
                    AssemblyUsageInfo currentAssembly;

                    if (!allDependencies.TryGetValue(memberInfo, out var usedIn))
                    {
                        return;
                    }

                    foreach (var file in usedIn)
                    {
                        // Add the current file to the dictionary
                        currentAssembly = perAsmUsage.GetOrAdd(file, new Func<AssemblyInfo, AssemblyUsageInfo>(ai => new AssemblyUsageInfo(ai, targets.Count)));

                        for (int i = 0; i < targets.Count; i++)
                        {
                            if (missingDeps.ContainsKey(memberInfo) && (missingDeps[memberInfo].TargetStatus[i] == null || missingDeps[memberInfo].TargetStatus[i] > targets[i].Version))
                            {
                                // If the dependency is missing, check to see if we know how to change the code for this API
                                currentAssembly.UsageData[i].IncrementCallsToUnavailableApi();
                            }
                            else
                            {
                                currentAssembly.UsageData[i].IncrementCallsToAvailableApi();
                            }
                        }
                    }
                }));

            return perAsmUsage.Values.ToList();
        }

        /// <summary>
        /// Give a list of assemblies it will compute which name an assembly must have.
        /// For instance, if we have a single assembly, we will use the assembly simple name.
        /// If we have more than one then we should use the full assembly name in order to distinguish betweeen them.
        /// </summary>
        private static Dictionary<AssemblyInfo, string> ComputeAssemblyNames(IEnumerable<AssemblyUsageInfo> assemblyUsage)
        {
            // Group the assemblies by the simple name. In order to do that we need to parse the assembly identity and use the Name property.
            var mapAssemblyNameOccurences = assemblyUsage.GroupBy(asui => new System.Reflection.AssemblyName(asui.SourceAssembly.AssemblyIdentity).Name)
                                                .SelectMany(assemblyNameGroup => assemblyNameGroup.Select(assemblyInfo => new
                                                {
                                                    // If we have more than one assembly with the same name, use the fullassemblyidentity
                                                    Name = assemblyNameGroup.Count() > 1 ? assemblyInfo.SourceAssembly.GetFullAssemblyIdentity() : assemblyNameGroup.Key,
                                                    SourceAssembly = assemblyInfo.SourceAssembly
                                                }))
                                                .ToDictionary(key => key.SourceAssembly, value => value.Name);

            return mapAssemblyNameOccurences;
        }
    }
}
