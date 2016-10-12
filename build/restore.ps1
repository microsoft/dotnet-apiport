[CmdletBinding(DefaultParameterSetName="All")]
param(
    [Parameter(Position = 0, Mandatory = "true", ParameterSetName="PublishVsix")]
    [string]$BuildNumber,

    [ValidateSet("build","revision")]
    [Parameter(Position = 1, Mandatory = "true", ParameterSetName="PublishVsix")]
    [string]$VersionType = "revision",

    [Parameter(Mandatory = "true", ParameterSetName="PublishVsix")]
    [switch]$PublishVsix
)

function Get-BuildNumber
{
    # The parsing regex is how our VSTS variables for build number is set
    $BuildNumberRegex = "dotnet-apiport\.(?<revision>[0-9]+)"
    
    if ($BuildNumber -match $BuildNumberRegex) 
    {
        return $Matches["revision"]
    }
    else 
    {
        return $null
    }
}

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

if ($IsPublishVsix)
{
    $number = Get-BuildNumber

    if ($number -eq $null) 
    {
        Write-Error "Could not get a revision from build number [$BuildNumber]"
        return -1
    }

    . $(& $buildToolScript "vsix")

    Vsix-IncrementVsixVersion -buildNumber $number -versionType $VersionType | Vsix-UpdateBuildVersion
}