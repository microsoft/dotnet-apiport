[CmdletBinding()] # Needed to support -Verbose
param(
	[string][ValidateSet("Release","Debug")]$configuration = "Release",
	[string]$feedUrl,
	[string]$apiKey
)

$ErrorActionPreference = "Stop"

$root = $PSScriptRoot
$drop = $env:TF_BUILD_BINARIESDIRECTORY 
$nuget = & "$root\Get-Nuget.ps1"
$netFramework = "net46"
$netStandard = "netstandard1.3"

if(!$drop)
{
	$drop = "$root\..\bin\$configuration"
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
				| Select -First 1
	$nuspecData.package.metadata.version = "$version"
	$nuspecData.Save($nuspecName)

	[string]$output = & $nuget pack
	
	if($output -Match "Attempting to build package from '(.*)'. Successfully created package '(.*)'.")
	{
		$item = New-Object PSObject -Property @{"Package" = [System.IO.Path]::GetFileName($matches[2]); "Nuspec" = $matches[1]; "Path" = $matches[2]; "Pushed" = $false}
		
		if($feedUrl)
		{
			$pushedOutput = & $nuget push $item.Path $apiKey -Source $feedUrl

			if($pushedOutput -match "Your package was pushed.")
			{
				$item.Pushed = $true;
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

function Copy-OfflineMode()
{
    $extensionsToInclude = @("*.exe", "*.dll", "*.pdb", "*.config")
	$offlineDrop = "$drop\ApiPort.Offline"

	Remove-Item $offlineDrop -Recurse -Force -ErrorAction Ignore
	New-Item -Type Directory $offlineDrop -ErrorAction Ignore | Out-Null

	Copy-Item $drop\ApiPort\$netFramework\* -Include $extensionsToInclude $offlineDrop
	Copy-Item $drop\Microsoft.Fx.Portability.Offline\$netStandard\* -Include $extensionsToInclude $offlineDrop
	Copy-Item $drop\Microsoft.Fx.Portability.Reports.Json\$netStandard\* -Include $extensionsToInclude $offlineDrop
	Copy-Item $drop\Microsoft.Fx.Portability.Reports.Html\$netFramework\* -Include $extensionsToInclude $offlineDrop
}

Write-Progress -Activity "Creating portability nupkgs" -Status "Complete" -PercentComplete 100

Copy-Item "$PSScriptRoot\..\.data\catalog.bin" $drop\Microsoft.Fx.Portability.Offline -Recurse -Force

Copy-OfflineMode

# Copying the license terms into our drop so we don't have to manually do it when we want to release
Copy-Item "$PSScriptRoot\..\docs\LicenseTerms" $drop\ApiPort\$netFramework -Recurse -Force
Copy-Item "$PSScriptRoot\..\docs\LicenseTerms" $drop\ApiPort.Offline\ -Recurse -Force