$ErrorActionPreference = "Stop"

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
		(New-Object System.Net.WebClient).DownloadFile($url, $OutputPath) | Out-Null

		Write-Host "Downloaded $OutputPath"
	} catch {
		Write-Warning "Failed to download '$url', it will not be included in the available additional data. $($Error[0])"
	}
}

# Setup the URLs for downloading the different additionaldata binaries
$exceptionAddress = "https://portability.blob.core.windows.net/additionaldata/exceptions-stable.bin"

# Download each additionaldata binary individually
DownloadFile "$exceptionAddress" "$PSScriptRoot\..\.data\exceptions.bin"