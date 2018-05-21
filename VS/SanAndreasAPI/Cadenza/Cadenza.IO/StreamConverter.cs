//
// StreamConverter.cs
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

	[CLSCompliant (false)]
	public abstract class StreamConverter : IValueReader, IValueWriter, IDisposable
	{
		protected StreamConverter ()
		{
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
		}

		public abstract IValueReader Read (out bool value);
		public abstract IValueReader Read (out byte value);
		public abstract IValueReader Read (byte[] value, int offset, int count);
		public abstract IValueReader Read (out char value);
		public abstract IValueReader Read (out DateTime value);
		public abstract IValueReader Read (out decimal value);
		public abstract IValueReader Read (out double value);
		public abstract IValueReader Read (out short value);
		public abstract IValueReader Read (out int value);
		public abstract IValueReader Read (out long value);
		public abstract IValueReader Read (out float value);
		public abstract IValueReader Read (out string value);
		public abstract IValueReader Read (int size, Encoding encoding, out string value);

		public virtual IValueReader Read (out sbyte value)
		{
			byte v;
			Read (out v);
			value = (sbyte) v;
			return this;
		}

		public virtual IValueReader Read (out ushort value)
		{
			short v;
			Read (out v);
			value = (ushort) v;
			return this;
		}

		public virtual IValueReader Read (out uint value)
		{
			int v;
			Read (out v);
			value = (uint) v;
			return this;
		}

		public virtual IValueReader Read (out ulong value)
		{
			long v;
			Read (out v);
			value = (ulong) v;
			return this;
		}

		public StreamConverter Read (byte[] value)
		{
			Check.Value (value);

			Read (value, 0, value.Length);
			return this;
		}

		public abstract IValueWriter Write (bool value);
		public abstract IValueWriter Write (byte value);
		public abstract IValueWriter Write (char value);
		public abstract IValueWriter Write (DateTime value);
		public abstract IValueWriter Write (decimal value);
		public abstract IValueWriter Write (double value);
		public abstract IValueWriter Write (short value);
		public abstract IValueWriter Write (int value);
		public abstract IValueWriter Write (long value);
		public abstract IValueWriter Write (float value);
		public abstract IValueWriter Write (string value);

		public abstract StreamConverter Write (byte[] value, int offset, int count);

		public virtual IValueWriter Write (sbyte value)
		{
			return Write ((byte) value);
		}

		public virtual IValueWriter Write (ushort value)
		{
			return Write ((short) value);
		}

		public virtual IValueWriter Write (uint value)
		{
			return Write ((int) value);
		}

		public virtual IValueWriter Write (ulong value)
		{
			return Write ((long) value);
		}

		public StreamConverter Write (byte[] value)
		{
			Check.Value (value);

			return Write (value, 0, value.Length);
		}
	}

	[CLSCompliant (false)]
	public static class StreamConverterCoda
	{
		public static StreamConverter Read<TValue> (this StreamConverter self, out TValue value)
		{
			Check.Self (self);

			byte[] data = new byte [Marshal.SizeOf (typeof (TValue))];
			self.Read (data);
			GCHandle handle = GCHandle.Alloc (data, GCHandleType.Pinned);

			try { 
				value = (TValue) Marshal.PtrToStructure (handle.AddrOfPinnedObject (), typeof (TValue)); 
			} finally {
				handle.Free();
			}

			return self;
		}

		public static StreamConverter Write<TValue> (this StreamConverter self, TValue value)
		{
			Check.Self (self);

			byte[] data = new byte [Marshal.SizeOf (typeof (TValue))];

			GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			try {
				Marshal.StructureToPtr (value, handle.AddrOfPinnedObject(), false);
			} finally {
				handle.Free();
			}

			return self.Write (data);
		}
	}
}

