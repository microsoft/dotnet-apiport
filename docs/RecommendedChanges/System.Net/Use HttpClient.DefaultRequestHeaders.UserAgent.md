### Recommended Action
Use System.Net.Http.HttpClient.DefaultRequestHeaders.UserAgent (applies to all requests made by that client) or System.Net.Http.HttpRequestMessage.Headers.UserAgent (applies to specific request).

### Affected APIs
* `M:System.Net.HttpWebRequest.get_UserAgent`
* `M:System.Net.HttpWebRequest.set_UserAgent(System.String)`
