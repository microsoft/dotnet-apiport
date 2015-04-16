// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace Microsoft.Fx.Portability.Analyzer
{
    internal class ReflectionMetadataDependencyInfo : IDependencyInfo
    {
        private readonly ConcurrentDictionary<string, ICollection<string>> _unresolvedAssemblies = new ConcurrentDictionary<string, ICollection<string>>(StringComparer.Ordinal);
        private readonly HashSet<string> _assembliesWithError = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<AssemblyInfo> _userAssemblies = new HashSet<AssemblyInfo>();
        private readonly ConcurrentDictionary<MemberInfo, ICollection<AssemblyInfo>> _cachedDependencies = new ConcurrentDictionary<MemberInfo, ICollection<AssemblyInfo>>();
        private readonly IEnumerable<string> _inputAssemblies;

        private ReflectionMetadataDependencyInfo(IEnumerable<string> inputAssemblies)
        {
            _inputAssemblies = inputAssemblies;
        }

        public static ReflectionMetadataDependencyInfo ComputeDependencies(IEnumerable<string> inputAssemblies, IProgressReporter progressReport)
        {
            var engine = new ReflectionMetadataDependencyInfo(inputAssemblies);

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
            _inputAssemblies.AsParallel().ForAll(filename =>
            {
                foreach (var dependencies in GetDependencies(filename))
                {
                    var m = new MemberInfo
                    {
                        MemberDocId = dependencies.MemberDocId,
                        TypeDocId = dependencies.TypeDocId,
                        DefinedInAssemblyIdentity = dependencies.DefinedInAssemblyIdentity
                    };

                    // Add this memberinfo
                    var newassembly = new HashSet<AssemblyInfo> { dependencies.CallingAssembly };

                    var assemblies = _cachedDependencies.AddOrUpdate(m, newassembly, (key, existingSet) =>
                    {
                        lock (existingSet)
                        {
                            existingSet.Add(dependencies.CallingAssembly);
                        }
                        return existingSet;
                    });
                }
            });
        }

        private IEnumerable<MemberDependency> GetDependencies(string assemblyLocation)
        {
            using (var stream = File.OpenRead(assemblyLocation))
            using (var peFile = new PEReader(stream))
            {
                var metadataReader = peFile.GetMetadataReader();

                var helper = new DependencyFinderEngineHelper(metadataReader, assemblyLocation);
                helper.ComputeData();

                // Remember this assembly as a user assembly.
                _userAssemblies.Add(helper.CallingAssembly);

                if (helper != null)
                    return helper.MemberDependency;
                else
                    return null;
            }
        }
    }
}
