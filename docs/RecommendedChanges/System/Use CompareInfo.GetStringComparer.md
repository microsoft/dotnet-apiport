### Recommended Action
Use `System.Globalization.CultureInfo.InvariantCulture.CompareInfo.GetStringComparer(CompareOptions.IgnoreCase)` from the `System.Globalization.Extensions` package.

### Affected APIs
* `M:System.StringComparer.Create(System.Globalization.CultureInfo,System.Boolean)`
* `M:System.StringComparer.get_InvariantCultureIgnoreCase`
