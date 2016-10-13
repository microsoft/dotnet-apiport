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

    [string]$VersionSuffix = "alpha"
)

$ErrorActionPreference = "Stop"

# Libraries are currently pre-release
$env:VersionSuffix = $VersionSuffix

$binFolder = [IO.Path]::Combine("bin", $Configuration)

if (!(Test-Path $binFolder)) {
    New-Item $binFolder -ItemType Directory
}

.\build\restore.ps1

$MSBuild = Join-Path ${env:ProgramFiles(x86)} "MSBuild\14.0\bin\MSBuild.exe"

# PortabilityTools.sln understands "Any CPU" not "AnyCPU"
$PlatformToUse = $Platform

if ($Platform -eq "AnyCPU") {
    $PlatformToUse = "Any CPU"
}

& $MSBuild PortabilityTools.sln /p:Configuration=$Configuration /p:Platform="$PlatformToUse" /nologo /m /v:m /nr:false /flp:logfile=$binFolder\msbuild.log`;verbosity=$Verbosity

if ($RunTests) {
    .\build\runtests.ps1 $Configuration 
}

if ($CreateNugetPackages) {
    .\build\postbuild.ps1 $Configuration
}