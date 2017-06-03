[CmdletBinding()] # Needed to support -Verbose
param(
    [string][ValidateSet("Release","Debug")]$Configuration = "Release",
    [string]$FeedUrl,
    [string]$ApiKey,

    [switch]$CreateNugetPackages,
    [switch]$PublishVsix,
    [switch]$PublishNuGet
)

$ErrorActionPreference = "Stop"

$root = $PSScriptRoot
$buildToolScript = Join-Path $root "Get-BuildTools.ps1"
$tools = [System.IO.Path]::Combine($root, "..", ".tools")
$nuget = & $buildToolScript "nuget"
$src = [System.IO.Path]::Combine($root, "..", "src")

$VisualStudioVersion = 2017
& $root\Set-VsDevEnv.ps1 -VisualstudioVersion $VisualStudioVersion

$MSBuildVersion = 15
$MSBuildCommand = $(Get-Command "MSBuild.exe" -CommandType Application -ErrorAction Ignore) | ? { $_.Version.Major -eq $MSBuildVersion } | Select -First 1

if (!$MSBuildCommand) {
    Write-Error "Could not find MSBuild.  Please set your VS environment to find MSBuild $MSBuildVersion"
}

$drop = $env:TF_BUILD_BINARIESDIRECTORY

if(!$drop)
{
    $drop = $(Resolve-Path "$root\..\bin\$Configuration").Path
}

function Generate-NuGetPackages
{
    $count = 0

    $projects = @(Get-ChildItem $src -Exclude "ApiPort*")

    foreach ($project in $projects)
    {
        $name = $project.Name
        $csproj = Join-Path $project.FullName "$($project.BaseName).csproj"

        if (!$(Test-Path $csproj)) {
            Write-Error "$csproj does not exist!"
        }

        Write-Progress -Activity "Creating portability nupkgs" -Status "Packing '$name" -PercentComplete ($count / $projects.Count * 100)

        $bin = Join-Path $drop $name

        if(!(Test-Path $bin))
        {
            Write-Warning "Could not find path: $bin"
            continue
        }

        $dll = "$name.dll"

        Write-Host "Looking for $dll in $bin..."

        # Update version based on compiled version
        $versions = Get-ChildItem $bin -Recurse `
                    | ? { $_.Name -eq $dll }

        if (@($versions).Count -eq 0) {
            Write-Error "Could not find any binaries in $bin matching $dll"
        } elseif (@($versions).Count -gt 1) {
            Write-Host "Found multiple versions..."

            foreach ($v in $versions) {
                Write-Host "$($v.FullName): $($v.VersionInfo.ProductVersion)"
            }

            Write-Host "Using first one"
        }

        $version = $versions | % { $_.VersionInfo.ProductVersion } | select -First 1

        Write-Host "Package: [$($project.Name)] Version: [$version]"

        & $MSBuildCommand /t:Pack /p:NoBuild=true /p:Version=$version /p:Configuration=$Configuration /p:PackageOutputPath=$bin "$csproj"

        $childItem = Get-ChildItem $bin | ? { $_.BaseName.StartsWith($name) -and $_.Extension.Equals(".nupkg") } | Select -First 1

        if ($childItem -ne $null)
        {
            Write-Host "Package created!"
        }
        else
        {
            Write-Error "Failed to create NuGet Package for $csproj."
        }

        if($PublishNuGet -and ![string]::IsNullOrWhitespace($FeedUrl))
        {
            $pushedOutput = & $nuget push $($childItem.FullName) -ApiKey $ApiKey -Source $FeedUrl

            if($pushedOutput -match "Your package was pushed.")
            {
                Write-Host "Package pushed: $($childItem.FullName)"
            }
            else
            {
                Write-Host $pushedOutput
                Write-Error "Package not pushed: $($childItem.FullName)"
            }
        }

        $count++
    }

    Write-Progress -Activity "Creating portability nupkgs" -Status "Complete" -PercentComplete 100
}

function Copy-OfflineMode()
{
    $netFramework = "net46"
    $netStandard = "netstandard1.3"

    $extensionsToInclude = @("*.exe", "*.dll", "*.pdb", "*.config")
    $offlineDrop = "$drop\ApiPort.Offline"

    Write-host "Creating offline drop [$offlineDrop]..."

    Remove-Item $offlineDrop -Recurse -Force -ErrorAction Ignore
    New-Item -Type Directory $offlineDrop -ErrorAction Ignore | Out-Null

    $catalogPath = Resolve-Path "$root\..\.data\catalog.bin"

    Write-Host "Copying: $catalogPath"

    Copy-Item $catalogPath $offlineDrop\ -Force

    Copy-Item $drop\Microsoft.Fx.Portability.Offline\$netStandard\* -Include $extensionsToInclude $offlineDrop
    Copy-Item $drop\Microsoft.Fx.Portability.Reports.Json\$netStandard\* -Include $extensionsToInclude $offlineDrop
    Copy-Item $drop\Microsoft.Fx.Portability.Reports.Html\$netFramework\* -Include $extensionsToInclude $offlineDrop
    Copy-Item $drop\ApiPort\$netFramework\win7-x64\* -Include $extensionsToInclude $offlineDrop
}

if ($CreateNugetPackages)
{
    Generate-NuGetPackages
}

Copy-OfflineMode

Write-Host "Copying license terms into ApiPort folders..."

# Copying the license terms into our drop so we don't have to manually do it when we want to release
foreach ($platform in $(Get-ChildItem $drop\ApiPort | ? { $_.PSIsContainer })) {

    Copy-Item "$root\..\docs\LicenseTerms" "$($platform.FullName)\" -Recurse -Force
}

Copy-Item "$root\..\docs\LicenseTerms" $drop\ApiPort.Offline\ -Recurse -Force

if ($PublishVsix) {
    # Setting these environment variables because they are used in the script
    # for the upload endpoint:
    # https://github.com/madskristensen/ExtensionScripts/blob/master/AppVeyor/vsix.ps1#L52-L55
    # https://github.com/madskristensen/ExtensionScripts/blob/master/AppVeyor/vsix.ps1#L68
    $env:APPVEYOR_REPO_PROVIDER = "github"
    $env:APPVEYOR_REPO_NAME = "microsoft/dotnet-apiport"

    $vsix = "$drop\ApiPort.Vsix\ApiPort.vsix"

    if (!(Test-Path $vsix)) {
        Write-Error "Could not find $vsix to upload"
    }

    . $(& $buildToolScript "vsix")

    Vsix-PublishToGallery -path $vsix
}