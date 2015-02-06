// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;

namespace Microsoft.Fx.Progress
{
	public sealed class NullProgressMonitor : IProgressMonitor
	{
		public static readonly NullProgressMonitor Instance = new NullProgressMonitor();

		private NullProgressMonitor()
		{
		}

		public void Dispose()
		{
		}

		public void SetTask(string description)
		{
		}

		public void SetDetails(string description)
		{
		}

		public void SetRemainingWork(float totalUnits)
		{
		}

		public void Report(float units)
		{
		}

		public CancellationToken CancellationToken
		{
			get { return CancellationToken.None; }
		}
	}
}
