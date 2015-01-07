using System;

namespace Microsoft.Fx.Portability
{
    public class TargetMapperException : PortabilityAnalyzerException
    {
        public TargetMapperException(string message) : base(message) { }

        public TargetMapperException(string message, Exception innerException) : base(message, innerException) { }
    }
}
