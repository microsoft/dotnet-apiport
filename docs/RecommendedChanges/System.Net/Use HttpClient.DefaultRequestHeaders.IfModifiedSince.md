### Recommended Action
Use System.Net.Http.HttpClient.DefaultRequestHeaders.IfModifiedSince (applies to all requests made by that client) or System.Net.Http.HttpRequestMessage.Headers.IfModifiedSince (applies to specific request).

### Affected APIs
* `M:System.Net.HttpWebRequest.get_IfModifiedSince`
* `M:System.Net.HttpWebRequest.set_IfModifiedSince(System.DateTime)`
