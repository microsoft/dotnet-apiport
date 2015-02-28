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

# For each project we want to
# 1) Unpack the .nupkg
# 2) Merge the corresponding .nuspec with the .nuspec in the .nupkg
# 3) Repack the .nupkg with the signed dlls
foreach ($projectDirectory in $(Get-ChildItem $BinariesDirectory | ? { $_.PSIsContainer }))
{
    $allNupkgs = Get-ChildItem $projectDirectory.FullName -Recurse | ? { $_.Extension -eq '.nupkg' }
    $excludingSymbolNupkgs = $allNupkgs | ? { !$_.Name.Contains("symbols") } 
    $nupkg = $excludingSymbolNupkgs| select -First 1

    if (@($excludingSymbolNupkgs).Count -gt 1)
    {
        Write-Error "There should only be 1 nupkg for each project [$projectDirectory]. Actual [$excludingSymbolNupkgs]"
        foreach ($pkg in $excludingSymbolNupkgs)
        {
            Write-Output "Package: $($pkg.FullName)"
        }

        continue
    }
    elseif (@($excludingSymbolNupkgs).Count -eq 0)
    {
        continue
    }
    
    $projectName = Get-ProjectName $projectDirectory
    $tempDirectory = Join-Path $env:TEMP $(Get-Random)
    $zipFile = Join-Path $tempDirectory "$($nupkg.BaseName).zip"

    # Expand-ZipFile only works with .zip
    New-Item $tempDirectory -ItemType Directory | Out-Null
    Copy-Item $nupkg.FullName $zipFile

    $resultingNuspecFile = Join-Path $projectDirectory.FullName "$projectName.nuspec"
    $originalNuspec = Join-Path $PSScriptRoot "$projectName.nuspec"
    $unzippedDirectory = Join-Path $tempDirectory "Unzipped"
    
    Copy-Item $originalNuspec $resultingNuspecFile
    Expand-ZipFile $zipFile $unzippedDirectory -ErrorAction Stop
    
    # Getting both .nuspec files, the template and the one from kpm build to merge.
    $sourceNuspecFile = Get-ChildItem $unzippedDirectory -Recurse -Include *.nuspec
    if (@($sourceNuspecFile).Count -ne 1)
    {
        Write-Error "ERROR: There should be 1 nuspec file in [$tempDirectory]! Actual [$sourceNuspecFile]"
        continue
    }

    $sourceNuspecFile = $sourceNuspecFile | select -First 1

    Replace-NuspecTokens $projectDirectory $resultingNuspecFile

    Join-XmlFile -SourceFile $sourceNuspecFile.FullName -TargetFile $resultingNuspecFile -ResultFile $resultingNuspecFile

    # Move all of the original nupkgs into another folder.
    $originalNupkgsDirectory = Join-Path $projectDirectory.FullName "originalnupkgs"
    
    New-Item $originalNupkgsDirectory -ItemType Directory | Out-Null
    $allNupkgs | Move-Item -Destination $originalNupkgsDirectory

    # Finally, repack all of the contents of the nupkg.
    Invoke-Expression "$PSScriptRoot\nuget.exe pack $resultingNuspecFile -OutputDirectory $($projectDirectory.FullName) -Symbols" -ErrorAction Stop

    # Cleaning up after ourselves...
    Remove-Item $resultingNuspecFile -Force
    Remove-Item $tempDirectory -Recurse -Force
}
