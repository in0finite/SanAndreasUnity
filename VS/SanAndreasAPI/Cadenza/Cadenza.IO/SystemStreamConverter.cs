//
// SystemStreamConverter.cs
//
// Authors:
//   Jonathan Pryor  <jpryor@novell.com>
//   Bojan Rajkovic  <bojanr@brandeis.edu>
//
// Copyright (c) 2008 Novell, Inc. (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Cadenza.IO {

	internal sealed class SystemStreamConverter : StreamConverter
	{
		internal SystemStreamConverter (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");

			BaseStream = stream;
			buffer = new byte [8];
		}

		Stream BaseStream { get; set; }

		private void AssertRead ()
		{
			if (!BaseStream.CanRead)
				throw new InvalidOperationException ("Cannot read from stream");
		}

		private void AssertWrite ()
		{
			if (!BaseStream.CanWrite)
				throw new InvalidOperationException ("Cannot write to stream");
		}

		private readonly byte[] buffer;

		private void FillBuffer (byte[] buffer, int offset, int count)
		{
			int read;

			while ((read = BaseStream.Read (buffer, offset, count - offset)) > 0) {
				offset += read;
			}

			if (read == -1)
				throw new EndOfStreamException ();
		}

		private byte[] Take (int size)
		{
			byte[] buff = size <= buffer.Length ? buffer : new byte [size];

			FillBuffer (buff, 0, size);

			return buff;
		}

		public override IValueReader Read (out bool value)
		{
			AssertRead ();

			value = BitConverter.ToBoolean (Take (1), 0);

			return this;
		}

		public override IValueReader Read (out byte value)
		{
			AssertRead ();

			int b = BaseStream.ReadByte ();
			if (b == -1)
				throw new EndOfStreamException ();

			value = (byte) b;

			return this;
		}

		public override IValueReader Read (out char value)
		{
			AssertRead ();

			value = BitConverter.ToChar (Take (2), 0);

			return this;
		}

		public override IValueReader Read (out DateTime value)
		{
			AssertRead ();

			long ticks;
			this.Read (out ticks);
			value = new DateTime (ticks);

			return this;
		}

		public override IValueReader Read (out decimal value)
		{
			AssertRead ();

			int a, b, c, d;
			this.Read (out a).Read (out b).Read (out c).Read (out d);

			value = new Decimal (new []{a, b, c, d});

			return this;
		}

		public override IValueReader Read (out double value)
		{
			AssertRead ();

			value = BitConverter.ToDouble (Take (8), 0);

			return this;
		}

		public override IValueReader Read (out short value)
		{
			AssertRead ();

			value = BitConverter.ToInt16 (Take (2), 0);

			return this;
		}

		public override IValueReader Read (out int value)
		{
			AssertRead ();

			value = BitConverter.ToInt32 (Take (4), 0);

			return this;
		}

		public override IValueReader Read (out long value)
		{
			AssertRead ();

			value = BitConverter.ToInt64 (Take (8), 0);

			return this;
		}

		public override IValueReader Read (out float value)
		{
			AssertRead ();

			value = BitConverter.ToSingle (Take (4), 0);

			return this;
		}

		public override IValueReader Read (out string value)
		{
			int len;
			this.Read (out len);
			if (len < 0)
				throw new InvalidOperationException ("Invalid string representation");

			byte[] buf = new byte [len];
			this.Read (buf);

			value = Encoding.UTF8.GetString (buf);

			return this;
		}

		public override IValueReader Read (out sbyte value)
		{
			byte b;
			this.Read (out b);
			value = (sbyte) b;
			return this;
		}

		public override IValueReader Read (byte[] value, int offset, int count)
		{
			AssertRead ();

			FillBuffer (value, offset, count);

			return this;
		}

		public override IValueReader Read (out ushort value)
		{
			AssertRead ();

			value = BitConverter.ToUInt16 (Take (2), 0);

			return this;
		}

		public override IValueReader Read (out uint value)
		{
			AssertRead ();

			value = BitConverter.ToUInt32 (Take (4), 0);

			return this;
		}

		public override IValueReader Read (out ulong value)
		{
			AssertRead ();

			value = BitConverter.ToUInt64 (Take (8), 0);

			return this;
		}

		public override IValueReader Read (int size, Encoding encoding, out string value)
		{
			AssertRead ();

			value = encoding.GetString (Take (size));

			return this;
		}

		public override IValueWriter Write (bool value)
		{
			AssertWrite ();

			return Write (BitConverter.GetBytes (value));
		}

		public override IValueWriter Write (byte value)
		{
			AssertWrite ();

			BaseStream.WriteByte (value);

			return this;
		}

		public override IValueWriter Write (char value)
		{
			AssertWrite ();

			return Write (BitConverter.GetBytes (value));
		}

		public override IValueWriter Write (DateTime value)
		{
			AssertWrite ();

			return Write (value.Ticks);
		}

		public override IValueWriter Write (decimal value)
		{
			AssertWrite ();

			int[] bits = decimal.GetBits (value);
			if (bits.Length != 4)
				throw new NotSupportedException ("Unexpected number of elements from decimal.GetBits().");
			foreach (int b in bits)
				Write (b);

			return this;
		}

		public override IValueWriter Write (double value)
		{
			AssertWrite ();

			return Write (BitConverter.GetBytes (value));
		}

		public override IValueWriter Write (short value)
		{
			AssertWrite ();

			return Write (BitConverter.GetBytes (value));
		}

		public override IValueWriter Write (int value)
		{
			AssertWrite ();

			return Write (BitConverter.GetBytes (value));
		}

		public override IValueWriter Write (long value)
		{
			AssertWrite ();

			return Write (BitConverter.GetBytes (value));
		}

		public override IValueWriter Write (float value)
		{
			AssertWrite ();

			return Write (BitConverter.GetBytes (value));
		}

		public override IValueWriter Write (sbyte value)
		{
			AssertWrite ();

			return Write ((byte) value);
		}

		public override IValueWriter Write (string value)
		{
			AssertWrite ();

			byte[] data = Encoding.UTF8.GetBytes (value);

			Write (data.Length);
			return Write (data);
		}

		public override StreamConverter Write (byte[] value, int offset, int count)
		{
			AssertWrite ();
			if (value == null)
				throw new ArgumentNullException ("value");

			BaseStream.Write (value, offset, count);
			return this;
		}

		public override IValueWriter Write (ushort value)
		{
			AssertWrite ();

			return Write (BitConverter.GetBytes (value));
		}

		public override IValueWriter Write (uint value)
		{
			AssertWrite ();

			return Write (BitConverter.GetBytes (value));
		}

		public override IValueWriter Write (ulong value)
		{
			AssertWrite ();

			return Write (BitConverter.GetBytes (value));
		}
	}
}

