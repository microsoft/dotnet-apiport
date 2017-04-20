[CmdletBinding()]
Param(
    [ValidateSet("Debug", "Release")]
	[Parameter(Position=0, Mandatory=$True)]
    [string]$Configuration,

    [ValidateSet(2015, 2017)]
    [int]$VisualStudioVersion = 2017
)

$ErrorActionPreference = "Stop"

$root = $PSScriptRoot
$testFolder = $(Resolve-Path $([IO.Path]::Combine($root, "..", "tests"))).Path

if (!(Test-Path $testFolder)) {
    Write-Error "Could not find test folder [$testFolder]"
    return -1
}

$VsTestConsoleCommand = $(Get-Command "vstest.console.exe" -CommandType Application -ErrorAction Ignore)

# Possible that the VS Developer Command prompt is not yet set.
if ($VsTestConsoleCommand -eq $null) {
    .\build\Set-VsDevEnv.ps1 -VisualstudioVersion $VisualStudioVersion
    $VsTestConsoleCommand = $(Get-Command "vstest.console.exe" -CommandType Application -ErrorAction Ignore)

    if ($VsTestConsoleCommand -eq $null) {
        Write-Error "Could not set visual studio $VisualStudioVersion environment and locate vstest.console.exe!"
    }
}

$vstest = $VsTestConsoleCommand.Path
$binaryFolders = Get-ChildItem $testFolder -Recurse | ? { $_.PsIsContainer -and $_.FullName.EndsWith($(Join-Path "bin" $Configuration)) }

$testDlls = New-Object System.Collections.ArrayList
$testAdapters = New-Object System.Collections.ArrayList

foreach ($folder in $binaryFolders) {
    
    $dlls = Get-ChildItem $folder.FullName | ? { $_.Extension -eq ".dll" }

    foreach ($dll in $dlls) {
        if ($dll.Name -match "(T|t)ests?\.dll") {
            $testDlls.Add($dll.FullName) | Out-Null
        } elseif ($dll.Name -match "testadapter\.dll") {
            $testAdapters.Add($dll.DirectoryName) | Out-Null
        }
    }
}

$testAdapterFolder = """$($testFolder | select -First 1)"""

foreach ($test in $testDlls) {
    & $vstest $test /logger:trx /TestAdapterPath:$testAdapterFolder
}