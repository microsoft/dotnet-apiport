using System;

namespace Microsoft.Fx.Portability
{
    public class PortabilityAnalyzerException : Exception
    {
        public PortabilityAnalyzerException(string message) : base(message) { }
        public PortabilityAnalyzerException(string message, Exception innerException) : base(message, innerException) { }
    }
}
