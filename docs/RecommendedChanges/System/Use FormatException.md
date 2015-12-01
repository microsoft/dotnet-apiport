### Recommended Action
Use FormatException instead.  If you need to differentiate, can check ex.GetType().ToString() == "System.UriFormatException".

### Affected APIs
* `T:System.UriFormatException`
