using System;

namespace Microsoft.Fx.Portability.Logging
{
    public interface ILogger : IDisposable
    {
        ITimedEvent CreateTimedEvent(string eventName);
        void LogEvent(params object[] eventObj);
    }
}
