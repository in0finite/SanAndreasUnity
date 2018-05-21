//
// BidirectionalLookup.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Cadenza.Collections
{
	/// <summary>
	/// A <see cref="IMutableLookup{TKey,TElement}"/> for many-to-many
	/// </summary>
	/// <typeparam name="TKey">The key type for the default lookup direction</typeparam>
	/// <typeparam name="TElement">The element type for the default lookup direction</typeparam>
	public class BidirectionalLookup<TKey, TElement>
		: IMutableLookup<TKey, TElement>
	{
		/// <summary>
		/// Creates a new instance of <see cref="BidirectionalLookup{TKey,TElement}"/> with default equality comparisons for <typeparamref name="TKey"/> and <typeparamref name="TElement"/>.
		/// </summary>
		public BidirectionalLookup ()
			: this (EqualityComparer<TKey>.Default, EqualityComparer<TElement>.Default)
		{
		}

		/// <summary>
		/// Creates a new instance of <see cref="BidirectionalLookup{TKey,TElement}"/> with supplied <paramref name="keyComparer"/> and <paramref name="valueComparer"/>.
		/// </summary>
		/// <param name="keyComparer">The <see cref="IEqualityComparer{T}"/> to compare keys in this direction.</param>
		/// <param name="valueComparer">The <see cref="IEqualityComparer{T}"/> to compare elements in this direction.</param>
		/// <exception cref="ArgumentNullException">
		/// <para><paramref name="keyComparer"/> is <c>null</c></para>
		/// <para>-- or --</para>
		/// <para><paramref name="valueComparer"/> is <c>null</c></para>
		/// </exception>
		public BidirectionalLookup (IEqualityComparer<TKey> keyComparer, IEqualityComparer<TElement> valueComparer)
		{
			if (keyComparer == null)
				throw new ArgumentNullException ("keyComparer");
			if (valueComparer == null)
				throw new ArgumentNullException ("valueComparer");

			this.keyComparer = keyComparer;
			this.valueComparer = valueComparer;
			this.keysToElements = new MutableLookup<TKey, TElement> (this.keyComparer);
			this.elementsToKeys = new MutableLookup<TElement, TKey> (this.valueComparer);
			this.inverse = new BidirectionalLookup<TElement, TKey> (this);
		}

		private BidirectionalLookup (BidirectionalLookup<TElement, TKey> inverse)
		{
			this.inverse = inverse;
			this.keysToElements = inverse.elementsToKeys;
			this.elementsToKeys = inverse.keysToElements;
		}

		/// <summary>
		/// Gets the inverse lookup, providing <see cref="IMutableLookup{TElement,TKey}"/>.
		/// </summary>
		public BidirectionalLookup<TElement, TKey> Inverse
		{
			get { return this.inverse; }
		}

		public bool Contains (TKey key)
		{
			return this.keysToElements.Contains (key);
		}

		public int Count
		{
			get { return this.keysToElements.Count; }
		}

		public IEnumerable<TElement> this [TKey key]
		{
			get { return this.keysToElements[key]; }
		}

		public void Add (TKey key, TElement element)
		{
			this.keysToElements.Add (key, element);
			this.elementsToKeys.Add (element, key);
		}

		public void Add (TKey key, IEnumerable<TElement> elements)
		{
			if (elements == null)
				throw new ArgumentNullException ("elements");

			elements = elements.ToArray();

			this.keysToElements.Add (key, elements);
			foreach (TElement element in elements)
				this.elementsToKeys.Add (element, key);
		}

		public bool Remove (TKey key, TElement element)
		{
			this.keysToElements.Remove (key, element);
			return this.elementsToKeys.Remove (element, key);
		}

		public bool Remove (TKey key)
		{
			foreach (TElement element in this.keysToElements[key])
				this.elementsToKeys.Remove (element, key);

			return this.keysToElements.Remove (key);
		}

		public void Clear()
		{
			this.keysToElements.Clear();
			this.elementsToKeys.Clear();
		}

		public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator()
		{
			return this.keysToElements.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private readonly IEqualityComparer<TKey> keyComparer;
		private readonly IEqualityComparer<TElement> valueComparer;
		private readonly MutableLookup<TKey, TElement> keysToElements;
		private readonly MutableLookup<TElement, TKey> elementsToKeys;
		private readonly BidirectionalLookup<TElement, TKey> inverse;
	}
}