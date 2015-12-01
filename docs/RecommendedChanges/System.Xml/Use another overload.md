### Recommended Action
Use System.Xml.XmlDictionaryReader.CreateTextReader(System.IO.Stream,System.Xml.XmlDictionaryReaderQuotas), and poll whether null to detect if it has been disposed. (Close() is deprecated.).

### Affected APIs
* `M:System.Xml.XmlDictionaryReader.CreateTextReader(System.IO.Stream,System.Text.Encoding,System.Xml.XmlDictionaryReaderQuotas,System.Xml.OnXmlDictionaryReaderClose)`
* `M:System.Xml.XmlDocument.Load(System.String)`
* `M:System.Xml.XmlConvert.ToString(System.DateTime,System.Xml.XmlDateTimeSerializationMode)`
* `M:System.Xml.XmlWriter.WriteNode(System.Xml.XPath.XPathNavigator,System.Boolean)`
