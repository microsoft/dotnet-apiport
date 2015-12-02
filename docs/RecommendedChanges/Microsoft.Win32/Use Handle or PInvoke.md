### Recommended Action
Use Handle property to get ACLs or PInvoke to RegGetSecurity (http://msdn.microsoft.com/en-us/library/windows/desktop/ms724878(v=vs.85).aspx).

### Affected APIs
* `M:Microsoft.Win32.RegistryKey.GetAccessControl`
* `M:Microsoft.Win32.RegistryKey.SetAccessControl(System.Security.AccessControl.RegistrySecurity)`
