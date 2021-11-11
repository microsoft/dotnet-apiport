// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Resources;
using Microsoft.Fx.Portability;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.IO;

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
#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread
                _statusBar.SetText(task);
#pragma warning restore VSTHRD010 // Invoke single-threaded types on Main thread
            }

            public override void Abort()
            {
                base.Abort();
#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread
                _statusBar.SetText(LocalizedStrings.AnalysisFailed);
#pragma warning restore VSTHRD010 // Invoke single-threaded types on Main thread
            }
        }
    }
}
