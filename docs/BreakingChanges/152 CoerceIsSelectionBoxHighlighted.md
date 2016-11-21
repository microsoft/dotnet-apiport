## 152: CoerceIsSelectionBoxHighlighted

### Scope
Minor

### Version Introduced
4.6

### Version Reverted
4.6.2

### Source Analyzer Status
Planned

### Change Description
Certain sequences of actions involving a ComboBox and its data source can result in a
NullReferenceException in ComboBox.CoerceIsSelectionBoxHighlighted.

- [ ] Quirked // Uses some mechanism to turn the feature on or off, usually using runtime targeting, AppContext or config files. Needs to be turned on automatically for some situations.
- [ ] Build-time break // Causes a break if attempted to recompile

### Recommended Action
If possible, please upgrade to .NET 4.6.2.

### Affected APIs
* `M:System.Windows.Controls.ComboBox.CoerceIsSelectionBoxHighlighted`

### Category
Windows Presentation Foundation (WPF)

<!--
    ### Original Bug
    125219
-->


