using Microsoft.Fx.Portability.Resources;

namespace Microsoft.Fx.Portability
{
    public class MovedPermanentlyException : PortabilityAnalyzerException
    {
        public MovedPermanentlyException() : base(LocalizedStrings.ServerEndpointMovedPermanently) { }
    }
}
