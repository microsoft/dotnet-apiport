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

$address = "https://dotnetportability.blob.core.windows.net/catalog/catalog.bin"
$token = "?sv=2015-04-05&sr=b&si=ReadCatalog&sig=4Qfue7TXbeyS9w3kDp9%2BA6TskFhVb5uW97IE7AVI5SA%3D&st=2017-11-14T21%3A34%3A39Z&se=2018-11-14T21%3A34%3A39Z"

DownloadFile "$address$token" "$PSScriptRoot\.data\catalog1.bin"
