// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.Fx.Progress
{
	public interface IModalBackgroundRunner
	{
		void Run(Action<IProgressMonitor> action);
		void RunNoncancelable(Action<IProgressMonitor> action);

		T Run<T>(Func<IProgressMonitor, T> action);
		T RunNoncancelable<T>(Func<IProgressMonitor, T> action);
	}
}
