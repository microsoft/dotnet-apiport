[CmdletBinding()] # Needed to support -Verbose
param(
	[string][ValidateSet("Release","Debug")]$configuration = "Release",
	[string]$feedUrl,
	[string]$apiKey,
	[string]$buildNumber,
	[string]$outdir = "$PSScriptRoot\..\nupkg"
)

$root = $PSScriptRoot
$src = "$root\..\src"
$nuget = & "$root\Get-Nuget.ps1"

Remove-Item $outdir -Recurse -Force -ErrorAction Ignore | Out-Null
New-Item $outdir -ItemType Directory | Out-Null

[object[]]$nuspecs = Get-ChildItem $root -Filter *.nuspec `
			| % { New-Object PSObject -Property @{"Name" = [System.IO.Path]::GetFileNameWithoutExtension($_.Name); "Path" = $_.FullName } }

$count = 0

foreach($nuspec in $nuspecs)
{
	Write-Progress -Activity "Creating portability nupkgs" -Status "Packing '$($nuspec.Name)" -PercentComplete ($count / $nuspecs.Count * 100)
	$bin = [System.IO.Path]::Combine($src, $nuspec.Name, "bin", $configuration)
	
	if(!(Test-Path $bin))
	{
		Write-Warning "Could not find path: $($nuspec.Name)"
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
		
		Copy-Item "$($item.Path)" $outdir -Force

		if($feedUrl)
		{
			$pushedOutput = & $nuget push $item.Path $apiKey -Source $feedUrl

			if($pushedOutput -match "Your package was pushed.")
			{
				$item.Pushed = $true;
			}
		}
		
		$item
	}
	else
	{
		Write-Warning "There was an error packing nuget.  Output was: '$output'"
	}
	
	Pop-Location
	$count++
}

Write-Progress -Activity "Creating portability nupkgs" -Status "Complete" -PercentComplete 100
