// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;

namespace Microsoft.Fx.Progress
{
	internal sealed class SynchronizedProgressMonitor : IProgressMonitor
	{
		private SynchronizationContext _synchronizationContext = SynchronizationContext.Current;
		private IProgressMonitor _target;

		public SynchronizedProgressMonitor(IProgressMonitor target)
		{
			_target = target;
		}

		public void Dispose()
		{
			_synchronizationContext.Post(s => _target.Dispose(), null);
		}

		public void SetTask(string description)
		{
			_synchronizationContext.Post(s => _target.SetTask(description), null);
		}

		public void SetDetails(string description)
		{
			_synchronizationContext.Post(s => _target.SetDetails(description), null);
		}

		public void SetRemainingWork(float totalUnits)
		{
			_synchronizationContext.Post(s => _target.SetRemainingWork(totalUnits), null);
		}

		public void Report(float units)
		{
			_synchronizationContext.Post(s => _target.Report(units), null);
		}

		public CancellationToken CancellationToken
		{
			get { return _target.CancellationToken; }
		}
	}
}
