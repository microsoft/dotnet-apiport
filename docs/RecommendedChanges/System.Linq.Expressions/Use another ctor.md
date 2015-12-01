### Recommended Action
Use a different constructor that does not take ExpressionType. Then override NodeType and Type properties to provide the values that would be specified to this constructor.

### Affected APIs
* `M:System.Linq.Expressions.Expression.#ctor(System.Linq.Expressions.ExpressionType,System.Type)`
