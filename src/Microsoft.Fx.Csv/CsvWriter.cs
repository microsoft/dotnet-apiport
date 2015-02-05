// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.Fx.Csv
{
	public abstract class CsvWriter : IDisposable
	{
		private CsvSettings _settings;

		protected CsvWriter(CsvSettings settings)
		{
			_settings = settings;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
		}

		public abstract void Write(string value);

		public virtual void Write(IEnumerable<string> values)
		{
			foreach (var value in values)
				Write(value);
		}

		public abstract void WriteLine();

		public virtual void WriteLine(IEnumerable<string> values)
		{
			foreach (var value in values)
				Write(value);

			WriteLine();
		}

		public virtual CsvSettings Settings { get { return _settings; } set { _settings = value; } }
	}
}
