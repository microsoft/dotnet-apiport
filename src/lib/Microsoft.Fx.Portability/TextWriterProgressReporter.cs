// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Resources;
using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Fx.Portability
{
    public class TextWriterProgressReporter : IProgressReporter
    {
        private readonly List<string> _issuesReported = new List<string>();

        public TextWriterProgressReporter(TextWriter textWriter)
        {
            Writer = textWriter;
        }

        protected TextWriter Writer { get; }

        public virtual IProgressTask StartTask(string taskName)
        {
            return new TextWriterProgressTask(Writer, taskName);
        }

        public virtual IProgressTask StartTask(string taskName, int total)
        {
            return StartTask(taskName);
        }

        public IReadOnlyCollection<string> Issues => _issuesReported.AsReadOnly();

        public void ReportIssue(string issue)
        {
            _issuesReported.Add(issue);
        }

        public void Suspend()
        { }

        public void Resume()
        { }

        protected virtual void Dispose(bool disposing)
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected class TextWriterProgressTask : IProgressTask
        {
            private readonly TextWriter _textWriter;
            private readonly string _task;

            private bool _completed = false;

            public TextWriterProgressTask(TextWriter textWriter, string task)
            {
                _textWriter = textWriter;
                _task = task.TrimEnd().PadRight(50);
                _textWriter.Write(_task + " ");
            }

            public virtual void ReportUnitComplete()
            {
            }

            public virtual void Abort()
            {
                EndTask(LocalizedStrings.ProgressReportFailed);
            }

            protected virtual void Dispose(bool disposing)
            {
                EndTask(LocalizedStrings.ProgressReportDone);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void EndTask(string message)
            {
                if (_completed)
                {
                    return;
                }

                _completed = true;

                _textWriter.WriteLine(message);
            }
        }
    }
}
