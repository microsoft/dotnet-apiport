### Recommended Action
Use System.Net.Http.HttpClient.Timeout. More granular timeouts are also available: System.Net.Http.WinHttpHandler.ConnectTimout, .ReceiveTimeout, .SendTimeout.

### Affected APIs
* `M:System.Net.WebRequest.get_Timeout`
* `M:System.Net.WebRequest.set_Timeout(System.Int32)`
