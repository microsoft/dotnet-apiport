### Recommended Action
Use other Stream types.  Most stream implementations in .Net framework already buffer their content, which is preferable to wrapping in a buffered stream.

### Affected APIs
* `T:System.IO.BufferedStream`
