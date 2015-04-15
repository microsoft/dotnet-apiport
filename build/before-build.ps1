param(
    [Parameter(Mandatory=$true)][string]$SourceDirectory,
    [switch]$IsDebug
)

Import-Module $(Join-Path $PSScriptRoot "build-utilities.psm1") -Verbose -ErrorAction Stop

function Get-KREPath
{
    $kreFolder = Join-Path $env:USERPROFILE ".k"
    $kreRuntimes = Join-Path $kreFolder "runtimes"

    $defaultAliasFile = [System.IO.Path]::Combine($kreFolder, "alias", "default.txt")
    
    $runtimeToUse = ""

    # If the KRE was installed previously, it will have this alias folder with the default alias to use.
    # When we upgrade the KRE, default is set to the latest version
    if (Test-Path $defaultAliasFile)
    {
        $defaultAlias = Get-Content $defaultAliasFile
        $runtimeToUse = Join-Path $kreRuntimes $defaultAlias
    }
    else
    {
        $allExistingRuntimes = Get-ChildItem $kreRuntimes -Exclude temp

        if (@($allExistingRuntimes).Count -ne 1)
        {
            Write-Error "There was an error installing KRE. We were supposed to have 1 runtime. [$allExistingRuntimes]"
            return $null
        }

        $runtimeFolder = $allExistingRuntimes | select -First 1
        $runtimeToUse = $runtimeFolder.FullName
    }

    return $(Join-Path $runtimeToUse "bin")
}

Write-Output "Installing KRE and KPM..."

$kpmPath = Join-Path "$env:USERPROFILE" ".kpm"
$krePath = Join-Path "$env:USERPROFILE" ".k"

# Check if we need to install KRE and KPM
$isKPMExist = Test-Path $kpmPath -PathType Container
$isKREExist = Test-Path $krePath -PathType Container

if ($IsDebug)
{
    Write-Output "Source: [$SourceDirectory]"
    Write-Output "KPM: [$kpmPath]"
    Write-Output "KPM Exists? [$isKPMExist]"
    
    Write-Output "KRE: [$krePath]"
    Write-Output "KRE Exists? [$isKREExist]"
}

if (!$isKPMExist -or !$isKREExist)
{
    & powershell -NoProfile -ExecutionPolicy Unrestricted -Command "Invoke-Expression ((New-Object Net.WebClient).DownloadString('https://raw.githubusercontent.com/aspnet/Home/master/kvminstall.ps1'))"

    $KREInstallPath = "$($env:USERPROFILE)\.k\bin"
    $kreInstallOutput = & powershell -NoProfile -ExecutionPolicy Unrestricted -Command "& $KREInstallPath\kvm.cmd upgrade"
    Write-Output "Install Output: $kreInstallOutput"
}
else
{
    & powershell -NoProfile -ExecutionPolicy Unrestricted -Command "& kvm.cmd install latest"
}

$kreBin = Get-KREPath

if ($IsDebug)
{
    Write-Output "KRE Bin: [$kreBin]"
}

Write-Output "Restoring project.json..."

# Get all of the projects with a project.json so we can restore packages.
foreach ($file in $(Get-ChildItem .\project.json -Recurse))
{
    $directory = $file.DirectoryName

    pushd $directory
    Invoke-Expression "& $kreBin\kpm.cmd restore"
    popd
}

## Restore packages
$nuget = Invoke-DownloadNuget

& $nuget restore "$PSScriptRoot\..\PortabilityTools.sln"