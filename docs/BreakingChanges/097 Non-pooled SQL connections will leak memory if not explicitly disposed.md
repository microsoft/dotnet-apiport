## 97: Non-pooled SQL connections will leak memory if not explicitly disposed

### Scope
Edge

### Version Introduced
4.5

### Version Reverted
4.5

### Change Description
In the .NET Framework 4.5, non-pooled SQL connections which are not explicitly exposed (via Dispose, Close, or using) will leak memory

- [ ] Quirked
- [ ] Build-time break
- [ ] Source analyzer planned

### Recommended Action
This issue is fixed in a .NET Framework 4.5 servicing update. Please update the .NET Framework 4.5, or upgrade to .NET Framework 4.5.1 or later, to fix this issue. Alternatively, this issue may be avoided by using the SqlConnection in a 'using' pattern (which is a best practice) or by explicitly calling Dispose or Close when the connection is no longer needed.

### Affected APIs
* `M:System.Data.SqlClient.SqlConnection.#ctor(System.String)`
* `M:System.Data.SqlClient.SqlConnection.#ctor(System.String,System.Data.SqlClient.SqlCredential)`

[More information](https://support.microsoft.com/kb/2748720)
