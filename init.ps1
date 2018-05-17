$ErrorActionPreference = "Stop"

function DownloadFile($url, $outputPath) {
    Write-Host "Attempt to download to $outputPath"

    # If the file has been downloaded don't download again. An empty file implies a failed download
    if (Test-Path $outputPath) {
        $file = Get-ChildItem $outputPath

        if ($file.Length -gt 0) {
            Write-Host "$outputPath is already downloaded"
            return;
        }
    }

    try {
        # Attempt to download.  If fails, placeholder remains so msbuild won't complain
        Invoke-WebRequest $url -OutFile $OutputPath | Out-Null

        Write-Host "Downloaded $OutputPath"
    }
    catch {
        Write-Error "Failed to download '$url'. $($Error[0])"
    }
}

function GetGitVersion($path) {
    $nuget = Join-Path $path "nuget.exe"

    if (!(Test-Path $nuget)) {
        Invoke-WebRequest "https://dist.nuget.org/win-x86-commandline/v4.7.0/nuget.exe" -OutFile $nuget | Out-Null
    }

    $gitversion = "$path\GitVersion.CommandLine\tools\GitVersion.exe"

    if (!(Test-Path $gitversion)) {
        & $nuget install -ExcludeVersion GitVersion.CommandLine -Source https://api.nuget.org/v3/index.json -Version 4.0.0-beta0012 -OutputDirectory $path
    }

    $gitversion = "$path\GitVersion.CommandLine\tools\GitVersion.exe"

    & $gitversion /updateAssemblyInfo "$path\GlobalAssemblyInfo.cs" /ensureassemblyinfo /output buildserver
}

$root = Join-Path $PSScriptRoot ".build"

# Create placeholder so directory exists
New-Item -Type Directory $root -ErrorAction Ignore | Out-Null

$address = "https://portability.blob.core.windows.net/catalog/catalog.bin"

DownloadFile "$address" (Join-Path $root "catalog.bin")
GetGitVersion $root
