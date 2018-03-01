## 6: System.Uri

### Scope
Major

### Version Introduced
4.5

### Source Analyzer Status
Available

### Change Description

URI parsing has changed in several ways in .NET 4.5. Note, however, that these changes only affect code targeting .NET 4.5. If a binary targets .NET 4.0, the old behavior will be observed.  
Changes to URI parsing in .NET 4.5 include:<ul><li>URI parsing will perform normalization and character checking according to the latest IRI rules in RFC 3987</li><li>Unicode normalization form C will only be performed on the host portion of the URI</li><li>Invalid mailto: URIs will now cause an exception</li><li>Trailing dots at the end of a path segment are now preserved</li><li>file:// URIs do not escape the '?' character</li><li>Unicode control characters U+0080 through U+009F are not supported</li><li>Comma characters (',' %2c) are not automatically unescaped</li></ul>

- [x] Quirked
- [ ] Build-time break

### Recommended Action
If the old .NET 4.0 URI parsing semantics are necessary (they often aren't), they can be used by targeting .NET 4.0. This can be accomplished by using a TargetFrameworkAttribute on the assembly, or through Visual Studio's project system UI in the 'project properties' page.

### Affected APIs
[More information](https://msdn.microsoft.com/en-us/library/hh367887\(v=vs.110\).aspx#core)

<!--
    ### Notes
    Changes IRI parsing, requires access to parameters to detect
    Source analyzer status: Pri 1, source done (AlPopa)
-->


