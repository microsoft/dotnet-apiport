$root = $PSScriptRoot
$nuget = & "$root\Get-Nuget.ps1"

& $nuget restore $root\..\PortabilityTools.sln