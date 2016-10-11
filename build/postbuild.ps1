[CmdletBinding()] # Needed to support -Verbose
param(
    [string][ValidateSet("Release","Debug")]$Configuration = "Release",
    [string]$FeedUrl,
    [string]$ApiKey,
    [switch]$PublishVsix
)

$ErrorActionPreference = "Stop"

$root = $PSScriptRoot
$buildToolScript = Join-Path $root "Get-BuildTools.ps1"
$tools = [System.IO.Path]::Combine($root, "..", ".tools")
$nuget = & $buildToolScript "nuget"

$netFramework = "net46"
$netStandard = "netstandard1.3"

$drop = $env:TF_BUILD_BINARIESDIRECTORY 

if(!$drop)
{
    $drop = $(Resolve-Path "$root\..\bin\$Configuration").Path
}

[object[]]$nuspecs = Get-ChildItem $root -Filter *.nuspec `
			| % { New-Object PSObject -Property @{"Name" = [System.IO.Path]::GetFileNameWithoutExtension($_.Name); "Path" = $_.FullName } }

$count = 0

foreach($nuspec in $nuspecs)
{
	Write-Progress -Activity "Creating portability nupkgs" -Status "Packing '$($nuspec.Name)" -PercentComplete ($count / $nuspecs.Count * 100)
	$bin = Join-Path $drop $nuspec.Name
	
	if(!(Test-Path $bin))
	{
		Write-Warning "Could not find path: $($nuspec)"
		continue
	}
	
	Copy-Item $nuspec.Path $bin
	Push-Location $bin

	# Update version based on compiled version
	$nuspecName = "$bin\$($nuspec.Name).nuspec"
	[xml]$nuspecData = Get-Content $nuspecName
	$version = $nuspecData.package.files.file `
				| where {$_.src.EndsWith("$($nuspec.Name).dll")} `
				| % { Get-ChildItem "$bin\$($_.src)" } `
				| % { $_.VersionInfo.ProductVersion } `
				| select -First 1
    
    Write-Verbose "Package: [$($nuspec.Name)] Version: [$version]"

	$nuspecData.package.metadata.version = "$version"
	$nuspecData.Save($nuspecName)

	[string]$output = & $nuget pack
	
	if($output -Match "Attempting to build package from '(.*)'. Successfully created package '(.*)'.")
	{
		$item = New-Object PSObject -Property @{"Package" = [System.IO.Path]::GetFileName($matches[2]); "Nuspec" = $matches[1]; "Path" = $matches[2]; "Pushed" = $false}
		
		if($FeedUrl)
		{
			$pushedOutput = & $nuget push $item.Path $ApiKey -Source $FeedUrl

			if($pushedOutput -match "Your package was pushed.")
			{
				$item.Pushed = $true
			}
		}

        Write-Host "Package: [$($item.Package)], Pushed: [$($item.Pushed)]"
	}
	else
	{
		Write-Warning "There was an error packing nuget.  Output was: '$output'"
	}
	
	Pop-Location
	$count++
}

Write-Progress -Activity "Creating portability nupkgs" -Status "Complete" -PercentComplete 100

function Copy-OfflineMode()
{
    $extensionsToInclude = @("*.exe", "*.dll", "*.pdb", "*.config")
	$offlineDrop = "$drop\ApiPort.Offline"

	Remove-Item $offlineDrop -Recurse -Force -ErrorAction Ignore
	New-Item -Type Directory $offlineDrop -ErrorAction Ignore | Out-Null

    Copy-Item "$root\..\.data\catalog.bin" $drop\Microsoft.Fx.Portability.Offline -Recurse -Force

	Copy-Item $drop\Microsoft.Fx.Portability.Offline\$netStandard\* -Include $extensionsToInclude $offlineDrop
	Copy-Item $drop\Microsoft.Fx.Portability.Reports.Json\$netStandard\* -Include $extensionsToInclude $offlineDrop
	Copy-Item $drop\Microsoft.Fx.Portability.Reports.Html\$netFramework\* -Include $extensionsToInclude $offlineDrop
	Copy-Item $drop\ApiPort\$netFramework\* -Include $extensionsToInclude $offlineDrop
}

Copy-OfflineMode

# Copying the license terms into our drop so we don't have to manually do it when we want to release
Copy-Item "$root\..\docs\LicenseTerms" $drop\ApiPort\$netFramework -Recurse -Force
Copy-Item "$root\..\docs\LicenseTerms" $drop\ApiPort.Offline\ -Recurse -Force

if ($PublishVsix) {
    # Setting these environment variables because they are used in the script
    # for the upload endpoint:
    # https://github.com/madskristensen/ExtensionScripts/blob/master/AppVeyor/vsix.ps1#L52-L55
    # https://github.com/madskristensen/ExtensionScripts/blob/master/AppVeyor/vsix.ps1#L68
    $env:APPVEYOR_REPO_PROVIDER = "github"
    $env:APPVEYOR_REPO_NAME = "microsoft/dotnet-apiport"

    . $(& $buildToolScript "vsix")

    Vsix-PublishToGallery -path $drop\**\*.vsix
}