// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPort.Resources;
using Microsoft.Fx.Portability;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace ApiPort
{
    public sealed class ConsoleProgressReporter : IProgressReporter
    {
        private readonly List<string> _issuesReported = new List<string>();
        private readonly List<ConsoleProgressTask> _progressTasks = new List<ConsoleProgressTask>();

        private bool _disposed = false;

        public IProgressTask StartTask(string taskName)
        {
            var task = new ConsoleProgressTask(taskName, null);
            _progressTasks.Add(task);
            return task;
        }

        public IProgressTask StartTask(string taskName, int total)
        {
            var task = new ConsoleProgressTask(taskName, total);
            _progressTasks.Add(task);
            return task;
        }

        public IReadOnlyCollection<string> Issues { get { return _issuesReported.AsReadOnly(); } }

        public void ReportIssue(string issue)
        {
            _issuesReported.Add(issue);
        }

        /// <summary>
        /// Suspends all progress tasks.
        /// </summary>
        public void Suspend()
        {
            foreach (var task in _progressTasks)
            {
                task.SuspendAnimation();
            }
        }

        /// <summary>
        /// Resumes all progress tasks.
        /// </summary>
        public void Resume()
        {
            foreach (var task in _progressTasks)
            {
                task.ResumeAnimation();
            }
        }

        private static void WriteColor(string message, ConsoleColor color)
        {
            var previousColor =
#if LINUX
                // Console.get_ForegroundColor is unsopported by the Linux PAL
                ConsoleColor.White;
#else // LINUX
                Console.ForegroundColor;
#endif // LINUX
            try
            {
                Console.ForegroundColor = color;
                Console.Write(message);
            }
            finally
            {
                Console.ForegroundColor = previousColor;
            }
        }

        private static void WriteColorLine(string message, ConsoleColor color)
        {
            WriteColor(message, color);
            Console.WriteLine();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;

                foreach (var task in _progressTasks)
                {
                    task.Dispose();
                }

                _progressTasks.Clear();
            }
        }

        private sealed class ConsoleProgressTask : IProgressTask
        {
            private static readonly TimeSpan MaxWaitTime = TimeSpan.FromMinutes(10);
            private static readonly char[] InProgressCharacters = new char[] { '-', '\\', '|', '/' };

            private readonly Task _animationTask;

            private readonly CancellationTokenSource _cancellationSource = new CancellationTokenSource();
            private readonly ManualResetEventSlim _animationResetEvent = new ManualResetEventSlim(initialState: true);

            private readonly int? _totalCount;
            private readonly string _task;

            private bool _completed = false;

            public ConsoleProgressTask(string task, int? totalCount)
            {
                _task = task.TrimEnd().PadRight(50);
                Console.Write(_task);

                _totalCount = totalCount;
                _animationTask = RunBusyAnimation(_cancellationSource.Token);
            }

            private async Task RunBusyAnimation(CancellationToken cancelToken)
            {
                await Task.Delay(1);
                var count = 0;

                // 'left' is the last character written after the task name was written.
                var left = Console.CursorLeft;
                while (!cancelToken.IsCancellationRequested)
                {
                    _animationResetEvent.Wait(MaxWaitTime, cancelToken);

                    Console.SetCursorPosition(left, Console.CursorTop);

                    WriteColor(string.Format(CultureInfo.CurrentCulture, LocalizedStrings.ProgressReportInProgress, new string('.', ++count % 4).PadRight(3)), ConsoleColor.Yellow);

                    await Task.Delay(350);
                }

                // Make sure we remove the last characted that we wrote.
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(" ".PadLeft(Console.WindowWidth - 1));
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(_task);
            }

            public void ReportUnitComplete()
            {
            }

            public void SuspendAnimation()
            {
                if (_completed)
                {
                    return;
                }

                _animationResetEvent.Reset();
            }

            public void ResumeAnimation()
            {
                if (_completed)
                {
                    return;
                }

                Console.Write(_task);
                _animationResetEvent.Set();
            }

            public void Abort()
            {
                EndTask(LocalizedStrings.ProgressReportFailed, ConsoleColor.Red);
            }

            public void Dispose()
            {
                EndTask(LocalizedStrings.ProgressReportDone, ConsoleColor.Green);
            }

            private void EndTask(string message, ConsoleColor color)
            {
                if (_completed)
                {
                    return;
                }

                _completed = true;

                if (_cancellationSource != null)
                {
                    _cancellationSource.Cancel();
                }

                if (_animationTask != null)
                {
                    _animationTask.Wait();
                }

                WriteColorLine(message, color);
            }
        }
    }
}
