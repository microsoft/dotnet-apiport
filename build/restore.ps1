[CmdletBinding(DefaultParameterSetName="All")]
param(
    [Parameter(Position = 0, Mandatory = "true", ParameterSetName="PublishVsix")]
    [int]$BuildNumber,

    [ValidateSet("build","revision")]
    [Parameter(Position = 1, Mandatory = "true", ParameterSetName="PublishVsix")]
    [string]$VersionType = "revision",

    [Parameter(Mandatory = "true", ParameterSetName="PublishVsix")]
    [switch]$PublishVsix
)

$ErrorActionPreference = "Stop"

$IsPublishVsix = $PsCmdlet.ParameterSetName -eq "PublishVsix"

$root = $PSScriptRoot
$buildToolScript = Join-Path $root "Get-BuildTools.ps1"
$nuget = & $buildToolScript "nuget"
$portabilitySolution = $(Resolve-Path "$root\..\PortabilityTools.sln").Path

# Restoring NuGet packages into this specific directory because the automated
# build definition in VSTS needs to be able to find the Visual Studio Test Adapter 
# (xunit.runner.visualstudio) in a known location
if($env:NuGetPackagesDirectory)
{
    & $nuget restore $portabilitySolution -PackagesDirectory $env:NuGetPackagesDirectory
}
else
{
    & $nuget restore $portabilitySolution
}

# Restore data for offline lib (cannot do this in the csproj due to race conditions)
& $root\Get-CatalogFile.ps1 $root\..\.data\catalog.bin

if ($IsPublishVsix) {

    . $(& $buildToolScript "vsix")

    Vsix-IncrementVsixVersion -buildNumber $BuildNumber -versionType $VersionType | Vsix-UpdateBuildVersion
}