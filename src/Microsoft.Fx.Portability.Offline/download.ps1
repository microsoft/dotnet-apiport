# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

param($catalogPath, $breakingChangePath)

function DownloadFile($url, $outputPath) {
	Write-Host "Attempt to download to $outputPath"
	
	# If the file has been downloaded don't download again. An empty file implies a failed download
	if(Test-Path $outputPath) {
		$file = Get-ChildItem $outputPath

		if($file.Length -gt 0) {
			Write-Host "$outputPath is already downloaded"
			return;
		}
	}
	
	try {
		# Create placeholder so directory exists
		New-Item -Type File $OutputPath -Force | Out-Null
		
		# Attempt to download.  If fails, placeholder remains so msbuild won't complain
		Invoke-WebRequest $url -OutFile $OutputPath | Out-Null 

		Write-Host "Downloaded $OutputPath"
	} catch {
		Write-Host "Failed to download '$url'"
	}
}

function ExtractBreakingChanges( $zipfilename, $destination )
{
	# Only bother extracting if the BreakingChanges folder doesn't exist yet
	if (Test-Path $destination\BreakingChanges) {
		return;
	}

	# Create the output directory
	[System.IO.Directory]::CreateDirectory($destination + "\\BreakingChanges")

	# Open the archive
	[System.Reflection.Assembly]::LoadWithPartialName("System.IO.Compression") | Out-Null
	[System.Reflection.Assembly]::LoadWithPartialName("System.IO.Compression.FileSystem") | Out-Null
	$archiveMode = [System.IO.Compression.ZipArchiveMode]::Read
	$archive = [System.IO.Compression.ZipFile]::Open($zipfilename, $archiveMode)

	# Find markdown files from the BreakingChanges directory and extract them
	Foreach ($entry in $archive.Entries) {
		if ($entry.Name -ne "" -And 
		[System.IO.Path]::GetDirectoryName($entry.FullName).Contains("BreakingChanges") -And
		[System.IO.Path]::GetExtension($entry.FullName).ToLowerInvariant().Equals(".md")) {
			[System.IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $destination + "\\BreakingChanges\\" + $entry.Name)
		}
	}

	$archive.Dispose()
}

DownloadFile "TBD" $catalogPath
DownloadFile "TBD" $breakingChangePath\Recommendations.zip
ExtractBreakingChanges "$breakingChangePath\Recommendations.zip" "$breakingChangePath"