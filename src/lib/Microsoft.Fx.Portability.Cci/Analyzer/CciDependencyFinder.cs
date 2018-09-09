// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Cci.Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Microsoft.Fx.Portability.Analyzer
{
    public class CciDependencyFinder : IDependencyFinder
    {
        public IDependencyInfo FindDependencies(IEnumerable<IAssemblyFile> inputAssemblies, IProgressReporter progressReport)
        {
            var inputAssemblyPaths = inputAssemblies.Where(f => FilterValidFiles(f, progressReport)).Select(i => i.Name).ToList();

            using (var task = progressReport.StartTask(LocalizedStrings.DetectingAssemblyReferences, inputAssemblyPaths.Count))
            {
                try
                {
                    return DependencyFinderEngine.ComputeDependencies(inputAssemblyPaths, task);
                }
                catch (Exception)
                {
                    task.Abort();
                    throw;
                }
            }
        }

        private static bool FilterValidFiles(IAssemblyFile file, IProgressReporter progressReport)
        {
            if (file.Exists)
            {
                return true;
            }

            progressReport.ReportIssue(string.Format(CultureInfo.CurrentCulture, LocalizedStrings.UnknownFile, file.Name));

            return false;
        }
    }
}
