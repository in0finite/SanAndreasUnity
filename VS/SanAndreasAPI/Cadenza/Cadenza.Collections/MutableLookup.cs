//
// MutableLookup.cs
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
	/// <summary>
	/// A mutable lookup implementing <see cref="ILookup{TKey,TElement}"/>
	/// </summary>
	/// <typeparam name="TKey">The lookup key.</typeparam>
	/// <typeparam name="TElement">The elements under each <typeparamref name="TKey"/>.</typeparam>
	public class MutableLookup<TKey, TElement>
		: IMutableLookup<TKey, TElement>
	{
		public MutableLookup ()
			: this (EqualityComparer<TKey>.Default)
		{
		}

		public MutableLookup (IEqualityComparer<TKey> comparer)
		{
			if (comparer == null)
				throw new ArgumentNullException ("comparer");

			this.groupings = new Dictionary<TKey, MutableLookupGrouping> (comparer);
			if (!typeof(TKey).IsValueType)
				this.nullGrouping = new MutableLookupGrouping (default(TKey));
		}

		public MutableLookup (ILookup<TKey, TElement> lookup)
		{
			if (lookup == null)
				throw new ArgumentNullException ("lookup");

			this.groupings = new Dictionary<TKey, MutableLookupGrouping> (lookup.Count);

			if (!typeof(TKey).IsValueType)
				this.nullGrouping = new MutableLookupGrouping (default(TKey), lookup[default(TKey)]);
			
			foreach (var grouping in lookup) {
				if (grouping.Key == null)
					continue;

				this.groupings.Add (grouping.Key, new MutableLookupGrouping (grouping.Key, grouping));
			}
		}

		/// <summary>
		/// Adds <paramref name="element"/> under the specified <paramref name="key"/>. <paramref name="key"/> does not need to exist.
		/// </summary>
		/// <param name="key">The key to add <paramref name="element"/> under.</param>
		/// <param name="element">The element to add.</param>
		public void Add (TKey key, TElement element)
		{
			MutableLookupGrouping grouping;
			if (key == null)
				grouping = nullGrouping;
			else if (!this.groupings.TryGetValue (key, out grouping))
			{
				grouping = new MutableLookupGrouping (key);
				this.groupings.Add (key, grouping);
			}
			
			grouping.Add (element);
		}

		public void Add (TKey key, IEnumerable<TElement> elements)
		{
			if (elements == null)
				throw new ArgumentNullException ("elements");

			MutableLookupGrouping grouping;
			if (key == null)
				grouping = nullGrouping;
			else if (!this.groupings.TryGetValue (key, out grouping))
			{
				grouping = new MutableLookupGrouping (key);
				this.groupings.Add (key, grouping);
			}

			grouping.AddRange (elements);
		}

		/// <summary>
		/// Removes <paramref name="element"/> from the <paramref name="key"/>.
		/// </summary>
		/// <param name="key">The key that <paramref name="element"/> is located under.</param>
		/// <param name="element">The element to remove from <paramref name="key"/>. </param>
		/// <returns><c>true</c> if <paramref name="key"/> and <paramref name="element"/> existed, <c>false</c> if not.</returns>
		public bool Remove (TKey key, TElement element)
		{
			if (key == null)
				return this.nullGrouping.Remove (element);
			
			if (!this.groupings.ContainsKey (key))
				return false;
			
			if (this.groupings[key].Remove (element)) {
				if (this.groupings[key].Count == 0)
					this.groupings.Remove (key);
				
				return true;
			}
			
			return false;
		}

		/// <summary>
		/// Removes <paramref name="key"/> from the lookup.
		/// </summary>
		/// <param name="key">They to remove.</param>
		/// <returns><c>true</c> if <paramref name="key"/> existed.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
		public bool Remove (TKey key)
		{
			if (key == null) {
				bool removed = (this.nullGrouping.Count > 0);
				this.nullGrouping.Clear ();
				return removed;
			}
			
			return this.groupings.Remove (key);
		}

		public void Clear ()
		{
			if (nullGrouping != null)
				this.nullGrouping.Clear ();

			this.groupings.Clear();
		}

		public bool TryGetValues (TKey key, out IEnumerable<TElement> values)
		{
			values = null;

			if (key == null)
			{
				if (nullGrouping.Count != 0)
				{
					values = nullGrouping;
					return true;
				}
				else
					return false;
			}

			MutableLookupGrouping grouping;
			if (!this.groupings.TryGetValue (key, out grouping))
				return false;

			values = grouping;
			return true;
		}

		#region ILookup Members
		/// <summary>
		/// Gets the number of groupings.
		/// </summary>
		public int Count {
			get { return this.groupings.Count + ((this.nullGrouping != null && this.nullGrouping.Count > 0) ? 1 : 0); }
		}

		/// <summary>
		/// Gets the elements for <paramref name="key"/>.
		/// </summary>
		/// <param name="key">The key to get the elements for.</param>
		/// <returns>The elements under <paramref name="key"/>.</returns>
		public IEnumerable<TElement> this[TKey key] {
			get {
				if (key == null)
					return this.nullGrouping;
				
				MutableLookupGrouping grouping;
				if (this.groupings.TryGetValue (key, out grouping))
					return grouping;
				
				return new TElement[0];
			}
		}

		/// <summary>
		/// Gets whether or not there's a grouping for <paramref name="key"/>.
		/// </summary>
		/// <param name="key">The key to check for.</param>
		/// <returns><c>true</c> if <paramref name="key"/> is present.</returns>
		public bool Contains (TKey key)
		{
			if (key == null)
				return (this.nullGrouping.Count > 0);
			
			return this.groupings.ContainsKey (key);
		}

		public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator ()
		{
			foreach (var g in this.groupings.Values)
				yield return g;
			
			if (this.nullGrouping != null && this.nullGrouping.Count > 0)
				yield return this.nullGrouping;
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
		{
			return this.GetEnumerator ();
		}
		#endregion

		private readonly Dictionary<TKey, MutableLookupGrouping> groupings;
		private readonly MutableLookupGrouping nullGrouping;

		private class MutableLookupGrouping : List<TElement>, IGrouping<TKey, TElement>
		{
			public MutableLookupGrouping (TKey key)
			{
				this.Key = key;
			}

			public MutableLookupGrouping (TKey key, IEnumerable<TElement> collection) : base(collection)
			{
				this.Key = key;
			}

			#region IGrouping<TKey,TElement> Members

			public TKey Key { get; private set; }

			#endregion
		}
	}
}
