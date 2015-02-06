// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;

namespace Microsoft.Fx.Progress
{
	internal sealed class ConntectableProgressMonitor : IProgressMonitor
	{
		private CancellationToken _cancellationToken;
		private object _lock = new object();
		private IProgressMonitor _target;
		private bool _started;
		private float _unitsWorked;
		private float _totalUnits;
		private string _task;
		private string _details;
		private bool _disposed;

		public ConntectableProgressMonitor(CancellationToken cancellationToken)
		{
			_cancellationToken = cancellationToken;
		}

		public void Connect(IProgressMonitor target)
		{
			lock (_lock)
			{
				_target = target;

				_target.SetTask(_task);
				_target.SetDetails(_details);

				if (_started)
				{
					_target.SetRemainingWork(_totalUnits);
					if (_unitsWorked > 0.0f)
						_target.Report(_unitsWorked);
				}

				if (_disposed)
					_target.Dispose();
			}
		}

		public void Dispose()
		{
			lock (_lock)
			{
				_disposed = true;
				if (_target != null)
					_target.Dispose();
			}
		}

		public void SetTask(string description)
		{
			lock (_lock)
			{
				_task = description;

				if (_target != null)
					_target.SetTask(description);
			}
		}

		public void SetDetails(string description)
		{
			lock (_lock)
			{
				_details = description;

				if (_target != null)
					_target.SetDetails(description);
			}
		}

		public void SetRemainingWork(float totalUnits)
		{
			lock (_lock)
			{
				_totalUnits = totalUnits;
				_unitsWorked = 0.0f;

				_started = true;
				if (_target != null)
					_target.SetRemainingWork(totalUnits);
			}
		}

		public void Report(float units)
		{
			lock (_lock)
			{
				_unitsWorked += units;

				if (_target != null)
					_target.Report(units);
			}
		}

		public CancellationToken CancellationToken
		{
			get { return _cancellationToken; }
		}
	}
}
