// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;

namespace Microsoft.Fx.Progress
{
	public class ProgressStream : Stream
	{
		private Stream _stream;
		private IProgressMonitor _progressMonitor;
		private long _lastPosition;

		public ProgressStream(Stream stream, IProgressMonitor progressMonitor)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");

			if (!stream.CanSeek)
				throw new ArgumentException("stream must support seeking", "stream");

			if (progressMonitor == null)
				throw new ArgumentNullException("progressMonitor");

			_stream = stream;
			_progressMonitor = progressMonitor;
			_progressMonitor.SetRemainingWork(_stream.Length - _stream.Position);
		}

		public override void Flush()
		{
			_stream.Flush();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			var result = _stream.Seek(offset, origin);
			UpdateProgress();
			return result;
		}

		public override void SetLength(long value)
		{
			_stream.SetLength(value);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			var result = _stream.Read(buffer, offset, count);
			UpdateProgress();
			return result;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			_stream.Write(buffer, offset, count);
			UpdateProgress();
		}

		public override int ReadByte()
		{
			var readByte = _stream.ReadByte();
			UpdateProgress();
			return readByte;
		}

		public override void WriteByte(byte value)
		{
			_stream.WriteByte(value);
		}

		public override bool CanRead
		{
			get { return _stream.CanRead; }
		}

		public override bool CanSeek
		{
			get { return _stream.CanSeek; }
		}

		public override bool CanWrite
		{
			get { return _stream.CanWrite; }
		}

		public override long Length
		{
			get { return _stream.Length; }
		}

		public override long Position
		{
			get { return _stream.Position; }
			set
			{
				_stream.Position = value;
				UpdateProgress();
			}
		}

		private void UpdateProgress()
		{
			var delta = Math.Abs(Position - _lastPosition);
			_lastPosition = Position;
			_progressMonitor.Report(delta);
		}
	}
}
