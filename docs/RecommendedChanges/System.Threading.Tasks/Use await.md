### Recommended Action
Don't use IAsyncResult, use "await" instead, and don't dispose Tasks.

### Affected APIs
* `M:System.Threading.Tasks.Task.Dispose`
