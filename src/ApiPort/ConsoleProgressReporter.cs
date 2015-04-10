// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPort.Resources;
using Microsoft.Fx.Portability;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ApiPort
{
    public class ConsoleProgressReporter : IProgressReporter
    {
        private readonly List<string> _issuesReported = new List<string>();

        public IProgressTask StartTask(string taskName)
        {
            return new ConsoleProgressTask(taskName, null);
        }

        public IProgressTask StartTask(string taskName, int total)
        {
            return new ConsoleProgressTask(taskName, total);
        }

        public IReadOnlyCollection<string> Issues { get { return _issuesReported.AsReadOnly(); } }

        public void ReportIssue(string issue)
        {
            _issuesReported.Add(issue);
        }

        private static void WriteColor(string message, ConsoleColor color)
        {
            var previousColor = Console.ForegroundColor;

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

        private sealed class ConsoleProgressTask : IProgressTask
        {
            private readonly static char[] s_chars = new char[] { '-', '\\', '|', '/' };

            private readonly Task _animationTask;
            private readonly CancellationTokenSource _cancellationSource = new CancellationTokenSource();
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

                {
                    var count = 0;

                    // 'left' is the last character written after the task name was written.
                    var left = Console.CursorLeft;
                    while (!cancelToken.IsCancellationRequested)
                    {
                        Console.SetCursorPosition(left, Console.CursorTop);

                        WriteColor(string.Format(LocalizedStrings.ProgressReportInProgress, new string('.', ++count % 4).PadRight(3)), ConsoleColor.Yellow);


                        await Task.Delay(350);
                    }

                    // Make sure we remove the last characted that we wrote.
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write(" ".PadLeft(Console.WindowWidth - 1));
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write(_task);
                }
            }

            public void ReportUnitComplete()
            {
            }

            public void Abort()
            {
                EndTask(Microsoft.Fx.Portability.Resources.LocalizedStrings.ProgressReportFailed, ConsoleColor.Red);
            }

            public void Dispose()
            {
                EndTask(Microsoft.Fx.Portability.Resources.LocalizedStrings.ProgressReportDone, ConsoleColor.Green);
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
