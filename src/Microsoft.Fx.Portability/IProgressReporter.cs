// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.Fx.Portability
{
    public interface IProgressReporter
    {
        void AbortTask();
        void FinishTask();
        void ReportIssue(string issueFormat, params object[] items);
        void ReportUnitComplete();
        void StartParallelTask(string taskName, string details);
        void StartTask(string taskName);
        IReadOnlyCollection<string> Issues { get; }
    }
}