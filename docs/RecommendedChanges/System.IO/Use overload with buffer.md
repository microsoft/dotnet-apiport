### Recommended Action
Use an overload that supplies a buffer and maintain that buffer outside of memory stream.   If you need the internal buffer, create a copy via .ToArray().

### Affected APIs
* `M:System.IO.MemoryStream.GetBuffer`
