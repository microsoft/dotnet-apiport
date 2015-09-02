## 70: SqlConnection can no longer connect to SQL Server 1997 or databases using the VIA adapter

### Scope
Edge

### Version Introduced
4.5

### Source Analyzer Status
Planned

### Change Description
Connections to SQL Server databases using the Virtual Interface Adapter (VIA) protocol are no longer supported. If this app is connecting to SQL via a protocol other than VIA, then no breaking change will be encountered.

Also, connections to SQL Server 1997 are no longer supported.

- [ ] Quirked
- [ ] Build-time break

### Recommended Action
The VIA protocol is deprecated, so an alternative protocol should be used to connect to SQL databases. The most common protocol used is TCP/IP. Instructions for enabling the TCP/IP protocol can be found [here](https://msdn.microsoft.com/en-us/library/bb909712(v=vs.90).aspx). If the database is only accessed from within an intranet, the shared pipes protocol may provide better performance if the network is slow.

### Affected APIs
* `M:System.Data.SqlClient.SqlConnection.#ctor(System.String)`
* `M:System.Data.SqlClient.SqlConnection.#ctor(System.String,System.Data.SqlClient.SqlCredential)`

[More information](https://msdn.microsoft.com/en-us/library/hh367887%28v=vs.110%29.aspx#sql)
