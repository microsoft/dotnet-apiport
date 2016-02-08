### Recommended Action

Use the async alternatives AuthenticateAsClientAsync and AuthenticateAsServerAsync.

### Affected APIs

* `M:System.Net.Security.SslStream.AuthenticateAsClient(System.String)`
* `M:System.Net.Security.SslStream.AuthenticateAsClient(System.String,System.Security.Cryptography.X509Certificates.X509CertificateCollection,System.Security.Authentication.SslProtocols,System.Boolean)`
* `M:System.Net.Security.SslStream.AuthenticateAsServer(System.Security.Cryptography.X509Certificates.X509Certificate)`
* `M:System.Net.Security.SslStream.AuthenticateAsServer(System.Security.Cryptography.X509Certificates.X509Certificate,System.Boolean,System.Security.Authentication.SslProtocols,System.Boolean)`
