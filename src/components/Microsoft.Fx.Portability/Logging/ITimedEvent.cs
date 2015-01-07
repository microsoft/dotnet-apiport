using System;

namespace Microsoft.Fx.Portability.Logging
{
    public interface ITimedEvent : IDisposable
    {
        void Cancel();
    }
}
