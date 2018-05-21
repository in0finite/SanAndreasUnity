//
// TextValueReader.cs
//
// Author:
//   Jonathan Pryor  <jpryor@novell.com>
//
// Copyright (c) 2008-2009 Novell, Inc. (http://www.novell.com)
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

namespace Cadenza.IO {

	[CLSCompliant (false)]
	public sealed class TextValueReader : ValueReader<string>
	{
		public TextValueReader (IEnumerable<string> values)
			: base (values)
		{
		}

		protected override bool ToBoolean (string value)
		{
			return bool.Parse (value);
		}

		protected override byte ToByte (string value)
		{
			return byte.Parse (value);
		}

		protected override char ToChar (string value)
		{
			return char.Parse (value);
		}

		protected override DateTime ToDateTime (string value)
		{
			return DateTime.Parse (value);
		}

		protected override decimal ToDecimal (string value)
		{
			return decimal.Parse (value);
		}

		protected override double ToDouble (string value)
		{
			return double.Parse (value);
		}

		protected override short ToInt16 (string value)
		{
			return short.Parse (value);
		}

		protected override int ToInt32 (string value)
		{
			return int.Parse (value);
		}

		protected override long ToInt64 (string value)
		{
			return long.Parse (value);
		}

		protected override sbyte ToSByte (string value)
		{
			return sbyte.Parse (value);
		}

		protected override float ToSingle (string value)
		{
			return float.Parse (value);
		}

		protected override string ToString (string value)
		{
			return value;
		}

		protected override ushort ToUInt16 (string value)
		{
			return ushort.Parse (value);
		}

		protected override uint ToUInt32 (string value)
		{
			return uint.Parse (value);
		}

		protected override ulong ToUInt64 (string value)
		{
			return ulong.Parse (value);
		}

		//
		// Extensions
		//
		public TextValueReader Read (IFormatProvider provider, out byte value)
		{
			value = byte.Parse (GetNextItem (), provider);
			return this;
		}

		public TextValueReader Read (IFormatProvider provider, out DateTime value)
		{
			value = DateTime.Parse (GetNextItem (), provider);
			return this;
		}

		public TextValueReader Read (IFormatProvider provider, out decimal value)
		{
			value = decimal.Parse (GetNextItem (), provider);
			return this;
		}

		public TextValueReader Read (IFormatProvider provider, out double value)
		{
			value = double.Parse (GetNextItem (), provider);
			return this;
		}

		public TextValueReader Read (IFormatProvider provider, out short value)
		{
			value = short.Parse (GetNextItem (), provider);
			return this;
		}

		public TextValueReader Read (IFormatProvider provider, out int value)
		{
			value = int.Parse (GetNextItem (), provider);
			return this;
		}

		public TextValueReader Read (IFormatProvider provider, out long value)
		{
			value = long.Parse (GetNextItem (), provider);
			return this;
		}

		public TextValueReader Read (IFormatProvider provider, out sbyte value)
		{
			value = sbyte.Parse (GetNextItem (), provider);
			return this;
		}

		public TextValueReader Read (IFormatProvider provider, out float value)
		{
			value = float.Parse (GetNextItem (), provider);
			return this;
		}

		public TextValueReader Read (IFormatProvider provider, out ushort value)
		{
			value = ushort.Parse (GetNextItem (), provider);
			return this;
		}

		public TextValueReader Read (IFormatProvider provider, out uint value)
		{
			value = uint.Parse (GetNextItem (), provider);
			return this;
		}

		public TextValueReader Read (IFormatProvider provider, out ulong value)
		{
			value = ulong.Parse (GetNextItem (), provider);
			return this;
		}
	}
}

