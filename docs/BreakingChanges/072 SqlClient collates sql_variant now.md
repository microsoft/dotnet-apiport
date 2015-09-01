## 72: SqlClient collates sql_variant now

### Scope
Transparent

### Version Introduced
4.5

### Change Description
`sql_variant` data uses `sql_variant` collation rather than database collation. 

- [ ] Quirked
- [ ] Build-time break
- [ ] Source analyzer planned

### Recommended Action
This change addresses possible data corruption if the database collation differs from the `sql_variant` collation. Applications that rely on the corrupted data may experience failure. 

### Affected APIs
* Not detectable via API analysis

[More information](https://msdn.microsoft.com/en-us/library/hh367887(v=vs.110).aspx#xml)
