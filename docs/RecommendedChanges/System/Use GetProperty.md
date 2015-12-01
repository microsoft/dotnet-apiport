### Recommended Action
Use GetProperty(string, Type, Type[]) to search for public properties by name, return type, and parameter types or filter the results of GetMethods(BindingFlags) using LINQ for other queries.

### Affected APIs
* `M:System.Type.GetProperty(System.String,System.Reflection.BindingFlags,System.Reflection.Binder,System.Type,System.Type[],System.Reflection.ParameterModifier[])`
* `M:System.Type.GetProperty(System.String,System.Type,System.Type[],System.Reflection.ParameterModifier[])`
