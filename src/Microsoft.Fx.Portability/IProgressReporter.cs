// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.Fx.Portability
{
    public interface IProgressReporter : IDisposable
    {
        void ReportIssue(string issue);
        IProgressTask StartTask(string taskName, int totalUnits);
        IProgressTask StartTask(string taskName);
        IReadOnlyCollection<string> Issues { get; }

        void Suspend();
        void Resume();
    }
}
