[CmdletBinding()]
Param(
    [ValidateSet("Debug", "Release")]
	[Parameter(Position=0, Mandatory=$True)]
    [string]$Configuration
)

$ErrorActionPreference = "Stop"

$root = $PSScriptRoot
$testFolder = $(Resolve-Path $([IO.Path]::Combine($root, "..", "tests"))).Path

if (!(Test-Path $testFolder)) {
    Write-Error "Could not find test folder [$testFolder]"
    return -1
}

if ($env:VS140COMNTOOLS) {
    $vstest = Resolve-Path ([IO.Path]::Combine($env:VS140COMNTOOLS, "..", "IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe"))
} elseif ($env:VS120COMNTOOLS) {
    $vstest = Resolve-Path ([IO.Path]::Combine($env:VS120COMNTOOLS, "..", "IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe"))
} else {
    Write-Error "Could not set vstest.console.exe because %VS140COMNTOOLS% or %VS120COMNTOOLS% are not set."
}

if (!(Test-Path $vstest)) {
    Write-Error "$vstest does not exist"
}

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