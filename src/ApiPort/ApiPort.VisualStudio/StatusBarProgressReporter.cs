// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using ApiPortVS.Resources;
using Microsoft.Fx.Portability;
using Microsoft.VisualStudio.Shell.Interop;

namespace ApiPortVS
{
    public class StatusBarProgressReporter : TextWriterProgressReporter
    {
        private readonly IVsStatusbar _statusBar;

        public StatusBarProgressReporter(TextWriter writer, IVsStatusbar statusBar)
            : base(writer)
        {
            _statusBar = statusBar;
        }

        public override IProgressTask StartTask(string taskName, int total)
        {
            return new StatusBarProgressTask(Writer, taskName, _statusBar);
        }

        public override IProgressTask StartTask(string taskName)
        {
            return new StatusBarProgressTask(Writer, taskName, _statusBar);
        }

        private class StatusBarProgressTask : TextWriterProgressTask
        {
            private readonly IVsStatusbar _statusBar;

            public StatusBarProgressTask(TextWriter writer, string task, IVsStatusbar statusBar)
                : base(writer, task)
            {
                _statusBar = statusBar;

                _statusBar.SetText(task);
            }

            public override void Abort()
            {
                base.Abort();
                _statusBar.SetText(LocalizedStrings.AnalysisFailed);
            }
        }
    }
}
