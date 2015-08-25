## 14: enableViewStateMac

### Scope
Major

### Version Introduced
4.5.2

### Change Description
ASP.NET no longer allows developers to specify &lt;pages enableViewStateMac="false"/&gt; or &lt;@Page EnableViewStateMac="false" %&gt;. The view state message authentication code (MAC) is now enforced for all requests with embedded view state. Only apps that explicitly set the EnableViewStateMac property to false are affected.

- [ ] Quirked
- [ ] Build-time break
- [x] Source analyzer available

### Recommended Action
EnableViewStateMac must be assumed to be true, and any resulting MAC errors must be resolved (as explained in <a href="https://support.microsoft.com/en-us/kb/2915218">this</a> guidance, which contains multiple resolutions depending on the specifics of what is causing MAC errors).

### Affected APIs
* Not detectable via API analysis

[More information](https://msdn.microsoft.com/en-us/library/dn720774#ASP_NET)
