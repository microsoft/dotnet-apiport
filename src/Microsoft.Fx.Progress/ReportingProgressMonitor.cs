// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Threading;

namespace Microsoft.Fx.Progress
{
	internal sealed class ReportingProgressMonitor : IProgressMonitor
	{
		private ProgressReporter _progressReporter;
		private float _unitsWorked;
		private float _totalUnits;
		private Stopwatch _stopwatch;

		public ReportingProgressMonitor(ProgressReporter progressReporter)
		{
			_progressReporter = progressReporter;
			_progressReporter.Start();
		}

		public void Dispose()
		{
			if (_progressReporter == null)
				return;

			_progressReporter.PercentageComplete = 1.0f;
			_progressReporter.Finish();
			_progressReporter = null;
		}

		public void SetTask(string description)
		{
			if (_progressReporter == null)
				return;

			_progressReporter.Task = description;
		}

		public void SetDetails(string description)
		{
			if (_progressReporter == null)
				return;

			_progressReporter.Details = description;
		}

		public void SetRemainingWork(float totalUnits)
		{
			if (_progressReporter == null)
				return;

			_progressReporter.IsIndeterminate = float.IsNaN(totalUnits);
			_totalUnits = totalUnits;
			_unitsWorked = 0.0f;

			if (_stopwatch == null)
				_stopwatch = Stopwatch.StartNew();
		}

		public void Report(float units)
		{
			if (_progressReporter == null)
				return;

			var remainingWork = _totalUnits - _unitsWorked;
			var remainingPercentage = 1.0f - _progressReporter.PercentageComplete;
			var workedPercentage = units / remainingWork * remainingPercentage;

			_unitsWorked += units;
			_progressReporter.PercentageComplete += workedPercentage;

			UpdateRemainingTime();
		}

		public CancellationToken CancellationToken
		{
			get
			{
				return _progressReporter == null
						   ? CancellationToken.None
						   : _progressReporter.CancellationToken;
			}
		}

		private void UpdateRemainingTime()
		{
			if (_stopwatch == null || _progressReporter == null)
				return;

			var elapsedPercentage = _progressReporter.PercentageComplete;
			var elapsedMilliseconds = _stopwatch.Elapsed.TotalMilliseconds;

			if (elapsedPercentage > 0.0f && elapsedMilliseconds > 0)
			{
				var remainingPercentage = 1.0f - elapsedPercentage;
				var remainingMilliseconds = elapsedMilliseconds / elapsedPercentage * remainingPercentage;
				_progressReporter.RemainingTime = TimeSpan.FromMilliseconds(remainingMilliseconds);
			}
		}
	}
}
