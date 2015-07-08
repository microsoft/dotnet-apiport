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

function DownloadBreakingChangeFile($destination, $fileName)
{
	$url = "https://raw.githubusercontent.com/Microsoft/dotnet-apiport/master/Documentation/BreakingChanges/" + $fileName
	$outputPath = $destination + $fileName
	DownloadFile $url $outputPath
}

DownloadFile "TBD" $catalogPath

# Unfortunately, it's not possible to download a specific directory from a Github repo (only the entire repo)
# Rather than download the entire ApiPort repo at build-time, list the individual breaking change files to download
DownloadBreakingChangeFile $breakingChangePath "! Template.md"
DownloadBreakingChangeFile $breakingChangePath "001- SoapFormatter cannot deserialize Hashtable and sim.md"
DownloadBreakingChangeFile $breakingChangePath "003- WPF DataTemplate elements are now visible to UIA.md"
DownloadBreakingChangeFile $breakingChangePath "004- WPF TextBox selected text appears a different colo.md"
DownloadBreakingChangeFile $breakingChangePath "005- ListT.ForEach.md"
DownloadBreakingChangeFile $breakingChangePath "006- System.Uri.md"
DownloadBreakingChangeFile $breakingChangePath "010- System.Uri escaping now supports RFC 3986 (http.md"
DownloadBreakingChangeFile $breakingChangePath "026- Task.WaitAll methods with time-out arguments.md"