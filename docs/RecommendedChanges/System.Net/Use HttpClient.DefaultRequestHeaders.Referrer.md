### Recommended Action
Use System.Net.Http.HttpClient.DefaultRequestHeaders.Referrer (applies to all requests made by that client) or System.Net.Http.HttpRequestMessage.Headers.Referrer (applies to specific request).

### Affected APIs
* `M:System.Net.HttpWebRequest.get_Referer`
