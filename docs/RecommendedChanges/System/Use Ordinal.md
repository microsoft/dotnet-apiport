### Recommended Action
It is almost always incorrect to using InvariantCulture for string equality, see http://msdn.microsoft.com/en-us/library/dd465121(v=vs.110).aspx. The most common replacement is StringComparison.Ordinal, otherwise, if you need InvariantCulture, use CultureInfo.InvariantCulture.ComparerInfo.GetStringComparer(CompareOptions).

### Affected APIs
* `F:System.StringComparison.InvariantCulture`
