param(
    [Parameter(Mandatory=$true)][string]$SourceDirectory,
    [switch]$IsDebug
)

Import-Module $(Join-Path $PSScriptRoot "build-utilities.psm1") -Verbose -ErrorAction Stop

Write-Output "Installing dnx and dnu..."

$dnxPath = Join-Path "$env:USERPROFILE" ".dnx"
$env:Path="$env:Path;$dnxPath\bin"

if (!(Test-Path $dnxPath -PathType Container))
{
    Invoke-WebRequest https://raw.githubusercontent.com/aspnet/Home/dev/dnvminstall.ps1 | Invoke-Expression
}

# Remove KRE_HOME if it still set - causes problems with DNX related commands
Remove-Item Env:\KRE_HOME -ErrorAction Ignore

Write-Output "Restoring project.json..."

# Get all of the projects with a project.json so we can restore packages.
foreach ($file in $(Get-ChildItem $SourceDirectory project.json -Recurse))
{
    Write-Output $file

    $directory = $file.DirectoryName

    pushd $directory
    dnu restore
    popd
}

## Restore packages
$nuget = Invoke-DownloadNuget

& $nuget restore "$PSScriptRoot\..\PortabilityTools.sln"
