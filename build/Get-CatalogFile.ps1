# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

param($catalogPath)

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

DownloadFile "https://portabilitystorage.blob.core.windows.net/catalog/catalog.bin?sr=c&sv=2015-02-21&si=ReadCatalog&sig=8tOHoX2ZvcSFLol0GI6lxmydNPJbnJdHNLKr06aD7t4%3D" $catalogPath
