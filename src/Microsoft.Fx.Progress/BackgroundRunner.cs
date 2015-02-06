// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Fx.Progress
{
	public sealed class BackgroundRunner
	{
		private IProgressMonitor _reportingProgressMonitor;
		private CancellationTokenSource _cancellationTokenSource;

		private ProgressReporter _progressReporter;

		public BackgroundRunner()
		{
			_progressReporter = new ProgressReporter();
		}

		public IProgressReporter ProgressReporter
		{
			get { return _progressReporter; }
		}

		public Task RunAsync(Action<IProgressMonitor> operation)
		{
			return RunAsync(operation, true);
		}

		public void RunAsync(Action<IProgressMonitor> operation, Action completionHandler)
		{
			RunAsync(operation, completionHandler, true);
		}

		public void RunAsync<T>(Func<IProgressMonitor, T> operation, Action<T> completionHandler)
		{
			RunAsync(operation, completionHandler, true);
		}

		public Task RunNoncancelableAsync(Action<IProgressMonitor> operation)
		{
			return RunAsync(operation, false);
		}

		public void RunNoncancelableAsync(Action<IProgressMonitor> operation, Action completionHandler)
		{
			RunAsync(operation, completionHandler, false);
		}

		public void RunNoncancelableAsync<T>(Func<IProgressMonitor, T> operation, Action<T> completionHandler)
		{
			RunAsync(operation, completionHandler, false);
		}

		private Task RunAsync(Action<IProgressMonitor> operation, bool supportsCancellation)
		{
			return InternalRunAsync(operation, null, supportsCancellation);
		}

		private void RunAsync(Action<IProgressMonitor> operation, Action completionHandler, bool supportsCancellation)
		{
			InternalRunAsync(operation, completionHandler, supportsCancellation);
		}

		private void RunAsync<T>(Func<IProgressMonitor, T> operation, Action<T> completionHandler, bool supportsCancellation)
		{
			var result = default(T);
			Action<IProgressMonitor> a = pm => result = operation(pm);
			Action c = () => completionHandler(result);
			RunAsync(a, c, supportsCancellation);
		}

		private Task InternalRunAsync(Action<IProgressMonitor> operation, Action completionHandler, bool supportsCancellation)
		{
			if (_reportingProgressMonitor != null)
			{
				_reportingProgressMonitor.Dispose();
				_reportingProgressMonitor = null;
			}

			if (_cancellationTokenSource != null)
			{
				_cancellationTokenSource.Cancel();
				_cancellationTokenSource = null;
			}

			var cts = new CancellationTokenSource();
			var ct = cts.Token;

			var reportingProgressMonitor = supportsCancellation
											   ? _progressReporter.CreateMonitor(cts)
											   : _progressReporter.CreateMonitor(ct);

			var task = RunAsync(reportingProgressMonitor, operation);

			if (completionHandler != null)
			{
				task.ContinueWith(t => completionHandler(),
										   ct,
										   TaskContinuationOptions.NotOnCanceled,
										   TaskScheduler.FromCurrentSynchronizationContext());
			}

			_reportingProgressMonitor = reportingProgressMonitor;
			_cancellationTokenSource = cts;

			return task;
		}

		private static Task RunAsync(IProgressMonitor progressMonitor, Action<IProgressMonitor> operation)
		{
			//      throttledProgressMonitor
			//   -> throwingProgressMonitor
			//   -> conntectableProgressMonitor
			//   -> synchronizedProgressMonitor              Worker Thread
			//  ----------------------------------------------------------
			//   -> progressMonitor                              UI Thread

			var synchronizedProgressMonitor = new SynchronizedProgressMonitor(progressMonitor);
			var conntectableProgressMonitor = new ConntectableProgressMonitor(progressMonitor.CancellationToken);
			var cancelingProgressMonitor = new CancelingProgressMonitor(conntectableProgressMonitor);
			var throttledProgressMonitor = new ThrottledProgressMonitor(cancelingProgressMonitor);

			var operationTask = Task.Run(() => operation(throttledProgressMonitor), progressMonitor.CancellationToken);

			operationTask.ContinueWith(t =>
			{
				throttledProgressMonitor.Dispose();
				cancelingProgressMonitor.Dispose();
				conntectableProgressMonitor.Dispose();
				synchronizedProgressMonitor.Dispose();
				progressMonitor.Dispose();
			}, TaskScheduler.FromCurrentSynchronizationContext());

			if (!operationTask.Wait(300))
				conntectableProgressMonitor.Connect(synchronizedProgressMonitor);

			return operationTask;
		}
	}
}
