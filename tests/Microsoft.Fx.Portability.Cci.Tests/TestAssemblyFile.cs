using System;
using System.IO;
using Microsoft.Fx.Portability;

namespace Microsoft.Fx.Portability.Cci.Tests
{
    internal class TestAssemblyFile : IAssemblyFile
    {
        internal FileInfo FileInfo { get; }

        public TestAssemblyFile(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            else if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Cannot be empty", nameof(path));
            }

            FileInfo = new FileInfo(path);
        }

        public bool Exists { get { return FileInfo.Exists; } }

        public string Name { get { return FileInfo.FullName; } }

        public string Version
        {
            get { return "1.0.0.0"; }
        }

        public Stream OpenRead()
        {
            return Exists ? FileInfo.OpenRead() : null;
        }
    }
}
