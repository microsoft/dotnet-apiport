function Join-XmlElements([System.Xml.XmlElement]$Template, [System.Xml.XmlElement]$Source, [System.Xml.XmlDocument]$TemplateDocument, [System.Xml.XmlDocument]$SourceDocument)
{
    $namespaceManager = New-Object System.Xml.XmlNamespaceManager $TemplateDocument.NameTable
    $namespaceManager.AddNamespace("nuspec", $Template.NamespaceURI)

    if (!$Template.HasChildNodes -and !$Source.HasChildNodes)
    {
        return;
    }
    elseif ($Template.FirstChild.LocalName -eq '#text')
    {
        $Source.FirstChild.Value = $Template.FirstChild.Value
        return;
    }

    foreach ($child in $Template.ChildNodes)
    {
        $xname = "nuspec:$($child.Name)"
        $matchingNode = $Source.SelectSingleNode($xname, $namespaceManager)
    
        if ($matchingNode -eq $null)
        {
            $copiedNode = $SourceDocument.ImportNode($child, $true)
            $Source.AppendChild($copiedNode)
        }
        else 
        {
            Join-XmlElements $([System.Xml.XmlElement]$child) $([System.Xml.XmlElement]$matchingNode) $TemplateDocument $SourceDocument
        }
    }
}

function Join-XmlFiles([string]$Template, [string]$Source, [string]$Result)
{
    $templateDocument = [xml](Get-Content $Template)
    $sourceDocument = [xml](Get-Content $Source)
    
    $templateXmlElement = $templateDocument.DocumentElement
    $sourceXmlElement = $sourceDocument.DocumentElement

    Join-XmlElements $templateXmlElement $sourceXmlElement $templateDocument $sourceDocument
    
    $sourceDocument.Save($Result)
}

function Add-VersionToNuspec([string]$Source, [string]$Version)
{
    $xml = [xml](Get-Content $Source)

    $namespaceManager = New-Object System.Xml.XmlNamespaceManager $xml.NameTable
    $namespaceManager.AddNamespace("nuspec", $xml.DocumentElement.NamespaceURI)

    $node = $xml.DocumentElement.SelectSingleNode("descendant::nuspec:version", $namespaceManager)
    if ($node -eq $null)
    {
        return;
    }
    
    $formatted = "{0}-{1}" -f $node.FirstChild.Value, $Version
    $node.FirstChild.Value = $formatted

    $xml.Save($Source)
}

function Get-ProjectName($NuGetPackage)
{
    if ($NuGetPackage.BaseName -match '(?<projectname>[a-zA-Z\.]+)\.(?<version>[0-9\.]+(\-\w+)?)')
    {
        return $Matches['projectname']
    }
    else
    {
        Write-Error "[$($NuGetPackage.BaseName)] should have matched the Regex!"
        return $null
    }
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

function Set-VSEnvironment
{
    if (!(Test-Path Env:\VS140COMNTOOLS))
    {
        return
    }

    #Set environment variables for Visual Studio Command Prompt
    $vcPath = Resolve-Path "$($env:VS140COMNTOOLS)\..\..\VC"

    pushd $vcPath

    cmd /c "vcvarsall.bat&set" |

    foreach {
      if ($_ -match "=") {
        $v = $_.split("="); set-item -force -path "ENV:\$($v[0])"  -value "$($v[1])"
      }
    }

    popd
}

# Download NuGet and stamp with date downloaded so subsequent calls won't download
function Invoke-DownloadNuget()
{
	$date = (Get-Date).ToString("MMddyyyy")
    $nugetExe = Join-Path $env:TEMP nuget-$date.exe
	
	if(-NOT (Test-Path $nugetExe)){
		Invoke-WebRequest "http://www.nuget.org/nuget.exe" -OutFile $nugetExe -ErrorAction Stop
	}
    return $nugetExe
}