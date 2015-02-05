// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.Contracts;
using System.Threading;

namespace Microsoft.Fx.Progress
{
	internal sealed class ChildProgressMonitor : IProgressMonitor
	{
		private IProgressMonitor _parentProgressMonitor;
		private bool _started;
		private float _totalWorkInParent;
		private float _unitsWorkedInParent;
		private float _totalUnits;
		private float _unitsWorked;

		public ChildProgressMonitor(IProgressMonitor parentProgressMonitor, float totalWorkInParent)
		{
			Contract.Requires(parentProgressMonitor != null);

			_parentProgressMonitor = parentProgressMonitor;
			_totalWorkInParent = totalWorkInParent;
		}

		public void Dispose()
		{
			// Make sure we report to our parent that we are finished

			if (!_started)
			{
				SetRemainingWork(1);
				Report(1);
			}
			else
			{
				var remainingWork = _totalUnits - _unitsWorked;
				if (remainingWork > 0)
					Report(remainingWork);
			}
		}

		public void SetTask(string description)
		{
			_parentProgressMonitor.SetTask(description);
		}

		public void SetDetails(string description)
		{
			_parentProgressMonitor.SetDetails(description);
		}

		public void SetRemainingWork(float totalUnits)
		{
			_started = true;
			_totalUnits = totalUnits;
			_unitsWorked = 0.0f;
		}

		public void Report(float units)
		{
			if (units == 0.0 || !_started)
				return;

			var remainingWork = _totalUnits - _unitsWorked;
			var remainingWorkInParent = _totalWorkInParent - _unitsWorkedInParent;
			var unitsInParent = units / remainingWork * remainingWorkInParent;

			_unitsWorked += units;
			_unitsWorkedInParent += unitsInParent;
			_parentProgressMonitor.Report(unitsInParent);
		}

		public CancellationToken CancellationToken
		{
			get { return _parentProgressMonitor.CancellationToken; }
		}
	}
}
