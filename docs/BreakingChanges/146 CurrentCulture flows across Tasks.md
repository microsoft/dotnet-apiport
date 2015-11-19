## 146: CurrentCulture and CurrentUICulture flow across tasks

### Scope
Minor

### Version Introduced
4.6

### Source Analyzer Status
Planned

### Change Description
Beginning in the .NET Framework 4.6, [CurrentCulture](https://msdn.microsoft.com/en-us/library/system.globalization.cultureinfo.currentculture%28v=vs.110%29.aspx) and [CurrentUICulture](https://msdn.microsoft.com/en-us/library/system.globalization.cultureinfo.currentuiculture%28v=vs.110%29.aspx) are stored in the thread's [ExecutionContext](https://msdn.microsoft.com/en-us/library/system.threading.executioncontext%28v=vs.110%29.aspx), which flows across asynchronous operations.

This means that changes to CurrentCulture or CurrentUICulture will be reflected in tasks which are later run asynchronously. This is different from the behavior of previous .NET Framework versions (which would reset CurrentCulture and CurrentUICulture in all asynchronous tasks).

- [x] Quirked
- [ ] Build-time break

### Recommended Action
Apps affected by this change may work around it by explicitly setting the desired CurrentCulture or CurrentUICulture as the first operation in an async Task. Alternatively, because the ExecutionContext change only affects apps targeting the .NET Framework 4.6 or newer, this break can be avoided by targeting the .NET Framework 4.5.2.

### Affected APIs
* `M:System.Globalization.CultureInfo.set_CurrentCulture(System.Globalization.CultureInfo)`
* `M:System.Threading.Thread.set_CurrentCulture(System.Globalization.CultureInfo)`
* `M:System.Globalization.CultureInfo.set_CurrentUICulture(System.Globalization.CultureInfo)`
* `M:System.Threading.Thread.set_CurrentUICulture(System.Globalization.CultureInfo)`

### Category
Core

[More information](https://msdn.microsoft.com/en-us/library/system.globalization.cultureinfo%28v=vs.110%29.aspx#Async)
