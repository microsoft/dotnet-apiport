[CmdletBinding()] # Needed to support -Verbose
param(
    [Parameter(Position = 0, Mandatory=$true)]
    [ValidateSet("Release","Debug")]
    [string]$Configuration,

    [Parameter(Position = 1, Mandatory=$true)]
    [ValidateSet("AnyCPU", "x86", "x64")]
    [string]$Platform,

    [Parameter(Position = 2)]
    [ValidateSet("quiet", "minimal", "normal","diagnostic")]
    [string]$Verbosity = "normal",

    [switch]$RunTests,
    [switch]$CreateNugetPackages,

    [string]$VersionSuffix = "alpha",
    
    [ValidateSet(2015, 2017)]
    [int]$VisualStudioVersion = 2017
)

$ErrorActionPreference = "Stop"

$root = $PSScriptRoot

& $root\build\Set-VsDevEnv.ps1 -VisualstudioVersion $VisualStudioVersion

$MSBuildCommand = $(Get-Command "MSBuild.exe" -CommandType Application -ErrorAction Ignore)

if ($MSBuildCommand -eq $null) {
    Write-Error "Could not set visual studio $VisualStudioVersion environment and locate msbuild!"
}

if ($VisualStudioVersion -eq 2017) {
    $MSBuildVersion = 15
} elseif ($VisualStudioVersion -eq 2015) {
    $MSBuildVersion = 14
} else {
    Write-Error "This VisualStudio version [$VisualStudioVersion] is not recognized."
}


$MSBuildCommand = $MSBuildCommand | ? { $_.Version.Major -eq $MSBuildVersion } | Select -First 1

if ($MSBuildCommand -eq $null) {
    Write-Error "Could not locate MSBuild $MSBuildVersion using Visual Studio $VisualStudioVersion developer command prompt"
}

$MSBuild = $MSBuildCommand.Path

Write-Host "MSBUILD: $MSBuild"

# Libraries are currently pre-release
$env:VersionSuffix = $VersionSuffix

$binFolder = [IO.Path]::Combine("bin", $Configuration)

if (!(Test-Path $binFolder)) {
    New-Item $binFolder -ItemType Directory
}

& $root\build\restore.ps1
# PortabilityTools.sln understands "Any CPU" not "AnyCPU"
$PlatformToUse = $Platform

if ($Platform -eq "AnyCPU") {
    $PlatformToUse = "Any CPU"
}

pushd $root

& $MSBuild PortabilityTools.sln /p:Configuration=$Configuration /p:Platform="$PlatformToUse" /nologo /m /v:m /nr:false /flp:logfile=$binFolder\msbuild.log`;verbosity=$Verbosity

popd

if ($RunTests) {
    .\build\runtests.ps1 $Configuration 
}

if ($CreateNugetPackages) {
    .\build\postbuild.ps1 $Configuration
}