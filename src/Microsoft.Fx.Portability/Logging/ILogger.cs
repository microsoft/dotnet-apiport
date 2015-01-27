// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.Fx.Portability.Logging
{
    public interface ILogger : IDisposable
    {
        ITimedEvent CreateTimedEvent(string eventName);
        void LogEvent(params object[] eventObj);
    }
}
