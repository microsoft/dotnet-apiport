using Microsoft.Fx.Portability.Resources;

namespace Microsoft.Fx.Portability
{
    public class NotFoundException : PortabilityAnalyzerException
    {
        public NotFoundException()
            : base(LocalizedStrings.NotFoundException)
        { }
    }
}
