// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Microsoft.Fx.Portability.ObjectModel
{
    public class FileIgnoreAssemblyInfoList : IgnoreAssemblyInfoList
    {
        private const string DEFAULT_IGNORE_ASSEMBLIES_FILE = "KnownSafeBreaks.json";

        public FileIgnoreAssemblyInfoList(bool noDefaultIgnoreFile, IEnumerable<string> ignoredAssemblyFiles)
        {
            if (!noDefaultIgnoreFile)
            {
                LoadJsonFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), DEFAULT_IGNORE_ASSEMBLIES_FILE));
            }
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
