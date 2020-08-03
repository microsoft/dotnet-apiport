// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Fx.Portability
{
    public class DependencyOrderer : IDependencyOrderer
    {
        private readonly IProgressReporter _progress;

        public DependencyOrderer(IProgressReporter progress)
        {
            _progress = progress;
        }

        private Dictionary<string, AssemblyInfo> CreateDictionary(IEnumerable<AssemblyInfo> userAssemblies)
        {
            var dictionary = new Dictionary<string, AssemblyInfo>(StringComparer.Ordinal);

            foreach (var assembly in userAssemblies)
            {
                var name = assembly.GetAssemblyName().Name;
                try
                {
                    dictionary.Add(name, assembly);
                }
                catch (ArgumentException)
                {
                    _progress.ReportIssue($"Duplicate assembly name {name} found. Only using first one found.");
                }
            }

            return dictionary;
        }

        public IEnumerable<string> GetOrder(string entryPoint, IEnumerable<AssemblyInfo> userAssemblies)
        {
            if (string.IsNullOrEmpty(entryPoint))
            {
                return Enumerable.Empty<string>();
            }

            var entryAssembly = userAssemblies.FirstOrDefault(u => string.Equals(u.GetAssemblyName().Name, entryPoint, StringComparison.OrdinalIgnoreCase));

            if (entryAssembly is null)
            {
                _progress.ReportIssue($"Entrypoint {entryPoint} could not be found");
                return Enumerable.Empty<string>();
            }

            var assemblies = CreateDictionary(userAssemblies);

            return PostOrderTraversal(
                new[] { entryAssembly },
                a => a.AssemblyReferences?.Select(r =>
                    {
                        if (assemblies.TryGetValue(r.Name, out var info))
                        {
                            return info;
                        }
                        else
                        {
                            return null;
                        }
                    })
                    .Where(r => r != null) ?? Enumerable.Empty<AssemblyInfo>())
                    .Select(r => r.AssemblyIdentity);
        }

        private static IEnumerable<T> PostOrderTraversal<T>(IEnumerable<T> initial, Func<T, IEnumerable<T>> selector, IEqualityComparer<T> comparer = null)
        {
            var result = new List<T>();
            var visited = new HashSet<T>(comparer);

            void Visit(T item)
            {
                if (!visited.Add(item))
                {
                    return;
                }

                foreach (var inner in selector(item))
                {
                    Visit(inner);
                }

                result.Add(item);
            }

            foreach (var item in initial)
            {
                Visit(item);
            }

            return result;
        }
    }
}
