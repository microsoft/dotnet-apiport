// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Analyzer;
using Microsoft.Fx.Portability.ObjectModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.Fx.Portability
{
    public class DependencyOrderer : IDependencyOrderer
    {
        private readonly IDependencyFilter _filter;

        public DependencyOrderer(IDependencyFilter filter)
        {
            _filter = filter;
        }

        public IEnumerable<AssemblyInfo> GetOrder(AssemblyInfo entryPoint, IEnumerable<AssemblyInfo> userAssemblies)
        {
            if (entryPoint is null)
            {
                return Enumerable.Empty<AssemblyInfo>();
            }

            var assemblies = userAssemblies
                .Where(a => !_filter.IsFrameworkAssembly(a.GetAssemblyName()))
                .ToDictionary(t => t.GetAssemblyName().Name, StringComparer.Ordinal);

            return PostOrderTraversal(
                new[] { entryPoint },
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
                    .Where(r => r != null) ?? Enumerable.Empty<AssemblyInfo>());
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
