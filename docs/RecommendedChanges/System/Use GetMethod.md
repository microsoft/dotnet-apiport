### Recommended Action
Use GetMethod(string, Type[]) to search for public methods by name and parameter type or filter the results of GetMethods(BindingFlags) using LINQ for other queries.

### Affected APIs
* `M:System.Type.GetMethod(System.String,System.Reflection.BindingFlags,System.Reflection.Binder,System.Reflection.CallingConventions,System.Type[],System.Reflection.ParameterModifier[])`
* `M:System.Type.GetMethod(System.String,System.Reflection.BindingFlags,System.Reflection.Binder,System.Type[],System.Reflection.ParameterModifier[])`
* `M:System.Type.GetMethod(System.String,System.Type[],System.Reflection.ParameterModifier[])`
