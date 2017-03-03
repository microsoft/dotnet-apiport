// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Resources;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Fx.Portability
{
    public class TextWriterProgressReporter : IProgressReporter
    {
        private readonly List<string> _issuesReported = new List<string>();
        private readonly TextWriter _textWriter;

        public TextWriterProgressReporter(TextWriter textWriter)
        {
            _textWriter = textWriter;
        }

        public IProgressTask StartTask(string taskName)
        {
            return new TextWriterProgressTask(_textWriter, taskName);
        }

        public IProgressTask StartTask(string taskName, int total)
        {
            return StartTask(taskName);
        }

        public IReadOnlyCollection<string> Issues { get { return _issuesReported.AsReadOnly(); } }

        public void ReportIssue(string issue)
        {
            _issuesReported.Add(issue);
        }

        public void Suspend()
        { }

        public void Resume()
        { }

        public void Dispose()
        { }

        private sealed class TextWriterProgressTask : IProgressTask
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

            public void ReportUnitComplete()
            {
            }

            public void Abort()
            {
                EndTask(LocalizedStrings.ProgressReportFailed);
            }

            public void Dispose()
            {
                EndTask(LocalizedStrings.ProgressReportDone);
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