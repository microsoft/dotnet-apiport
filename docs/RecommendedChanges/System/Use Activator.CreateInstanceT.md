### Recommended Action
Use Activator.CreateInstance<T>() if using public ctor, otherwise use reflection to get non-public ConstructorInfo and invoke it

### Affected APIs
* `M:System.Activator.CreateInstance(System.Type,System.Boolean)`