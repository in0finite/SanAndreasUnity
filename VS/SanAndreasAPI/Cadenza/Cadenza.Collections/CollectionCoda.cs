//
// CollectionCoda.cs
//
// Author:
//   Eric Maupin  <me@ermau.com>
//
// Copyright (c) 2010 Eric Maupin (http://www.ermau.com)
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
using System.Linq;
using System.Collections.Generic;

namespace Cadenza.Collections
{
	public static class CollectionCoda
	{
		public static bool RemoveAll<T> (this ICollection<T> self, Func<T, bool> predicate)
		{
			if (self == null)
				throw new ArgumentNullException ("self");
			if (predicate == null)
				throw new ArgumentNullException ("predicate");

			bool found = false;

			IList<T> list = (self as IList<T>);
			if (list != null)
			{
				for (int i = list.Count - 1; i >= 0; --i)
				{
					if (predicate (list[i]))
					{
						list.RemoveAt (i);
						found = true;
					}
				}
			}
			else
			{
				List<T> items = self.Where (predicate).ToList();
				for (int i = 0; i < items.Count; ++i)
				{
					if (self.Remove (items[i]))
						found = true;
				}
			}

			return found;
		}
	}
}