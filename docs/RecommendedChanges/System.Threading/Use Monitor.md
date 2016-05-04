### Recommended Action
If you need to synchronize destruction of the timer with execution of the callback, do this yourself via Monitor or other synchronization mechanism.  If not, simply use Dispose().  Code sample: http://aka.ms/porttocore_timer_dispose

### Affected APIs
* `M:System.Threading.Timer.Dispose(System.Threading.WaitHandle)`

### Replacement Code
The following class shows how you can wait for the callback of a timer to finish running. 

```C#
class StoppableTimer
{
    private readonly Timer _timer;
    private readonly Action _callback;
    private readonly object _lockObj = new object();
    private bool _running;

    public StoppableTimer(Action callback)
    {
        _callback = callback;
        _timer = new Timer(
            callback: s => ((StoppableTimer)s).FireTimer(), 
            state: this, 
            dueTime: Timeout.Infinite, 
            period: Timeout.Infinite);
    }

    public void Start(int durationMilliseconds)
    {
        _running = true;
        _timer.Change(
            dueTime: durationMilliseconds, 
            period: Timeout.Infinite); 
    }

    private void FireTimer()
    {
        lock (_lockObj)
        {
            if (_running)
            {
                _callback();
            }
        }
    }

    /// <summary>
    /// Stops the timer and synchronously waits for any running callbacks to finish running
    /// </summary>    
    public void Stop()
    {
        lock (_lockObj)
        {
            // FireTimer is *not* running _callback (since we got the lock)

            _timer.Change(
                dueTime: Timeout.Infinite, 
                period: Timeout.Infinite);

            _running = false;
        }
        // Now FireTimer will *never* run _callback
    }

    /// <summary>
    /// Stops the timer and returns a Task that will complete when any running callbacks finish running
    /// </summary>
    public Task StopAsync()
    {
        return Task.Factory.StartNew(
            action: s => ((StoppableTimer)s).Stop(), 
            state: this, 
            cancellationToken: CancellationToken.None, 
            creationOptions: TaskCreationOptions.DenyChildAttach, 
            scheduler: TaskScheduler.Default);
    }
}
```