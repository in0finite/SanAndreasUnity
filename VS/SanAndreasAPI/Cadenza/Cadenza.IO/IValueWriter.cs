//
// IValueWriter.cs
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
	public interface IValueWriter
	{
		IValueWriter Write (bool value);
		IValueWriter Write (byte value);
		IValueWriter Write (char value);
		IValueWriter Write (DateTime value);
		IValueWriter Write (decimal value);
		IValueWriter Write (double value);
		IValueWriter Write (short value);
		IValueWriter Write (int value);
		IValueWriter Write (long value);
		IValueWriter Write (sbyte value);
		IValueWriter Write (float value);
		IValueWriter Write (string value);
		IValueWriter Write (ushort value);
		IValueWriter Write (uint value);
		IValueWriter Write (ulong value);
	}
}

