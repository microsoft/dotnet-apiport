$root = $PSScriptRoot
$tools = [System.IO.Path]::Combine($root, "..", ".tools")
$nuget = Join-Path $tools "nuget.exe" 

# Bootstrap nuget
New-Item $tools -ItemType Directory -ErrorAction Ignore | Out-Null
$nugetUrl = "http://dist.nuget.org/win-x86-commandline/v3.1.0-beta/nuget.exe"

if(Test-Path $nuget)
{
	Write-Verbose "Nuget already available"
}
else
{
	Invoke-Webrequest $nugetUrl -OutFile $nuget
}

$nuget