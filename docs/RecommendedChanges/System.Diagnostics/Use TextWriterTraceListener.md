### Recommended Action
UseErrorStream ? new TextWriterTraceListener(Console.Error) : new TextWriterTraceListener(Console.Out).

### Affected APIs
* `T:System.Diagnostics.ConsoleTraceListener`
