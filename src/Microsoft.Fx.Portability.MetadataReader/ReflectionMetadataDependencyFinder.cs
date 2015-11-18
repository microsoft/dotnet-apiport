// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.Fx.Portability.Analyzer
{
    public class ReflectionMetadataDependencyFinder : IDependencyFinder
    {
        public IDependencyInfo FindDependencies(IEnumerable<FileInfo> inputAssemblies, IProgressReporter _progressReporter)
        {
            var inputAssemblyPaths = inputAssemblies.Where(f => FilterValidFiles(f, _progressReporter)).Select(i => i.FullName).ToList();

            using (var task = _progressReporter.StartTask(LocalizedStrings.DetectingAssemblyReferences, inputAssemblyPaths.Count))
            {
                try
                {
                    return ReflectionMetadataDependencyInfo.ComputeDependencies(inputAssemblyPaths, _progressReporter);
                }
                catch (Exception)
                {
                    task.Abort();

                    throw;
                }
            }
        }

        private static bool FilterValidFiles(FileInfo file, IProgressReporter _progressReporter)
        {
            if (file.Exists)
            {
                return true;
            }

            _progressReporter.ReportIssue(string.Format(LocalizedStrings.UnknownFile, file.FullName));

            return false;
        }
    }
}
