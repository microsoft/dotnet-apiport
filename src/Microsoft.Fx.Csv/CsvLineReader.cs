// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Microsoft.Fx.Csv
{
	internal sealed class CsvLineReader : IDisposable, IEnumerable<IEnumerable<string>>
	{
		private TextReader _textReader;
		private CsvSettings _settings;
		private char _next;
		private List<string> _fields = new List<string>();
		private StringBuilder _sb = new StringBuilder();

		private const char Eof = '\0';
		private const char CarriageReturn = '\r';
		private const char LineFeed = '\n';

		public CsvLineReader(TextReader textReader, CsvSettings settings)
		{
			_textReader = textReader;
			_settings = settings;
			_next = ToChar(_textReader.Peek());
		}

		public void Dispose()
		{
			_textReader.Dispose();
		}

		private char Read()
		{
			var current = ToChar(_textReader.Read());
			_next = ToChar(_textReader.Peek());
			return current;
		}

		private char Peek()
		{
			return _next;
		}

		private static char ToChar(int c)
		{
			return c < 0
					   ? Eof
					   : (char)c;
		}

		public IEnumerator<IEnumerable<string>> GetEnumerator()
		{
			var line = ReadLine();
			while (line != null)
			{
				yield return line;
				line = ReadLine();
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private IEnumerable<string> ReadLine()
		{
			if (Peek() == Eof)
				return null;

			_fields.Clear();
			var field = ReadField();
			while (field != null)
			{
				_fields.Add(field);
				field = ReadField();
			}

			return _fields;
		}

		private string ReadField()
		{
			if (Peek() == CarriageReturn)
			{
				Read();

				if (Peek() == LineFeed)
					Read();

				return null;
			}

			if (Peek() == LineFeed)
			{
				Read();
				return null;
			}

			ReadWhitespace();

			var firstChar = Peek();
			if (firstChar == Eof)
				return null;

			return firstChar == _settings.TextQualifier
					   ? ReadQualifiedField()
					   : ReadUnqualifiedField();
		}

		private void ReadWhitespace()
		{
			var c = Peek();
			while (Char.IsWhiteSpace(c) && c != _settings.Delimiter && c != CarriageReturn && c != LineFeed && c != Eof)
			{
				Read();
				c = Peek();
			}
		}

		private string ReadQualifiedField()
		{
			// Skip first quote
			Read();

			_sb.Clear();
			var c = Read();
			while (c != Eof)
			{
				if (c == _settings.TextQualifier)
				{
					if (Peek() == _settings.TextQualifier)
					{
						// Escaped quote
						// Skip one of the two qotes.
						Read();
					}
					else
					{
						// End of field
						break;
					}
				}
				_sb.Append(c);
				c = Read();
			}

			var result = _sb.ToString();

			// Skip everything up to and including the separator.
			ReadUnqualifiedField();

			return result;
		}

		private string ReadUnqualifiedField()
		{
			var c = Peek();
			_sb.Clear();
			while (c != _settings.Delimiter && c != CarriageReturn && c != LineFeed && c != Eof)
			{
				_sb.Append(c);
				Read();
				c = Peek();
			}

			if (c == _settings.Delimiter)
				Read();

			return _sb.ToString();
		}
	}
}
