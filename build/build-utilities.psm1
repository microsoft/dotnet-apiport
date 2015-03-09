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