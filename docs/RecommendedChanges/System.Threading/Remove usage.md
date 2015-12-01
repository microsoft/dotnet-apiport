### Recommended Action
Portable code should not rely on COM apartment semantics, and so should avoid this type.

### Affected APIs
* `T:System.Threading.ApartmentState`
* `T:System.Threading.ThreadAbortException`
* `T:System.Threading.ThreadPriority`
* `M:System.Threading.Thread.Abort`
* `M:System.Threading.Thread.Abort(System.Object)`
* `M:System.Threading.Thread.GetApartmentState`
* `M:System.Threading.ThreadPool.GetMaxThreads(System.Int32@,System.Int32@)`
* `M:System.Threading.ThreadPool.GetMinThreads(System.Int32@,System.Int32@)`
* `M:System.Threading.Thread.get_Priority`
* `M:System.Threading.Thread.Interrupt`
* `M:System.Threading.ThreadPool.QueueUserWorkItem(System.Threading.WaitCallback)`
* `M:System.Threading.Thread.ResetAbort`
* `M:System.Threading.ThreadPool.SetMaxThreads(System.Int32,System.Int32)`
* `M:System.Threading.ThreadPool.SetMinThreads(System.Int32,System.Int32)`
* `M:System.Threading.Thread.set_Priority(System.Threading.ThreadPriority)`
