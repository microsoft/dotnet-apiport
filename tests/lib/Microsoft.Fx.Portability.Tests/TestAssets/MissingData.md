## 5: List<T>.ForEach

### Scope
### Version Introduced


### Change Description
Beginning in .NET 4.5, a List&lt;T&gt;.ForEach enumerator will throw an InvalidOperationException exception if an element in the calling collection is modified. Previously, this would not throw an exception but could lead to race conditions.

- [x] Quirked

### Recommended Action
Ideally, code should be fixed such that Lists are not modifed while enumerating their elements, as that is never a safe operation. To revert to the previous behavior, though, an app may target .NET 4.0.


[More information](https://msdn.microsoft.com/en-us/library/hh367887\(v=vs.110\).aspx#core)


### Aff

<!--
    ### Notes
    This introduces an exception, but requires retargeting
    Source analyzer status: Pri 1, source and binary done (MikeRou)


