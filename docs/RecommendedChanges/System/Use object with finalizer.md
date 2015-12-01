### Recommended Action
Use object with finalizer stored in static variable, or use app-model specific unload notifications (e.g. Application.Suspending event for modern apps).

### Affected APIs
* `M:System.AppDomain.add_DomainUnload(System.EventHandler)`
* `M:System.AppDomain.add_ProcessExit(System.EventHandler)`
* `M:System.AppDomain.IsFinalizingForUnload`
* `M:System.AppDomain.remove_DomainUnload(System.EventHandler)`
