// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;

namespace Microsoft.Fx.Progress
{
	public interface IProgressReporter
	{
		void Cancel();

		bool IsRunning { get; }
		CancellationToken CancellationToken { get; }
		bool CanCancel { get; }
		string Task { get; }
		string Details { get; }
		float PercentageComplete { get; }
		bool IsIndeterminate { get; }
		TimeSpan RemainingTime { get; set; }

		event EventHandler IsRunningChanged;
		event EventHandler TaskChanged;
		event EventHandler DetailsChanged;
		event EventHandler PercentageCompleteChanged;
		event EventHandler IsIndeterminateChanged;
		event EventHandler RemainingTimeChanged;
	}
}
