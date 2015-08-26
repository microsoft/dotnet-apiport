## 18: BlockingCollection<T>.TryTakeFromAny doesn't throw anymore

### Scope
Minor

### Version Introduced
4.5

### Change Description
If one of the input collections is marked completed, BlockingCollection&lt;T&gt;.TryTakeFromAny(BlockingCollection&lt;T&gt;[], T) no longer returns -1 and BlockingCollection&lt;T&gt;.TakeFromAny no longer throws an exception. This change makes it possible to work with collections when one of the collections is either empty or completed, but the other collection still has items that can be retrieved.

- [ ] Quirked
- [ ] Build-time break
- [x] Source analyzer available

### Recommended Action
If TryTakeFromAny returning -1 or TakeFromAny throwing were used for control-flow purposes in cases of a blocking collection being completed, such code should now be changed to use .Any(b =&gt; b.IsCompleted) to detect that condition.

### Affected APIs
* ``M:System.Collections.Concurrent.BlockingCollection`1.TakeFromAny(System.Collections.Concurrent.BlockingCollection{`0}[],`0@)``
* ``M:System.Collections.Concurrent.BlockingCollection`1.TakeFromAny(System.Collections.Concurrent.BlockingCollection{`0}[],`0@,System.Threading.CancellationToken)``
* ``M:System.Collections.Concurrent.BlockingCollection`1.TryTakeFromAny(System.Collections.Concurrent.BlockingCollection{`0}[],`0@)``
* ``M:System.Collections.Concurrent.BlockingCollection`1.TryTakeFromAny(System.Collections.Concurrent.BlockingCollection{`0}[],`0@,System.Int32)``
* ``M:System.Collections.Concurrent.BlockingCollection`1.TryTakeFromAny(System.Collections.Concurrent.BlockingCollection{`0}[],`0@,System.TimeSpan)``
* ``M:System.Collections.Concurrent.BlockingCollection`1.TryTakeFromAny(System.Collections.Concurrent.BlockingCollection{`0}``

[More information](https://msdn.microsoft.com/en-us/library/hh367887#core)
