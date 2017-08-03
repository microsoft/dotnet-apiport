// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Microsoft.Fx.Portability.Analyzer
{
    public class ReflectionMetadataDependencyFinder : IDependencyFinder
    {
        private readonly IDependencyFilter _assemblyFilter;

        public ReflectionMetadataDependencyFinder(IDependencyFilter assemblyFilter)
        {
            _assemblyFilter = assemblyFilter ?? throw new ArgumentNullException(nameof(assemblyFilter));
        }

        public IDependencyInfo FindDependencies(IEnumerable<IAssemblyFile> files, IProgressReporter _progressReporter)
        {
            var inputAssemblyPaths = files.Where(f => FilterValidFiles(f, _progressReporter)).ToList();

            using (var task = _progressReporter.StartTask(LocalizedStrings.DetectingAssemblyReferences))
            {
                try
                {
                    return ReflectionMetadataDependencyInfo.ComputeDependencies(files, _assemblyFilter, _progressReporter);
                }
                catch (Exception)
                {
                    task.Abort();

                    throw;
                }
            }
        }

        private static bool FilterValidFiles(IAssemblyFile file, IProgressReporter _progressReporter)
        {
            if (file.Exists)
            {
                return true;
            }

            _progressReporter.ReportIssue(string.Format(CultureInfo.CurrentCulture, LocalizedStrings.UnknownFile, file.Name));

            return false;
        }
    }
}
