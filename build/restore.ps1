$root = $PSScriptRoot
$packages = "$root\..\packages"

$nuget = & "$root\Get-Nuget.ps1"

New-Item $packages -ItemType Directory -ErrorAction Ignore

# Restoring NuGet packages into this specific directory because the automated
# build definition needs to be able to find the Visual Studio Test Adapter 
# (xunit.runner.visualstudio) in a known location
& $nuget restore $root\..\PortabilityTools.sln -PackagesDirectory $packages

# Restore data for offline lib (cannot do this in the csproj due to race conditions)
& $root\Get-CatalogFile.ps1 $root\..\.data\catalog.bin