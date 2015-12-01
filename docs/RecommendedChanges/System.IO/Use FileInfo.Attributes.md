### Recommended Action
Use `fileInfo.Attributes = fileInfo.Attributes | FileAttributes.ReadOnly` to set read-only and `fileInfo.Attributes = fileInfo.Attributes & ~FileAttributes.ReadOnly` to clear it.

### Affected APIs
* `M:System.IO.FileInfo.set_IsReadOnly(System.Boolean)`
