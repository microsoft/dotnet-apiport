### Recommended Action
Use AssemblyLoadContext. Evidence is not supported in .NET Core.

### Affected APIs
* `M:System.Reflection.Assembly.Load(System.Byte[])`
* `M:System.Reflection.Assembly.Load(System.Reflection.AssemblyName)`
* `M:System.Reflection.Assembly.Load(System.String)`
* `M:System.Reflection.Assembly.Load(System.Byte[],System.Byte[])`
* `M:System.Reflection.Assembly.Load(System.Byte[],System.Byte[],System.Security.Policy.Evidence)`
* `M:System.Reflection.Assembly.Load(System.Byte[],System.Byte[],System.Security.SecurityContextSource)`
* `M:System.Reflection.Assembly.Load(System.Reflection.AssemblyName,System.Security.Policy.Evidence)`
* `M:System.Reflection.Assembly.Load(System.String,System.Security.Policy.Evidence)`
* `M:System.Reflection.Assembly.LoadFrom(System.String)`
* `M:System.Reflection.Assembly.LoadFrom(System.String,System.Byte[],System.Configuration.Assemblies.AssemblyHashAlgorithm)`
* `M:System.Reflection.Assembly.LoadFrom(System.String,System.Security.Policy.Evidence)`
* `M:System.Reflection.Assembly.LoadFrom(System.String,System.Security.Policy.Evidence,System.Byte[],System.Configuration.Assemblies.AssemblyHashAlgorithm)`