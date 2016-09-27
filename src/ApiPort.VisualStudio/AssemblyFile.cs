// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Fx.Portability;

namespace ApiPortVS
{
    internal class AssemblyFile : IAssemblyFile
    {
        private readonly string _path;

        public AssemblyFile(string path)
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

    internal class AssemblyFileComparer : IComparer<IAssemblyFile>
    {
        public int Compare(IAssemblyFile x, IAssemblyFile y)
        {
            if (x == null)
            {
                return y == null ? 0 : -1;
            }


            return x.Name.CompareTo(y?.Name);
        }
    }
}
