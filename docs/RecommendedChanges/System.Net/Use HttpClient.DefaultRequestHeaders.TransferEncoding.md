### Recommended Action
Use System.Net.Http.HttpClient.DefaultRequestHeaders.TransferEncoding (applies to all requests made by that client) or System.Net.Http.HttpRequestMessage.Headers.TransferEncoding(applies to specific request).

### Affected APIs
* `M:System.Net.HttpWebRequest.set_TransferEncoding(System.String)`
