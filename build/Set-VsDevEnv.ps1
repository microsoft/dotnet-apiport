[CmdletBinding(DefaultParameterSetName = "Default")] # Needed to support -Verbose
param(
    [Parameter(ParameterSetName=’Default’, Mandatory=$true, Position=0)]
    [ValidateSet(2015,2017)]
    [int]$VisualstudioVersion,

    [Parameter(ParameterSetName=’PathGiven’, Mandatory=$true)]
    [ValidateScript({Test-Path $_ })]
    [string]$VsDevCmdPath
)

$ErrorActionPreference = "Stop"

[bool]$findVsVersion = $true

if ($PSCmdlet.ParameterSetName -eq "PathGiven") {
    $findVsVersion = $false
}

[string]$commonToolsPath = $null
[string]$VsDevCmdBat = "VsDevCmd.bat"

if ($findVsVersion) {
    switch ($VisualstudioVersion) {
        2015 
        {
            $commonToolsPath = $env:VS140COMNTOOLS
        }
        2017
        {
            $microsoftVisualStudio = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2017"

            if (Test-Path $microsoftVisualStudio) {
                $installations = Get-ChildItem $microsoftVisualStudio | ? { $_.PsIsContainer }
            
                foreach ($installation in $installations) {
                    $path = Join-Path $installation.FullName "Common7\Tools\"
                    
                    if (Test-Path $path) {
                        $commonToolsPath = $path
                        break
                    }
                }
            } else {
                Write-Error "Could not locate: $microsoftVisualStudio. Pass path to $VsDevCmdBat using parameter -VsDevCmdPath."
            }
        }
    }

    if ([string]::IsNullOrEmpty($commonToolsPath)) {
        Write-Error "Could not find Common Tools for Visual Studio $VisualstudioVersion"
    }

    $devEnv = Join-Path $commonToolsPath $VsDevCmdBat

    if (!(Test-Path $devEnv)) {
        Write-Error "Could not find VsDevCmd.bat for Visual Studio $VisualstudioVersion. Path: $devEnv"
    }

} else {
    $file = Get-Item $VsDevCmdPath
    $commonToolsPath = $file.DirectoryName
    $VsDevCmdBat = $file.Name
}

pushd $commonToolsPath

$output = cmd /c "$VsDevCmdBat & set"
 
popd
    
foreach ($line in $output)
{
    if ($line -match "(?<key>.*?)=(?<value>.*)") {
        $key = $matches["key"]
        $value = $matches["value"]
            
        Write-Verbose("$key=$value")
        Set-Item "ENV:\$key" -Value "$value" -Force
    }
}