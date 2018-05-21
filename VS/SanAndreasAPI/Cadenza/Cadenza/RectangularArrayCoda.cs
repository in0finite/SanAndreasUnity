//
// RectangularArrayCoda.cs
//
// Author:
//   Jonathan Pryor <jpryor@novell.com>
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

namespace Cadenza
{
	public static class RectangularArrayCoda
	{
		public static IEnumerable<IEnumerable<TSource>> Rows<TSource> (this TSource[,] self)
		{
			Check.Self (self);

			int rows = self.GetLength (0);
			int cols = self.GetLength (1);

			return CreateRowsIterator (self, rows, cols);
		}

		private static IEnumerable<IEnumerable<TSource>> CreateRowsIterator<TSource> (TSource[,] self, int rows, int cols)
		{
			for (int i = 0; i < rows; ++i)
				yield return CreateColumnIterator (self, i, cols);
		}

		private static IEnumerable<TSource> CreateColumnIterator<TSource> (TSource[,] self, int i, int cols)
		{
			for (int j = 0; j < cols; ++j)
				yield return self [i, j];
		}
	}
}
