// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if FEATURE_ASSEMBLY_LOCATION
using System.IO;
using System.Reflection;
#endif

namespace Microsoft.Fx.Portability.ObjectModel
{
    public class FileIgnoreAssemblyInfoList : IgnoreAssemblyInfoList
    {
        private const string DEFAULT_IGNORE_ASSEMBLIES_FILE = "KnownSafeBreaks.json";

        public FileIgnoreAssemblyInfoList(IApiPortOptions options)
        {
#if FEATURE_ASSEMBLY_LOCATION
            var noDefaultIgnoreFile = options.RequestFlags.HasFlag(AnalyzeRequestFlags.NoDefaultIgnoreFile);

            if (!noDefaultIgnoreFile)
            {
                LoadJsonFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), DEFAULT_IGNORE_ASSEMBLIES_FILE));
            }
#endif
            var ignoredAssemblyFiles = options.IgnoredAssemblyFiles;

            if (ignoredAssemblyFiles != null)
            {
                foreach (string ignoreFile in ignoredAssemblyFiles)
                {
                    LoadJsonFile(ignoreFile);
                }
            }
        }
    }
}
