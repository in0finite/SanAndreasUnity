//
// IValuReader.cs
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

namespace Cadenza.IO {

	[CLSCompliant (false)]
	public interface IValueReader
	{
		IValueReader Read (out bool value);
		IValueReader Read (out byte value);
		IValueReader Read (out char value);
		IValueReader Read (out DateTime value);
		IValueReader Read (out decimal value);
		IValueReader Read (out double value);
		IValueReader Read (out short value);
		IValueReader Read (out int value);
		IValueReader Read (out long value);
		IValueReader Read (out sbyte value);
		IValueReader Read (out float value);
		IValueReader Read (out string value);
		IValueReader Read (out ushort value);
		IValueReader Read (out uint value);
		IValueReader Read (out ulong value);
	}
}

