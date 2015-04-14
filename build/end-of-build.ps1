param(
    [Parameter(Mandatory=$true, Position = 0)][string]$BinariesDirectory,
    [Parameter(Mandatory=$true, Position = 1)][string]$NuGetFeed,
    [Parameter(Mandatory=$true, Position = 2)][string]$APIAccessKey
)

Import-Module $(Join-Path $PSScriptRoot "build-utilities.psm1") -Verbose -ErrorAction Stop

function Test-NupkgIsSigned($Nupkg)
{
    $tempDirectory = Join-Path $env:TEMP $(Get-Random)
    $unzippedDirectory = Join-Path $tempDirectory "Unzipped"
    $zipFile = Join-Path $tempDirectory "$($Nupkg.BaseName).zip"

    # Expand-ZipFile only works with .zip
    New-Item $tempDirectory -ItemType Directory | Out-Null
    Copy-Item $Nupkg.FullName $zipFile | Out-Null
    Expand-ZipFile $zipFile $unzippedDirectory -ErrorAction Stop | Out-Null

    $allBinaries = Get-ChildItem $unzippedDirectory -Recurse | ? { $_.Extension -eq '.dll' -or $_.Extension -eq '.exe' -or $_.Extension -eq '.winmd' }
    
    [bool]$isValid = $true
    
    foreach ($binary in $allBinaries)
    {
        Invoke-Expression "SignTool.exe verify /q /pa $($binary.FullName)" | Out-Null

        if ($LASTEXITCODE -ne 0)
        {
            $isValid = $false
            break;
        }
    }

    # Cleaning up after ourselves...
    Remove-Item $tempDirectory -Recurse -Force | Out-Null

    return $isValid
}

$nugetExe = Invoke-DownloadNuget

$releaseDirectory = Join-Path $BinariesDirectory "Release"
$nupkgsToUpload = Get-ChildItem $releaseDirectory | ? { $_.Extension -eq '.nupkg' }

if (@($nupkgsToUpload).Count -eq 0)
{
    Write-Host "There were no nupkgs in [$releaseDirectory] to upload..."
    return
}

& $env:windir\System32\where.exe /Q "SignTool.exe"
if ($LASTEXITCODE -ne 0)
{
    Write-Host "Setting VS environment..."
    Set-VSEnvironment
}

foreach ($package in $nupkgsToUpload)
{
    $packageIsValid = $(Test-NupkgIsSigned $package)
    Write-Host "Is Package Valid? [$packageIsValid]"
    if (!$packageIsValid)
    {
        Write-Host "[$package] is not signed... skipping upload."
        continue
    }
    else
    {
        Write-Host "Uploading package [$($package.Name)]..."
        Invoke-Expression "$nugetExe push $($package.FullName) $APIAccessKey -Source $NuGetFeed"
    }
}