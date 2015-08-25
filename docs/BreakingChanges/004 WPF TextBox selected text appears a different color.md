## 4: WPF TextBox selected text appears a different color when the text box is inactive

### Scope
Edge

### Version Introduced
4.5

### Change Description
In .NET 4.5, when a WPF text box control is inactive (it doesn't have focus), the selected text inside the box will appear a different color than when the control is active.

- [ ] Quirked
- [ ] Build-time break
- [x] Source analyzer available

### Recommended Action
Previous (.NET 4.0) behavior may be restored by setting the <a href="https://msdn.microsoft.com/en-us/library/system.windows.frameworkcompatibilitypreferences.areinactiveselectionhighlightbrushkeyssupported(v=vs.110).aspx">FrameworkCompatibilityPreferences.AreInactiveSelectionHighlightBrushKeysSupported</a> property to false.

### Affected APIs
* `T:System.Windows.Controls.TextBox`

[More information](https://msdn.microsoft.com/en-us/library/hh367887\(v=vs.110\).aspx#wpf)
