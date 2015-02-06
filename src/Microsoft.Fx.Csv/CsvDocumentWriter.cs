// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.Fx.Csv
{
    internal sealed class CsvDocumentWriter : CsvWriter
    {
        private IList<string> _keys;
        private IList<IDictionary<string, string>> _rows;

        private int _currentKey;
        private IDictionary<string, string> _currentRow = new Dictionary<string, string>();

        public CsvDocumentWriter(IList<string> keys, IList<IDictionary<string, string>> rows)
            : base(CsvSettings.Default)
        {
            _keys = keys;
            _rows = rows;
        }

        public override void Write(string value)
        {
            var key = _keys[_currentKey];
            _currentRow[key] = value;
            _currentKey++;
        }

        public override void WriteLine()
        {
            _rows.Add(_currentRow);
            _currentKey = 0;
            _currentRow = new Dictionary<string, string>();
        }
    }
}
