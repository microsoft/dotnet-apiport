## 13: Missing TFM results in 4.0 behavior

### Scope
Major

### Version Introduced
4.5

### Source Analyzer Status
Available

### Change Description
Applications without a <a href="https://msdn.microsoft.com/en-us/library/system.runtime.versioning.targetframeworkattribute%28v=vs.110%29.aspx">TargetFrameworkAttribute</a> applied at the assembly level will automatically run using the semantics (quirks) of the .NET Framework 4.0. To ensure high quality, it is recommended that all binaries be explicitly attributed with a TargetFrameworkAttribute indicating the version of the .NET Framework they were built with. Note that using a target framework moniker in a project file will caues MSBuild to automatically apply a TargetFrameworkAttribute.

- [ ] Quirked
- [ ] Build-time break

### Recommended Action
A <a href="https://msdn.microsoft.com/en-us/library/system.runtime.versioning.targetframeworkattribute%28v=vs.110%29.aspx">target framework attribute</a> should be supplied, either through adding the attribute directly to the assembly or by specifying a target framework in the <a href="http://blogs.msdn.com/b/visualstudio/archive/2010/05/19/visual-studio-managed-multi-targeting-part-1-concepts-target-framework-moniker-target-framework.aspx">project file or through Visual Studio's project properties GUI</a>.

### Affected APIs
* Not detectable via API analysis
