## 10: System.Uri escaping now supports RFC 3986 (http://tools.ietf.org/html/rfc3986)

### Scope
Minor

### Version Introduced
4.5

### Change Description
URI escaping has changed in .NET 4.5 to support <a href="http://tools.ietf.org/html/rfc3986">RFC 3986</a>. Specific changes include:<ul><li>EscapeDataString  escapes reserved characters based on <a href="http://tools.ietf.org/html/rfc3986">RFC 3986</a>.</li><li>EscapeUriString  does not escape reserved characters.</li><li>UnescapeDataString  does not throw an exception if it encounters an invalid escape sequence.</li><li>Unreserved escaped characters are un-escaped.</li></ul>

- [ ] Quirked
- [ ] Build-time break
- [x] Source analyzer available

### Recommended Action
* Update applications to not rely on UnescapeDataString to throw in the case of an invalid escape sequence. Such sequences must be detected directly now. 
* Similarly, expect that Escaped and Unescaped URI and Data strings may vary from .NET 4.0 and .NET 4.5 and should not be compared across .NET versions directly. Instead, they should be parsed and normalized in a single .NET version before any comparisons are made.

### Affected APIs
* M:System.Uri.EscapeDataString(System.String)
* M:System.Uri.EscapeUriString(System.String)
* M:System.Uri.UnescapeDataString(System.String)

[More information](https://msdn.microsoft.com/en-us/library/hh367887\(v=vs.110\).aspx#core)

<!--
    ### Notes
    Source analyzer status: Pri 1, Done
-->


