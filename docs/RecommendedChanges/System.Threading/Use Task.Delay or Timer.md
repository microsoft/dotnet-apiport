### Recommended Action
Use Task.Delay, or one of the other Timer .ctor overloads (passing Timeout.Infinite as needed).

### Affected APIs
* `M:System.Threading.Timer.#ctor(System.Threading.TimerCallback)`
