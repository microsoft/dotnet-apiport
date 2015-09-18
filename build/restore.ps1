$root = $PSScriptRoot
$nuget = & "$root\Get-Nuget.ps1"

& $nuget restore $root\..\PortabilityTools.sln

# Restore data for offline lib (cannot do this in the csproj due to race conditions)
& $root\Get-CatalogFile.ps1 $root\..\.data\catalog.bin