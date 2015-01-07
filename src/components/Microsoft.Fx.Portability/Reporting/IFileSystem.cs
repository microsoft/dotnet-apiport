using System.Collections.Generic;
using System.IO;

namespace Microsoft.Fx.Portability.Reporting
{
    public interface IFileSystem
    {
        string ChangeFileExtension(string filename, string newExtension);
        string CombinePaths(params string[] paths);
        bool FileExists(string path);
        IEnumerable<string> FilesInDirectory(string directoryPath);
        string GetDirectoryNameFromPath(string path);
        string GetFileExtension(string filename);
        Stream CreateFile(string path);
        IEnumerable<string> SearchPathForFile(string filename);
    }
}