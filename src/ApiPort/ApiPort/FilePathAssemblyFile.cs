// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.IO;

namespace Microsoft.Fx.Portability
{
    public class FilePathAssemblyFile : IAssemblyFile
    {
        private readonly string _path;

        public FilePathAssemblyFile(string path)
        {
            _path = path;
        }

        public string Name => _path;

        public bool Exists => File.Exists(_path);

        public string Version
        {
            get
            {
                try
                {
                    return FileVersionInfo.GetVersionInfo(_path).FileVersion;
                }
                catch (ArgumentException)
                {
                    // Temporary workaround for CoreCLR-on-Linux bug (dotnet/corefx#4727) that prevents get_FileVersion from working on that platform
                    // This bug is now fixed and the correct behavior should be present in .NET Core RC2
                    return new Version(0, 0).ToString();
                }
            }
        }

        public Stream OpenRead() => File.OpenRead(_path);
    }
}
