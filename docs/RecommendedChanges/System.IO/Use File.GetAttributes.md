### Recommended Action
Use File.GetAttributes(), check (fileInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly.

### Affected APIs
* `P:System.IO.FileInfo.IsReadOnly`
* `M:System.IO.FileInfo.get_IsReadOnly`
