[CmdletBinding()]
Param(
    [ValidateSet("Debug", "Release")]
	[Parameter(Position=0, Mandatory=$True)]
    [string]$Configuration,

    [ValidateSet(2015, 2017)]
    [int]$VisualStudioVersion = 2017
)
Write-Host "Running tests"

$ErrorActionPreference = "Stop"

$root = $PSScriptRoot
$testFolder = $(Resolve-Path $([IO.Path]::Combine($root, "..", "tests"))).Path
$testResults = [IO.Path]::Combine($root, "..", "TestResults")

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