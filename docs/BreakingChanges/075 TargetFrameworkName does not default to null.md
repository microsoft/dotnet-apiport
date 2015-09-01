## 75: TargetFrameworkName

### Scope
Edge

### Version Introduced
4.6

### Change Description
The TargetFrameworkName was previously null in the default app domain, unless it was explicitly set. Beginning in 4.6, the TargetFrameworkName property for the default app domain will have a default value derived from the TargetFrameworkAttribute (if one is present). Non-default app domains will continue to inherit their TargetFrameworkName from the default app domain (which will not default to null in 4.6) unless it is explicitly overridden.

- [ ] Quirked
- [ ] Build-time break
- [x] Source analyzer planned

### Recommended Action
Code should be updated to not depend on `AppDomainSetup.TargetFrameworkName` defaulting to null. If it is required that this property continue to evaluate to null, it can be explicitly set to that value.

### Affected APIs
* `M:System.AppDomainSetup.get_TargetFrameworkName`

[More information](https://msdn.microsoft.com/en-us/library/dn833125(v=vs.110).aspx#Core)
