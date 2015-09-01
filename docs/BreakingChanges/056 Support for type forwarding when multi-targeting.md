## 56: Support for type forwarding when multi-targeting

### Scope
Edge

### Version Introduced
4.5

### Change Description
A new CodeDOM feature allows a compiler to compile against the targeted version of mscorlib.dll instead of the .NET Framework 4.5 version of mscorlib.dll. 

- [ ] Quirked
- [ ] Build-time break
- [ ] Source analyzer planned

### Recommended Action
This change prevents compiler warnings (and compilation failure in cases where warnings are treated as errors) when CodeDOM finds two definitions for types that have been type-forwarded. This change may have unintended side effects only if different versions of reference assemblies are mixed in a single location.

### Affected APIs
* Not detectable via API analysis

[More information](https://msdn.microsoft.com/en-us/library/hh367887(v=vs.110).aspx#core)
