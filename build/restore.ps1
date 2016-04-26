$root = $PSScriptRoot
$nuget = & "$root\Get-Nuget.ps1"

# Restoring NuGet packages into this specific directory because the automated
# build definition in VSTS needs to be able to find the Visual Studio Test Adapter 
# (xunit.runner.visualstudio) in a known location
if($env:NuGetPackagesDirectory)
{
	& $nuget restore $root\..\PortabilityTools.sln -PackagesDirectory $env:NuGetPackagesDirectory
}
else
{
	& $nuget restore $root\..\PortabilityTools.sln
}

# Restore data for offline lib (cannot do this in the csproj due to race conditions)
& $root\Get-CatalogFile.ps1 $root\..\.data\catalog.bin