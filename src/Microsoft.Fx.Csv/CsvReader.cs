// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.Fx.Csv
{
	public abstract class CsvReader : IDisposable
	{
		protected CsvReader(CsvSettings settings)
		{
			Settings = settings;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
		}

		public abstract IEnumerable<string> Read();

		public CsvSettings Settings { get; set; }
	}
}
