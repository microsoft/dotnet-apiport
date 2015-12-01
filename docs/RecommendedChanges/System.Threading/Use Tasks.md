### Recommended Action
Use Task.Run(), or Task.Factory.StartNew(..., TaskCreationOptions.LongRunning) if long-running.

### Affected APIs
* `M:System.Threading.Thread.#ctor(System.Threading.ParameterizedThreadStart,System.Int32)`
