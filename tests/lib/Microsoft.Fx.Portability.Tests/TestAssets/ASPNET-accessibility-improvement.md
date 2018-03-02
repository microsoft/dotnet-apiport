## ASP.NET Accessibility Improvements in .NET 4.7.1

### Scope
Minor

### Version Introduced
4.7.1

### Source Analyzer Status
NotPlanned

### Change Description
Starting with the .NET Framework 4.7.1, ASP.NET has improved how ASP.NET Web Controls work with accessibility technology in Visual Studio to better support ASP.NET customers.

- [x] Quirked
- [ ] Build-time break

### Recommended Action

In order for the Visual Studio Designer to benefit from these changes
- Install Visual Studio 2017 15.3 or later, which supports the new accessibility features with the following AppContext Switch by default.
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <runtime>
    ...
    <!-- AppContextSwitchOverrides value attribute is in the form of 'key1=true|false;key2=true|false  -->
    <AppContextSwitchOverrides value="...;Switch.UseLegacyAccessibilityFeatures=false" />
    ...
  </runtime>
</configuration>
```
