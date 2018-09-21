// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Cci.Extensions;
using Microsoft.Fx.Portability.ObjectModel;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.Fx.Portability.Analyzer
{
    public class DependencyFinderEngine : IDependencyInfo
    {
        private ConcurrentDictionary<string, ICollection<string>> _unresolvedAssemblies = new ConcurrentDictionary<string, ICollection<string>>();
        private HashSet<string> _assembliesWithError = new HashSet<string>();
        private HashSet<AssemblyInfo> _userAssemblies = new HashSet<AssemblyInfo>();
        private ConcurrentDictionary<MemberInfo, ICollection<AssemblyInfo>> _cachedDependencies;

        private IEnumerable<string> _inputAssemblies;

        private DependencyFinderEngine(IEnumerable<string> inputAssemblies)
        {
            _inputAssemblies = inputAssemblies;
        }

        public static DependencyFinderEngine ComputeDependencies(IEnumerable<string> inputAssemblies, IProgressTask progressTask)
        {
            var engine = new DependencyFinderEngine(inputAssemblies);

            engine.FindDependencies(progressTask);

            return engine;
        }

        public IDictionary<MemberInfo, ICollection<AssemblyInfo>> Dependencies
        {
            get { return _cachedDependencies; }
        }

        public IEnumerable<string> AssembliesWithErrors
        {
            get { return _assembliesWithError; }
        }

        public IDictionary<string, ICollection<string>> UnresolvedAssemblies
        {
            get { return _unresolvedAssemblies; }
        }

        public IEnumerable<AssemblyInfo> UserAssemblies
        {
            get { return _userAssemblies; }
        }

        private void FindDependencies(IProgressTask progressTask)
        {
            var dependencies = new ConcurrentDictionary<MemberInfo, ICollection<AssemblyInfo>>();
            _inputAssemblies.AsParallel().ForAll(filename =>
                {
                    foreach (var dep in GetDependencies(filename))
                    {
                        MemberInfo m = new MemberInfo() { MemberDocId = dep.MemberDocId, TypeDocId = dep.TypeDocId, DefinedInAssemblyIdentity = dep.DefinedInAssemblyIdentity };

                        // Add this memberinfo
                        var newassembly = new HashSet<AssemblyInfo>
                        {
                            dep.CallingAssembly
                        };
                        ICollection<AssemblyInfo> assemblies = dependencies.AddOrUpdate(m, newassembly, (key, existingSet) =>
                                                                                                        {
                                                                                                            lock (existingSet)
                                                                                                            {
                                                                                                                existingSet.Add(dep.CallingAssembly);
                                                                                                            }
                                                                                                            return existingSet;
                                                                                                        });
                    }
                    progressTask.ReportUnitComplete();
                });

            _cachedDependencies = dependencies;
        }

        private IEnumerable<MemberDependency> GetDependencies(string assemblyLocation)
        {
            using (var host = new HostEnvironment())
            {
                host.UnableToResolve += (s, e) =>
                {
                    string callingAssembly = e.Referrer.FullName();

                    // Try to get better information about the referrer. This may throw, but we don't want to crash even if we do.
                    try
                    {
                        callingAssembly = e.Referrer.GetAssemblyReference().AssemblyIdentity.Format();
                    }
                    catch { }

                    HashSet<string> newValue = new HashSet<string>
                    {
                        callingAssembly
                    };
                    _unresolvedAssemblies.AddOrUpdate(e.Unresolved.Format(), newValue, (key, existingHashSet) =>
                    {
                        lock (existingHashSet)
                        {
                            existingHashSet.Add(callingAssembly);
                        }
                        return existingHashSet;
                    });
                };

                host.UnifyToLibPath = true;
                var cciAssembly = host.LoadAssembly(assemblyLocation);

                if (cciAssembly == null)
                {
                    _assembliesWithError.Add(assemblyLocation);
                    yield break;
                }

                // Extract the fileversion and assembly version from the assembly.
                FileVersionInfo fileInfo = FileVersionInfo.GetVersionInfo(assemblyLocation);

                var assemblyInfo = new AssemblyInfo
                {
                    Location = cciAssembly.Location,
                    AssemblyIdentity = cciAssembly.AssemblyIdentity.Format(),
                    FileVersion = fileInfo.FileVersion ?? string.Empty,
                    TargetFrameworkMoniker = cciAssembly.GetTargetFrameworkMoniker()
                };

                // remember this assembly as a user assembly.
                _userAssemblies.Add(assemblyInfo);

                // Identify references to members (generic and non-generic)
                foreach (var reference in cciAssembly.GetTypeMemberReferences())
                {
                    if (reference.ContainingType.GetAssemblyReference() == null)
                        continue;

                    string definedIn = reference.ContainingType.GetAssemblyReference().ContainingAssembly.AssemblyIdentity.Format();

                    // return the type
                    yield return new MemberDependency()
                    {
                        CallingAssembly = assemblyInfo,
                        MemberDocId = reference.ContainingType.DocId(),
                        DefinedInAssemblyIdentity = definedIn
                    };

                    // return the member
                    yield return new MemberDependency()
                    {
                        CallingAssembly = assemblyInfo,
                        MemberDocId = reference.DocId(),
                        TypeDocId = reference.ContainingType.DocId(),
                        DefinedInAssemblyIdentity = definedIn
                    };
                }

                // Identify references to types
                foreach (var refence in cciAssembly.GetTypeReferences())
                {
                    string definedIn = refence.GetAssemblyReference().ContainingAssembly.AssemblyIdentity.Format();

                    // return the type
                    yield return new MemberDependency()
                    {
                        CallingAssembly = assemblyInfo,
                        MemberDocId = refence.DocId(),
                        DefinedInAssemblyIdentity = definedIn
                    };
                }
            }
        }
    }
}
