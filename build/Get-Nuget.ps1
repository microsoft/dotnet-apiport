$root = $PSScriptRoot
$tools = [System.IO.Path]::Combine($root, "..", ".tools")
$nuget = Join-Path $tools "nuget.exe" 

# Bootstrap nuget
New-Item $tools -ItemType Directory -ErrorAction Ignore | Out-Null

# TODO Once nuget v3.5.0 is officially released, we'll go back to using the
# `latest` URI. v3.4.0 does not understand restoring packages for .NETCoreApp,
# while results in a lot of errors.
# $nugetUrl = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"

$nugetUrl = "https://dist.nuget.org/win-x86-commandline/v3.5.0-rc1/NuGet.exe"

if(Test-Path $nuget)
{
	Write-Verbose "Nuget already available"
}
else
{
	Invoke-Webrequest $nugetUrl -OutFile $nuget
}

$nuget
