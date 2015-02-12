// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Microsoft.Fx.Portability.Analyzer
{
	public class ReflectionMetadataDependencyFinder : IDependencyFinder
	{
		public IDependencyInfo FindDependencies(IEnumerable<FileInfo> inputAssemblies, IProgressReporter _progressReporter)
		{
			var inputAssemblyPaths = inputAssemblies.Where(f => FilterValidFiles(f, _progressReporter)).Select(i => i.FullName).ToList();

			_progressReporter.StartParallelTask(LocalizedStrings.DetectingAssemblyReferences, String.Format(CultureInfo.CurrentCulture, LocalizedStrings.ProcessedFiles, "{0}", inputAssemblyPaths.Count));
			var computedDependencies = DependencyFinderEngine.ComputeDependencies(inputAssemblyPaths, _progressReporter);
			_progressReporter.FinishTask();

			return computedDependencies;
		}

		private static bool FilterValidFiles(FileInfo file, IProgressReporter _progressReporter)
		{
			if (file.Exists)
			{
				return true;
			}

			_progressReporter.ReportIssue(LocalizedStrings.UnknownFile, file.FullName);

			return false;
		}
	}
}
