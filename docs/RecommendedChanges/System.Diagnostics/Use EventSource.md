### Recommended Action
Use EventSource type instead which now supports channels, enabling logging ETW events to the event log.  Currently there is no other workaround, but we are working on it. Please check back.

### Affected APIs
* `T:System.Diagnostics.EventLogEntryType`
* `M:System.Diagnostics.EventLog.#ctor`
* `M:System.Diagnostics.EventLog.set_Log(System.String)`
* `M:System.Diagnostics.EventLog.set_Source(System.String)`
* `M:System.Diagnostics.EventLog.WriteEvent(System.Diagnostics.EventInstance,System.Object[])`
