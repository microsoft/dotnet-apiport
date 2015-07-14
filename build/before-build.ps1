param(
    [Parameter(Mandatory=$true)][string]$SourceDirectory,
    [switch]$IsDebug
)

Import-Module $(Join-Path $PSScriptRoot "build-utilities.psm1") -Verbose -ErrorAction Stop

$dnxPath = Join-Path "$env:USERPROFILE" ".dnx"
$dnxBin = Join-Path $dnxPath "bin"
$env:Path = "$env:Path;$dnxBin"

# Remove KRE_HOME if it still set - causes problems with DNX related commands
Remove-Item Env:\KRE_HOME -ErrorAction Ignore

if (!(Test-Path $dnxPath -PathType Container))
{
    Write-Output "Installing dnx and dnu..."

    Invoke-WebRequest https://raw.githubusercontent.com/aspnet/Home/dev/dnvminstall.ps1 | Invoke-Expression
    & dnvm upgrade

    $defaultRuntime = Get-Content $dnxPath\alias\default.txt -ErrorAction Stop
    $dnxRuntime = "$dnxPath\runtimes\$defaultRuntime\bin"

    $env:Path = "$env:Path;$dnxRuntime"

    Write-Host "Path after DNX install: [$($env:Path)]"
}

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
