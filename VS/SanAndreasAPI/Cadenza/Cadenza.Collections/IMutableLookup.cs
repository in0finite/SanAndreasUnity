//
// IMutableLookup.cs
//
// Author:
//   Eric Maupin  <me@ermau.com>
//
// Copyright (c) 2011 Eric Maupin (http://www.ermau.com)
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

namespace Cadenza.Collections
{
	public interface IMutableLookup<TKey, TElement>
		: ILookup<TKey, TElement>
	{
		/// <summary>
		/// Adds <paramref name="element"/> under the specified <paramref name="key"/>.
		/// </summary>
		/// <param name="key">The key to add <paramref name="element"/> under.</param>
		/// <param name="element">The element to add.</param>
		void Add (TKey key, TElement element);

		/// <summary>
		/// Adds <paramref name="elements"/> under the specified <paramref name="key"/>.
		/// </summary>
		/// <param name="key">They key to add <paramref name="elements"/> under.</param>
		/// <param name="elements">The elements to add.</param>
		/// <exception cref="ArgumentNullException"><paramref name="elements"/> is <c>null</c>.</exception>
		void Add (TKey key, IEnumerable<TElement> elements);

		/// <summary>
		/// Removes <paramref name="element"/> from the <paramref name="key"/>.
		/// </summary>
		/// <param name="key">The key that <paramref name="element"/> is located under.</param>
		/// <param name="element">The element to remove from <paramref name="key"/>. </param>
		/// <returns><c>true</c> if <paramref name="key"/> and <paramref name="element"/> existed, <c>false</c> if not.</returns>
		bool Remove (TKey key, TElement element);

		/// <summary>
		/// Removes <paramref name="key"/> from the lookup.
		/// </summary>
		/// <param name="key">They to remove.</param>
		/// <returns><c>true</c> if <paramref name="key"/> existed.</returns>
		bool Remove (TKey key);

		/// <summary>
		/// Clears the lookup.
		/// </summary>
		void Clear ();
	}
}