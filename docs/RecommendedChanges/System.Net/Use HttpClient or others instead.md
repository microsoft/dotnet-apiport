### Recommended Action
Use System.Net.Http.HttpClient, System.Net.Http.HttpClientHandler and System.Net.Http.WinHttpHandler instead.

### Affected APIs
* `T:System.Net.HttpContinueDelegate`
* `T:System.Net.HttpResponseHeader`
* `T:System.Net.HttpWebResponse`
* `T:System.Net.ServicePoint`
* `T:System.Net.WebResponse`
* `M:System.Net.WebHeaderCollection.set_Item(System.Net.HttpResponseHeader,System.String)`
* `M:System.Net.ServicePointManager.set_ServerCertificateValidationCallback(System.Net.Security.RemoteCertificateValidationCallback)`
