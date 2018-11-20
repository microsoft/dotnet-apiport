// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Analyzer.Resources;
using System;
using System.Collections.Generic;

namespace Microsoft.Fx.Portability.Analyzer
{
    public class ReflectionMetadataDependencyFinder : IDependencyFinder
    {
        private readonly IDependencyFilter _assemblyFilter;
        private readonly SystemObjectFinder _objectFinder;

        public ReflectionMetadataDependencyFinder(IDependencyFilter assemblyFilter, SystemObjectFinder objectFinder)
        {
            _assemblyFilter = assemblyFilter ?? throw new ArgumentNullException(nameof(assemblyFilter));
            _objectFinder = objectFinder;
        }

        public IDependencyInfo FindDependencies(IEnumerable<IAssemblyFile> files, IProgressReporter progressReporter)
        {
            using (var task = progressReporter.StartTask(LocalizedStrings.DetectingAssemblyReferences))
            {
                try
                {
                    return ReflectionMetadataDependencyInfo.ComputeDependencies(files, _assemblyFilter, progressReporter, _objectFinder);
                }
                catch (Exception)
                {
                    task.Abort();

                    throw;
                }
            }
        }
    }
}
