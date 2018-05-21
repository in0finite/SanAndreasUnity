//
// EnumerableValueReader.cs
//
// Author:
//   Jonathan Pryor  <jpryor@novell.com>
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Cadenza.IO {

	[CLSCompliant (false)]
	public sealed class EnumerableValueReader<T> : ValueReader<T>
	{
		public EnumerableValueReader (IEnumerable<T> values)
			: base (values)
		{
		}

		private R Convert<R> (T value)
		{
			return Either.TryConvert<T, R> (value)
				.Fold<R> (v => v, v => {throw v;});
		}

		protected override bool ToBoolean (T value)
		{
			return Convert<bool> (value);
		}

		protected override byte ToByte (T value)
		{
			return Convert<byte> (value);
		}

		protected override char ToChar (T value)
		{
			return Convert<char> (value);
		}

		protected override DateTime ToDateTime (T value)
		{
			return Convert<DateTime> (value);
		}

		protected override decimal ToDecimal (T value)
		{
			return Convert<decimal> (value);
		}

		protected override double ToDouble (T value)
		{
			return Convert<double> (value);
		}

		protected override short ToInt16 (T value)
		{
			return Convert<short> (value);
		}

		protected override int ToInt32 (T value)
		{
			return Convert<int> (value);
		}

		protected override long ToInt64 (T value)
		{
			return Convert<long> (value);
		}

		protected override sbyte ToSByte (T value)
		{
			return Convert<sbyte> (value);
		}

		protected override float ToSingle (T value)
		{
			return Convert<float> (value);
		}

		protected override string ToString (T value)
		{
			return Convert<string> (value);
		}

		[CLSCompliant (false)]
		protected override ushort ToUInt16 (T value)
		{
			return Convert<ushort> (value);
		}

		[CLSCompliant (false)]
		protected override uint ToUInt32 (T value)
		{
			return Convert<uint> (value);
		}

		[CLSCompliant (false)]
		protected override ulong ToUInt64 (T value)
		{
			return Convert<ulong> (value);
		}
	}
}

