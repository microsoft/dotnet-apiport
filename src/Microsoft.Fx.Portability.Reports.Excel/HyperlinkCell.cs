using System;

namespace Microsoft.Fx.OpenXmlExtensions
{
    internal class HyperlinkCell
    {
        public string DisplayString { get; set; }
        public Uri Url { get; set; }
        public uint? StyleIndex { get; set; }
    }
}
