// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace Microsoft.Fx.Portability.Analyzer
{
    internal class DependencyFinderEngine : IDependencyInfo
    {
        private readonly ConcurrentDictionary<string, ICollection<string>> _unresolvedAssemblies = new ConcurrentDictionary<string, ICollection<string>>();
        private readonly HashSet<string> _assembliesWithError = new HashSet<string>();
        private readonly HashSet<AssemblyInfo> _userAssemblies = new HashSet<AssemblyInfo>();
        private ConcurrentDictionary<MemberInfo, ICollection<AssemblyInfo>> _cachedDependencies;
        private readonly IEnumerable<string> _inputAssemblies;

        private DependencyFinderEngine(IEnumerable<string> inputAssemblies)
        {
            _inputAssemblies = inputAssemblies;
        }

        public static DependencyFinderEngine ComputeDependencies(IEnumerable<string> inputAssemblies, IProgressReporter progressReport)
        {
            var engine = new DependencyFinderEngine(inputAssemblies);

            engine.FindDependencies(progressReport);

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

        private void FindDependencies(IProgressReporter progressReport)
        {
            var dependencies = new ConcurrentDictionary<MemberInfo, ICollection<AssemblyInfo>>();
            _inputAssemblies.AsParallel().ForAll(filename =>
            {
                IEnumerable<MemberDependency> MemberDependencyList = GetDependencies(filename);
                foreach (var dep in MemberDependencyList)
                {
                    MemberInfo m = new MemberInfo() { MemberDocId = dep.MemberDocId, TypeDocId = dep.TypeDocId, DefinedInAssemblyIdentity = dep.DefinedInAssemblyIdentity };

                    // Add this memberinfo
                    HashSet<AssemblyInfo> newassembly = new HashSet<AssemblyInfo>();
                    newassembly.Add(dep.CallingAssembly);
                    ICollection<AssemblyInfo> assemblies = dependencies.AddOrUpdate(m, newassembly, (key, existingSet) =>
                    {
                        lock (existingSet)
                        {
                            existingSet.Add(dep.CallingAssembly);
                        }
                        return existingSet;
                    });
                }
                progressReport.ReportUnitComplete();
            });

            _cachedDependencies = dependencies;
        }



        private IEnumerable<MemberDependency> GetDependencies(string assemblyLocation)
        {
            DependencyFinderEngineHelper helper = null;
            using (var stream = File.OpenRead(assemblyLocation))
            {
                using (var peFile = new PEReader(stream))
                {
                    MetadataReader metadatareader = peFile.GetMetadataReader();
                    helper = new DependencyFinderEngineHelper(metadatareader, assemblyLocation);
                    helper.ComputeData();
                }
            }


            // remember this assembly as a user assembly.
            _userAssemblies.Add(helper.CallingAssembly);


            if (helper != null)
                return helper.memberDependency;
            else
                return null;
        }
    }
}
