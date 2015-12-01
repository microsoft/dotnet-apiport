### Recommended Action
Depends on the subclass of System.Net.Http.HttpContent used within the System.Net.Http.HttpRequestMessage object.  Both buffered and unbuffered are supported.

### Affected APIs
* `M:System.Net.HttpWebRequest.get_AllowWriteStreamBuffering`
* `M:System.Net.HttpWebRequest.set_AllowWriteStreamBuffering(System.Boolean)`
