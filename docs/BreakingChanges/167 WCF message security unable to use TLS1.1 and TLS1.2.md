## 167: WCF message security unable to use TLS1.1 and TLS1.2

### Scope
Edge

### Version Introduced
4.7

### Source Analyzer Status
Planned

### Change Description
.NET 4.7 gave customer the ability to configure either TLS1.1 or TLS1.2 in WCF message security in addition to SSL3.0 and TLS1.0 through the appsettings. 

- [x] Quirked
- [ ] Build-time break

### Recommended Action
Prior to 4.7, this change is disabled by default. You can enable this fix by adding the following line to the `<runtime>` section of the app.config or web.config file:

   ```xml
   <runtime>
      <AppContextSwitchOverrides value="Switch.System.ServiceModel.DisableUsingServicePointManagerSecurityProtocols=false;Switch.System.Net.DontEnableSchUseStrongCrypto=false" />
   </runtime>
   ```   


### Affected APIs
* `P:System.ServiceModel.Security.TlsnegoTokenAuthenticator.CreateSspiState`
* `P:System.ServiceModel.TlsnegoTokenProvider.CreateTlsSspiState`


### Category
* Windows Communication Foundation (WCF)