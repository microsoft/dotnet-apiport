using System;

namespace Microsoft.Fx.Portability.ObjectModel
{
    [Flags]
    public enum AnalyzeRequestFlags
    {
        None = 0x0,
        NoTelemetry = 0x1
    }
}
