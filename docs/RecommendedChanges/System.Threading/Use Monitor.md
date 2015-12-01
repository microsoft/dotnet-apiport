### Recommended Action
If you need to synchronize destruction of the timer with execution of the callback, do this yourself via Monitor or other synchronization mechanism.  If not, simply use Dispose().

### Affected APIs
* `M:System.Threading.Timer.Dispose(System.Threading.WaitHandle)`
