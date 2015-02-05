// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPort.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Fx.Portability;

namespace ApiPort
{
    public class ConsoleProgressReporter : IProgressReporter
    {
        private Task _animationTask;
        private CancellationTokenSource _cancellationSource;
        private int _completedUnits = 0;
        private bool _isParallel = false;

        private Stopwatch _timer = new Stopwatch();
        private string _currentTask;
        private string _detailsString;
        private List<string> _issuesReported = new List<string>();

        private char[] _chars = new char[] { '-', '\\', '|', '/' };
        private int _pos = 0;

        private void RunBusyAnimation(CancellationToken cancelToken)
        {
            if (!Console.IsOutputRedirected)
            {
                int left = Console.CursorLeft;
                // left is the last character written after the task name was written.
                while (!cancelToken.IsCancellationRequested)
                {
                    Console.SetCursorPosition(left, Console.CursorTop);

                    // This shows the details for a given task
                    if (_isParallel)
                    {
                        Console.SetCursorPosition(left, Console.CursorTop);
                        Console.Write(_detailsString, _completedUnits);
                    }

                    // This is the spinning animation.
                    Console.Write(_chars[_pos = ++_pos % 4]);
                    Thread.Sleep(200);
                }

                // Make sure we remove the last characted that we wrote.
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(" ".PadLeft(Console.WindowWidth - 1));
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(_currentTask);
            }
        }

        public void StartTask(string taskName)
        {
            // make sure we have a trailing space.
            taskName = taskName.TrimEnd() + " ";
            _cancellationSource = new CancellationTokenSource();
            _isParallel = false;
            Console.Write(taskName);
            _currentTask = taskName;
            _timer.Reset();
            _timer.Start();

            _animationTask = Task.Run(() => RunBusyAnimation(_cancellationSource.Token));
        }

        public void AbortTask()
        {
            EndTask(LocalizedStrings.ProgressReportFailed);
        }

        public void FinishTask()
        {
            EndTask(LocalizedStrings.ProgressReportDone);
        }

        private void EndTask(string message)
        {
            _timer.Stop();

            if (_cancellationSource != null)
            {
                _cancellationSource.Cancel();
            }

            if (_animationTask != null)
            {
                _animationTask.Wait();
            }

            if (_isParallel)
            {
                Console.Write(_detailsString, _completedUnits);
            }

            Console.WriteLine(message, _timer.Elapsed.TotalSeconds);
        }

        public void StartParallelTask(string taskName, string details)
        {
            _completedUnits = 0;
            _detailsString = details;
            StartTask(taskName);
            _isParallel = true;
        }

        public IReadOnlyCollection<string> Issues { get { return _issuesReported.AsReadOnly(); } }

        public void ReportUnitComplete()
        {
            Interlocked.Increment(ref _completedUnits);
        }

        public void ReportIssue(string issueFormat, params object[] items)
        {
            _issuesReported.Add(string.Format(issueFormat, items));
        }
    }
}
