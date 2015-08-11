param
(
	[ValidateSet("Release","Debug")]
	[string]$configuration = "Release",
	[string]$feedUrl,
	[string]$apiKey,
	[string]$outputPath = $env:TF_BUILD_BINARIESDIRECTORY,
	[string]$buildNumber = $env:TF_BUILD_BUILDNUMBER
)

New-Item $outputPath -ItemType Directory -ErrorAction Ignore | Out-Null

$root = Join-Path $PSScriptRoot ".."

function CopyProjects([string]$dir)
{
	foreach($item in Get-ChildItem $root\$dir -Directory)
	{
		Copy-Item "$($item.FullName)\bin\$configuration" $outputPath\$dir\$item -Recurse
	}	
}

Push-Location $PSScriptRoot

.\package.ps1 -Configuration $configuration -feedUrl $feedUrl -apiKey $apiKey -buildNumber $buildNumber

CopyProjects "src"
CopyProjects "tests"

Pop-Location