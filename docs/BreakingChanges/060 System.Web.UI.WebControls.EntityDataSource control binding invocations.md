## 60: System.Web.UI.WebControls.EntityDataSource control

### Scope
Edge

### Version Introduced
4.5

### Source Analyzer Status
Planned

### Change Description
The `Page.LoadComplete` event no longer causes the System.Web.UI.WebControls.EntityDataSource control to invoke data binding for changes to create/update/delete parameters. 
This change eliminates an extraneous trip to the database, prevents the values of controls from being reset, and produces behavior that is consistent with other data controls, such as SqlDataSource and ObjectDataSource. 
This change produces different behavior in the unlikely event that applications rely on invoking data binding in the `Page.LoadComplete` event. 

- [ ] Quirked
- [ ] Build-time break

### Recommended Action
TODO

### Affected APIs
* Not detectable via API analysis - API NOT IN DATABASE

[More information](https://msdn.microsoft.com/en-us/library/hh367887(v=vs.110).aspx#asp)

<!--
    ### Notes
    This change produces different behavior in the unlikely event that applications rely on invoking data binding in the Page.LoadComplete event.
-->


