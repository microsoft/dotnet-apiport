// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;

namespace Microsoft.Fx.Progress
{
	internal sealed class CancelingProgressMonitor : IProgressMonitor
	{
		private IProgressMonitor _progressMonitor;

		public CancelingProgressMonitor(IProgressMonitor progressMonitor)
		{
			_progressMonitor = progressMonitor;
		}

		public void Dispose()
		{
			_progressMonitor.Dispose();
		}

		public void SetTask(string description)
		{
			_progressMonitor.CancellationToken.ThrowIfCancellationRequested();
			_progressMonitor.SetTask(description);
		}

		public void SetDetails(string description)
		{
			_progressMonitor.CancellationToken.ThrowIfCancellationRequested();
			_progressMonitor.SetDetails(description);
		}

		public void SetRemainingWork(float totalUnits)
		{
			_progressMonitor.CancellationToken.ThrowIfCancellationRequested();
			_progressMonitor.SetRemainingWork(totalUnits);
		}

		public void Report(float units)
		{
			_progressMonitor.CancellationToken.ThrowIfCancellationRequested();
			_progressMonitor.Report(units);
		}

		public CancellationToken CancellationToken
		{
			get { return _progressMonitor.CancellationToken; }
		}
	}
}
