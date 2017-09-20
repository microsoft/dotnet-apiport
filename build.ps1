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

    [string]$VersionSuffix = "alpha",

    [ValidateSet(2017)]
    [int]$VisualStudioVersion = 2017
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot

function Invoke-Tests() {
    Write-Host "Running tests"

    $testFolder = $(Resolve-Path $([IO.Path]::Combine($root, "tests"))).Path
    $testResults = [IO.Path]::Combine($root, "TestResults")

    if (!(Test-Path $testFolder)) {
        Write-Error "Could not find test folder [$testFolder]"
        return -1
    }

    if (!(Test-Path $testResults)) {
        Write-Host "Creating $testResults folder..."
        New-Item $testResults -ItemType Directory
    }

    $dotnet = "dotnet.exe"
    $dotnetCommand = $(Get-Command $dotnet -CommandType Application -ErrorAction Ignore)

    # Possible that the VS Developer Command prompt is not yet set.
    if ($dotnetCommand -eq $null) {
        .\build\Set-VsDevEnv.ps1 -VisualstudioVersion $VisualStudioVersion
        $dotnetCommand = $(Get-Command $dotnet -CommandType Application -ErrorAction Ignore)

        if ($dotnetCommand -eq $null) {
            Write-Error "Could not set visual studio $VisualStudioVersion environment and locate $dotnet!"
        }
    }

    foreach ($test in $(Get-ChildItem $testFolder | ? { $_.PsIsContainer })) {
        $csprojs = Get-ChildItem $test.FullName -Recurse | ? { $_.Extension -eq ".csproj" }
        foreach ($proj in $csprojs) {
            $trx = "$($proj.BaseName).$(Get-Date -Format "yyyy-MM-dd.hh_mm_ss").trx"
            $fullpath = Join-Path $testResults $trx

            Write-Host "Testing $($proj.Name). Output: $trx"

            & $dotnetCommand test "$($proj.FullName)" --configuration $Configuration --logger "trx;LogFileName=$fullpath" --no-build
        }
    }
}

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

$MSBuildCommand = $MSBuildCommand | Where-Object { $_.Version.Major -eq $MSBuildVersion } | Select -First 1

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

& "$root\init.ps1"

# PortabilityTools.sln understands "Any CPU" not "AnyCPU"
$PlatformToUse = $Platform

if ($Platform -eq "AnyCPU") {
    $PlatformToUse = "Any CPU"
}

Push-Location $root

& $MSBuild PortabilityTools.sln "/t:restore;build;pack" /p:Configuration=$Configuration /p:Platform="$PlatformToUse" /nologo /m /v:m /nr:false /flp:logfile=$binFolder\msbuild.log`;verbosity=$Verbosity

Pop-Location

if ($RunTests) {
    Invoke-Tests
}
