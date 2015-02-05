// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.Fx.Progress
{
	public static class ProgressMonitorExtensions
	{
		public static IProgressMonitor CreateChild(this IProgressMonitor parentProgressMonitor, float totalWorkInParent)
		{
			return parentProgressMonitor == null
					   ? null
					   : new ChildProgressMonitor(parentProgressMonitor, totalWorkInParent);
		}

		public static T RunChild<T>(this IProgressMonitor parent, float totalWorkInParent, Func<IProgressMonitor, T> operation)
		{
			using (var cpm = parent.CreateChild(totalWorkInParent))
				return operation(cpm);
		}

		public static void RunChild(this IProgressMonitor parent, float totalWorkInParent, Action<IProgressMonitor> operation)
		{
			using (var cpm = parent.CreateChild(totalWorkInParent))
				operation(cpm);
		}

		public static void SetTask(this IProgressMonitor progressMonitor, string format, params object[] args)
		{
			var details = string.Format(format, args);
			progressMonitor.SetTask(details);
		}

		public static void SetDetails(this IProgressMonitor progressMonitor, string format, params object[] args)
		{
			var details = string.Format(format, args);
			progressMonitor.SetDetails(details);
		}

		public static IEnumerable<T> WithProgress<T>(this ICollection<T> source, IProgressMonitor progressMonitor)
		{
			return source.WithProgress(source.Count, progressMonitor);
		}

		public static IEnumerable<T> WithProgress<T>(this IEnumerable<T> source, int count, IProgressMonitor progressMonitor)
		{
			progressMonitor.SetRemainingWork(count);
			foreach (var element in source)
			{
				yield return element;
				progressMonitor.Report(1);
			}
		}
	}
}
