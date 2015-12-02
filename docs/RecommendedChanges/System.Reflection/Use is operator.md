### Recommended Action
Use typeof(), "is FieldInfo", "is PropertyInfo" to reason about the Member's type. Example: if (membertype == membertypes.Field) --> if (member is FieldInfo).

### Affected APIs
* `T:System.Reflection.MemberTypes`
