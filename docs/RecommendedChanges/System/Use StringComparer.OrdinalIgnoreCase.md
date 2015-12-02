### Recommended Action
It is almost always incorrect to using InvariantCulture for string equality, see http://msdn.microsoft.com/en-us/library/dd465121(v=vs.110).aspx. The most common replacement is StringComparer.OrdinalIgnoreCase, otherwise, if you need InvariantCulture, use CultureInfo.InvariantCulture.ComparerInfo.GetStringComparer(CompareOptions).

### Affected APIs
* `P:System.StringComparer.InvariantCultureIgnoreCase`
