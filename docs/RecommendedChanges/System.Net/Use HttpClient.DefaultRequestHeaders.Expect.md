### Recommended Action
Use System.Net.Http.HttpClient.DefaultRequestHeaders.Expect (applies to all requests made by that client) or System.Net.Http.HttpRequestMessage.Headers.Expect (applies to specific request).

### Affected APIs
* `M:System.Net.HttpWebRequest.set_Expect(System.String)`
