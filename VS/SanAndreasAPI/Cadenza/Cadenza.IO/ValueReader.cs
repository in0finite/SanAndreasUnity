//
// ValueReader.cs
//
// Authors:
//   Jonathan Pryor  <jpryor@novell.com>
//
// Copyright (c) 2009 Novell, Inc. (http://www.novell.com)
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
using System.Collections.Generic;

namespace Cadenza.IO
{
	[CLSCompliant (false)]
	public abstract class ValueReader<T> : IValueReader, IDisposable
	{
		IEnumerator<T> items;

		public ValueReader (IEnumerable<T> values)
		{
			if (values == null)
				throw new ArgumentNullException ("values");
			this.items = values.GetEnumerator ();
		}

		protected abstract bool ToBoolean (T value);
		protected abstract byte ToByte (T value);
		protected abstract char ToChar (T value);
		protected abstract DateTime ToDateTime (T value);
		protected abstract decimal ToDecimal (T value);
		protected abstract double ToDouble (T value);
		protected abstract short ToInt16 (T value);
		protected abstract int ToInt32 (T value);
		protected abstract long ToInt64 (T value);
		protected abstract float ToSingle (T value);
		protected abstract string ToString (T value);

		protected virtual sbyte ToSByte (T value)
		{
			byte b = ToByte (value);
			return (sbyte) b;
		}

		protected virtual ushort ToUInt16 (T value)
		{
			return (ushort) ToInt16 (value);
		}

		protected virtual uint ToUInt32 (T value)
		{
			return (uint) ToInt32 (value);
		}

		protected virtual ulong ToUInt64 (T value)
		{
			return (ulong) ToInt64 (value);
		}

		internal protected T GetNextItem ()
		{
			if (!items.MoveNext ())
				throw new InvalidOperationException ("no more elements");

			return items.Current;
		}

		public void Dispose ()
		{
			Dispose (true);
			items.Dispose ();
		}

		protected virtual void Dispose (bool disposing)
		{
		}

		public IValueReader Read (out bool value)
		{
			value = ToBoolean (GetNextItem ());
			return this;
		}

		public IValueReader Read (out byte value)
		{
			value = ToByte (GetNextItem ());
			return this;
		}

		public IValueReader Read (out char value)
		{
			value = ToChar (GetNextItem ());
			return this;
		}

		public IValueReader Read (out DateTime value)
		{
			value = ToDateTime (GetNextItem ());
			return this;
		}

		public IValueReader Read (out decimal value)
		{
			value = ToDecimal (GetNextItem ());
			return this;
		}

		public IValueReader Read (out double value)
		{
			value = ToDouble (GetNextItem ());
			return this;
		}

		public IValueReader Read (out short value)
		{
			value = ToInt16 (GetNextItem ());
			return this;
		}

		public IValueReader Read (out int value)
		{
			value = ToInt32 (GetNextItem ());
			return this;
		}

		public IValueReader Read (out long value)
		{
			value = ToInt64 (GetNextItem ());
			return this;
		}

		public IValueReader Read (out sbyte value)
		{
			value = ToSByte (GetNextItem ());
			return this;
		}

		public IValueReader Read (out float value)
		{
			value = ToSingle (GetNextItem ());
			return this;
		}

		public IValueReader Read (out string value)
		{
			value = ToString (GetNextItem ());
			return this;
		}

		public IValueReader Read (out ushort value)
		{
			value = ToUInt16 (GetNextItem ());
			return this;
		}

		public IValueReader Read (out uint value)
		{
			value = ToUInt32 (GetNextItem ());
			return this;
		}

		public IValueReader Read (out ulong value)
		{
			value = ToUInt64 (GetNextItem ());
			return this;
		}
	}

	[CLSCompliant (false)]
	public static class ValueReaderCoda
	{
		public static ValueReader<TSource> Read<TSource, TValue> (this ValueReader<TSource> self, out TValue value)
		{
			Check.Self (self);

			value = Either.TryConvert<TSource, TValue> (self.GetNextItem ())
				.Fold<TValue> (v => v, v => {throw v;});

			return self;
		}
	}
}
