// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

#if DESKTOP
using System.IO;
using System.Reflection;
#endif

namespace Microsoft.Fx.Portability.ObjectModel
{
    public class FileIgnoreAssemblyInfoList : IgnoreAssemblyInfoList
    {
        private const string DEFAULT_IGNORE_ASSEMBLIES_FILE = "KnownSafeBreaks.json";

        public FileIgnoreAssemblyInfoList(bool noDefaultIgnoreFile, IEnumerable<string> ignoredAssemblyFiles)
        {
#if DESKTOP
            if (!noDefaultIgnoreFile)
            {
                LoadJsonFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), DEFAULT_IGNORE_ASSEMBLIES_FILE));
            }
#endif
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
