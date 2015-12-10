// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.Fx.Portability.Reporting
{
    public class WindowsFileSystem : IFileSystem
    {
        public virtual string ChangeFileExtension(string filename, string newExtension)
        {
            return Path.ChangeExtension(filename, newExtension);
        }

        public virtual string CombinePaths(params string[] paths)
        {
            return Path.Combine(paths);
        }

        public virtual bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public virtual IEnumerable<string> FilesInDirectory(string directoryPath)
        {
            return Directory.EnumerateFiles(directoryPath);
        }

        public virtual string GetDirectoryNameFromPath(string path)
        {
            return Path.GetDirectoryName(path);
        }

        public virtual string GetFileExtension(string filename)
        {
            return Path.GetExtension(filename);
        }

        public virtual Stream CreateFile(string path)
        {
            return File.Open(path, FileMode.Create, FileAccess.ReadWrite);
        }

        public virtual IEnumerable<string> SearchPathForFile(string filename)
        {
            var pathDirs = (Environment.GetEnvironmentVariable("path") ?? string.Empty)
                           .Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            var hypotheticalPaths = from directory in pathDirs
                                    select Path.Combine(directory, filename);

            var files = from path in hypotheticalPaths
                        where File.Exists(path)
                        select path;

            return files;
        }
    }
}