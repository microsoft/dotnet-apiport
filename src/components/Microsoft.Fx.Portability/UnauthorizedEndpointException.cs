using Microsoft.Fx.Portability.Resources;
using System.Globalization;

namespace Microsoft.Fx.Portability
{
    public class UnauthorizedEndpointException : PortabilityAnalyzerException
    {
        public UnauthorizedEndpointException()
            : base(string.Format(CultureInfo.CurrentCulture, LocalizedStrings.UnauthorizedAccess))
        {
        }
    }
}
