// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;

namespace Microsoft.Fx.Progress
{
	public interface IProgressMonitor : IDisposable
	{
		void SetTask(string description);
		void SetDetails(string description);
		void SetRemainingWork(float totalUnits);
		void Report(float units);

		CancellationToken CancellationToken { get; }
	}
}
