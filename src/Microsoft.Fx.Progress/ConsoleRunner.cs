// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Threading;

namespace Microsoft.Fx.Progress
{
	public static class ConsoleRunner
	{
		private static Stopwatch s_stopwatch;
		public static void Run(Action<IProgressMonitor> operation)
		{
			if (Console.IsOutputRedirected)
				RunWithRedirectedConsole(operation);
			else
				RunWithConsoleWindow(operation);
		}

		private static void RunWithRedirectedConsole(Action<IProgressMonitor> operation)
		{
			var progressReporter = new ProgressReporter();
			progressReporter.TaskChanged += (s, e) => Console.WriteLine(progressReporter.Task);
			progressReporter.DetailsChanged += (s, e) => Console.WriteLine("  {0}", progressReporter.Details);

			s_stopwatch = Stopwatch.StartNew();
			Run(operation, progressReporter);
			s_stopwatch.Stop();

			Console.WriteLine("  Done in {0:g}.", s_stopwatch.Elapsed);
		}

		private static void RunWithConsoleWindow(Action<IProgressMonitor> operation)
		{
			var currentLeft = Console.CursorLeft;
			if (currentLeft > 0)
				Console.WriteLine();

			// If we have no more room in the buffer, double the size.
			if (Console.BufferHeight - Console.CursorTop <= 2)
			{
				Console.SetBufferSize(Console.BufferWidth, Math.Min(Int16.MaxValue, Console.BufferHeight * 2));
			}

			var currentTop = Console.CursorTop;

			var progressReporter = new ProgressReporter();

			var updateHandler = new EventHandler((s, e) => UpdateOutput(currentTop, progressReporter));

			progressReporter.TaskChanged += updateHandler;
			progressReporter.DetailsChanged += updateHandler;
			progressReporter.PercentageCompleteChanged += updateHandler;
			progressReporter.RemainingTimeChanged += updateHandler;

			s_stopwatch = Stopwatch.StartNew();

			Run(operation, progressReporter);

			s_stopwatch.Stop();

			WriteDone(currentTop, s_stopwatch.Elapsed, progressReporter.Task);
		}

		private static void Run(Action<IProgressMonitor> operation, ProgressReporter progressReporter)
		{
			var cancelHandler = new ConsoleCancelEventHandler((s, e) => progressReporter.Cancel());

			Console.CancelKeyPress += cancelHandler;
			try
			{
				using (var progressMonitor = progressReporter.CreateMonitor(CancellationToken.None))
				using (var throttledProgressMonitor = new ThrottledProgressMonitor(progressMonitor, TimeSpan.FromMilliseconds(500)))
					operation(throttledProgressMonitor);
			}
			finally
			{
				Console.CancelKeyPress -= cancelHandler;
			}
		}

		private static void ClearOutput(int top)
		{
			var emptyString = new string(' ', Console.BufferWidth);

			Console.SetCursorPosition(0, top);
			Console.Write(emptyString);

			Console.SetCursorPosition(0, top + 1);
			Console.Write(emptyString);

			Console.SetCursorPosition(0, top + 2);
			Console.Write(emptyString);
		}

		private static void UpdateOutput(int top, ProgressReporter progressReporter)
		{
			ClearOutput(top);

			Console.SetCursorPosition(0, top);
			Console.Write(progressReporter.Task);

			Console.SetCursorPosition(0, top + 1);
			Console.Write("  {0}", progressReporter.Details);

			Console.SetCursorPosition(0, top + 2);
			Console.Write("  {0:P2} - ETA {1:g}, Elapsed {2:g}",
				progressReporter.PercentageComplete,
				progressReporter.RemainingTime,
				s_stopwatch == null ? TimeSpan.FromMilliseconds(0) : s_stopwatch.Elapsed);
		}

		private static void WriteDone(int top, TimeSpan elapsed, string task)
		{
			ClearOutput(top);
			Console.SetCursorPosition(0, top);

			if (task.EndsWith("..."))
				task = task.Substring(0, task.Length - 3);
			else if (task.EndsWith("."))
				task = task.Substring(0, task.Length - 1);

			Console.WriteLine("{0}. Done in {1:g}.", task, elapsed);
		}

		public static T Run<T>(Func<IProgressMonitor, T> operation)
		{
			var result = default(T);
			var action = new Action<IProgressMonitor>(pm =>
			{
				result = operation(pm);
			});

			Run(action);
			return result;
		}
	}
}
