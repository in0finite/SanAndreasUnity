//
// BidirectionalDictionary.cs
//
// Author:
//   Chris Chilvers <chilversc@googlemail.com>
//
// Copyright (c) 2009 Chris Chilvers
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

namespace Cadenza.Collections
{
	public class BidirectionalDictionary<TKey, TValue> : IDictionary<TKey, TValue>
	{
		private readonly IEqualityComparer<TKey> keyComparer;
		private readonly IEqualityComparer<TValue> valueComparer;
		private readonly Dictionary<TKey, TValue> keysToValues;
		private readonly Dictionary<TValue, TKey> valuesToKeys;
		private readonly BidirectionalDictionary<TValue, TKey> inverse;


		public BidirectionalDictionary () : this (10, null, null) {}

		public BidirectionalDictionary (int capacity) : this (capacity, null, null) {}

		public BidirectionalDictionary (IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
			: this (10, keyComparer, valueComparer)
		{
		}

		public BidirectionalDictionary (int capacity, IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
		{
			if (capacity < 0)
				throw new ArgumentOutOfRangeException ("capacity", capacity, "capacity cannot be less than 0");

			this.keyComparer = keyComparer ?? EqualityComparer<TKey>.Default;
			this.valueComparer = valueComparer ?? EqualityComparer<TValue>.Default;

			keysToValues = new Dictionary<TKey, TValue> (capacity, this.keyComparer);
			valuesToKeys = new Dictionary<TValue, TKey> (capacity, this.valueComparer);

			inverse = new BidirectionalDictionary<TValue, TKey> (this);
		}

		private BidirectionalDictionary (BidirectionalDictionary<TValue, TKey> inverse)
		{
			this.inverse = inverse;
			keyComparer = inverse.valueComparer;
			valueComparer = inverse.keyComparer;
			valuesToKeys = inverse.keysToValues;
			keysToValues = inverse.valuesToKeys;
		}


		public BidirectionalDictionary<TValue, TKey> Inverse {
			get { return inverse; }
		}


		public ICollection<TKey> Keys {
			get { return keysToValues.Keys; }
		}

		public ICollection<TValue> Values {
			get { return keysToValues.Values; }
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator ()
		{
			return keysToValues.GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		void ICollection<KeyValuePair<TKey, TValue>>.CopyTo (KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			((ICollection<KeyValuePair<TKey, TValue>>) keysToValues).CopyTo (array, arrayIndex);
		}


		public bool ContainsKey (TKey key)
		{
			if (key == null)
				throw new ArgumentNullException ("key");
			return keysToValues.ContainsKey (key);
		}

		public bool ContainsValue (TValue value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			return valuesToKeys.ContainsKey (value);
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.Contains (KeyValuePair<TKey, TValue> item)
		{
			return ((ICollection<KeyValuePair<TKey, TValue>>) keysToValues).Contains (item);
		}

		public bool TryGetKey (TValue value, out TKey key)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			return valuesToKeys.TryGetValue (value, out key);
		}

		public bool TryGetValue (TKey key, out TValue value)
		{
			if (key == null)
				throw new ArgumentNullException ("key");
			return keysToValues.TryGetValue (key, out value);
		}

		public TValue this[TKey key] {
			get { return keysToValues [key]; }
			set {
				if (key == null)
					throw new ArgumentNullException ("key");
				if (value == null)
					throw new ArgumentNullException ("value");

				//foo[5] = "bar"; foo[6] = "bar"; should not be valid
				//as it would have to remove foo[5], which is unexpected.
				if (ValueBelongsToOtherKey (key, value))
					throw new ArgumentException ("Value already exists", "value");

				TValue oldValue;
				if (keysToValues.TryGetValue (key, out oldValue)) {
					// Use the current key for this value to stay consistent
					// with Dictionary<TKey, TValue> which does not alter
					// the key if it exists.
					TKey oldKey = valuesToKeys [oldValue];

					keysToValues [oldKey] = value;
					valuesToKeys.Remove (oldValue);
					valuesToKeys [value] = oldKey;
				} else {
					keysToValues [key] = value;
					valuesToKeys [value] = key;
				}
			}
		}

		public int Count {
			get { return keysToValues.Count; }
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly {
			get { return false; }
		}


		public void Add (TKey key, TValue value)
		{
			if (key == null)
				throw new ArgumentNullException ("key");
			if (value == null)
				throw new ArgumentNullException ("value");

			if (keysToValues.ContainsKey (key))
				throw new ArgumentException ("Key already exists", "key");
			if (valuesToKeys.ContainsKey (value))
				throw new ArgumentException ("Value already exists", "value");

			keysToValues.Add (key, value);
			valuesToKeys.Add (value, key);
		}

		public void Replace (TKey key, TValue value)
		{
			if (key == null)
				throw new ArgumentNullException ("key");
			if (value == null)
				throw new ArgumentNullException ("value");

			// replaces a key value pair, if the key or value already exists those mappings will be replaced.
			// e.g. you have; a -> b, b -> a; c -> d, d -> c
			// you add the mapping; a -> d, d -> a
			// this will remove both of the original mappings
			Remove (key);
			inverse.Remove (value);
			Add (key, value);
		}

		void ICollection<KeyValuePair<TKey, TValue>>.Add (KeyValuePair<TKey, TValue> item)
		{
			Add (item.Key, item.Value);
		}

		public bool Remove (TKey key)
		{
			if (key == null)
				throw new ArgumentNullException ("key");

			TValue value;
			if (keysToValues.TryGetValue (key, out value)) {
				keysToValues.Remove (key);
				valuesToKeys.Remove (value);
				return true;
			}
			else {
				return false;
			}
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.Remove (KeyValuePair<TKey, TValue> item)
		{
			bool removed = ((ICollection<KeyValuePair<TKey, TValue>>) keysToValues).Remove (item);
			if (removed)
				valuesToKeys.Remove (item.Value);
			return removed;
		}

		public void Clear ()
		{
			keysToValues.Clear ();
			valuesToKeys.Clear ();
		}


		private bool ValueBelongsToOtherKey (TKey key, TValue value)
		{
			TKey otherKey;
			if (valuesToKeys.TryGetValue (value, out otherKey))
				// if the keys are not equal the value belongs to another key
				return !keyComparer.Equals (key, otherKey);
			else
				// value doesn't exist in map, thus it cannot belong to another key
				return false;
		}
	}
}
