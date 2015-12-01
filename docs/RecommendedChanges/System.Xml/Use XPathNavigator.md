### Recommended Action
Create one of the XPathNavigator implementations. There are 3 implementations of the CreateNavigator () method in  XPathDocument, System.Xml.XPath.XDocument, and System.Xml.XPath.XmlDocument, use this to find your node.

### Affected APIs
* `M:System.Xml.XmlNode.SelectSingleNode(System.String)`
* `M:System.Xml.XmlNode.SelectSingleNode(System.String,System.Xml.XmlNamespaceManager)`
