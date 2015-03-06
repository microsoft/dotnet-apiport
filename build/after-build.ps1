param(
    [Parameter(Mandatory=$true, Position = 0)][string]$BinariesDirectory,
    [switch]$IsDebug
)

function Expand-ReplacementToken([string]$Contents, [string]$Token, [string]$ReplaceWith)
{
    $replaced = $Contents.Replace($Token, $ReplaceWith)
    return $replaced
}

function Expand-ZipFile([string]$Archive, [string]$Destination)
{
    if (Test-Path $Destination)
    {
        Remove-Item $Destination -Recurse -Force 
    }

    New-Item $Destination -ItemType Directory | Out-Null

    $shell = New-Object -com shell.application
    $zip = $shell.NameSpace($Archive)
    
    foreach($item in $zip.items())
    {
        $shell.Namespace($destination).copyhere($item)
    }
}

function Replace-NuspecTokens($ProjectOutputDirectory, $NuspecFile)
{
    $buildConfiguration = $ProjectOutputDirectory.Name
    $fileContents = Get-Content $NuspecFile -ErrorAction Stop
    
    $result = Expand-ReplacementToken $fileContents '$configuration$' $buildConfiguration

    $result | Set-Content $NuspecFile
}

function Get-ProjectName($ProjectOutputDirectory)
{
    $allDllsAndPdbs = Get-ChildItem $ProjectOutputDirectory.FullName -Recurse | ? { $_.Extension -eq '.dll' -or $_.Extension -eq '.pdb' }
    $uniqueNames = $allDllsAndPdbs | select BaseName -Unique

    if (@($uniqueNames).Count -ne 1)
    {
        Write-Error "There should only been 1 dll/pdb name... Actual [$uniqueNames]" -ErrorAction Stop
    }

    return $uniqueNames[0].BaseName
}

foreach ($projectDirectory in $(Get-ChildItem $BinariesDirectory | ? { $_.PSIsContainer }))
{
    $allNuGetPackages = Get-ChildItem $projectDirectory.FullName -Recurse | ? { $_.Extension -eq '.nupkg' -and !$_.Name.Contains("symbols") }
    $nupkg = $allNuGetPackages | select -First 1

    if (@($allNuGetPackages).Count -gt 1)
    {
        Write-Error "There should only be 1 nupkg for each project [$projectDirectory]. Actual [$allNuGetPackages]"
        continue
    }
    elseif (@($allNuGetPackages).Count -eq 0)
    {
        continue
    }
    
    $projectName = Get-ProjectName $projectDirectory
    $tempDirectory = Join-Path $env:TEMP $(Get-Random)
    $zipFile = Join-Path $tempDirectory "$($nupkg.BaseName).zip"

    # It needs to be a .zip file.
    New-Item $tempDirectory -ItemType Directory | Out-Null
    Copy-Item $nupkg.FullName $zipFile

    $resultingNuspecFile = Join-Path $($projectDirectory.FullName) "$projectName.nuspec"
    $originalNuspec = Join-Path $PSScriptRoot "$projectName.nuspec"
    $unzippedDirectory = Join-Path $tempDirectory "Unzipped"
    
    Copy-Item $originalNuspec $resultingNuspecFile
    Expand-ZipFile $zipFile $unzippedDirectory -ErrorAction Stop
    
    $sourceNuspecFile = Get-ChildItem $unzippedDirectory -Recurse -Include *.nuspec
    if (@($sourceNuspecFile).Count -ne 1)
    {
        Write-Error "ERROR: There should be 1 nuspec file in [$tempDirectory]! Actual [$sourceNuspecFile]"
        continue
    }

    $sourceNuspecFile = $sourceNuspecFile | select -First 1

    Replace-NuspecTokens $projectDirectory $resultingNuspecFile

    Join-XmlFile -SourceFile $sourceNuspecFile.FullName -TargetFile $resultingNuspecFile -ResultFile $resultingNuspecFile

    # Cleaning up after ourselves...
    # Remove-Item $tempFolder -Recurse -Force
}