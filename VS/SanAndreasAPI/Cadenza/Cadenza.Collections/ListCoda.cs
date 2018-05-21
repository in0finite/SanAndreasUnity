//
// ListCoda.cs: IList<T> extension methods.
//
// Author:
//   Chris Chilvers <chilversc@googlemail.com>
//
// Copyright (c) 2010 Chris Chilvers
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
using System.Linq;
using System.Text;

namespace Cadenza.Collections
{
	public static class ListCoda
	{
		public static int BinarySearch<TSource, TValue> (this IList<TSource> self, TValue item, Func<TSource, TValue, int> comparer)
		{
			Check.Self (self);
			Check.Comparer (comparer);

			int low = 0;
			int high = self.Count - 1;

			while (low <= high) {
				int probe = low + ((high - low) / 2);

				int result = comparer (self [probe], item);

				if (result < 0) //self [probe] < value
					low = probe + 1;
				else if (result > 0) //self [probe] > value
					high = probe - 1;
				else
					return probe;
			}

			return ~low;
		}
	}
}
