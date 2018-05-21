//
// DictionaryCoda.cs
//
// Authors:
//   Jonathan Pryor  <jpryor@novell.com>
//   Chris Howie <cdhowie@gmail.com>
//   Eric Maupin <me@ermau.com>
//
// Copyright (c) 2010 Chris Howie
// Copyright (c) 2010 Novell, Inc. (http://www.novell.com)
// Copyright (c) 2012 Eric Maupin (http://ermau.com)
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

using Cadenza;

namespace Cadenza.Collections {

	public static class DictionaryCoda {

		public static bool TryRemove<TKey, TValue> (this IDictionary<TKey, TValue> self, TKey key, out TValue value)
		{
			Check.Self (self);

			return self.TryGetValue (key, out value) && self.Remove (key);
		}

		public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key)
		{
			Check.Self (self);

			return GetValueOrDefault (self, key, default (TValue));
		}

		public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key, TValue defaultValue)
		{
			Check.Self (self);

			TValue value;
			return self.TryGetValue(key, out value) ? value : defaultValue;
		}

		public static bool SequenceEqual<TKey, TValue>(this IDictionary<TKey, TValue> self, IDictionary<TKey, TValue> other)
		{
			return SequenceEqual (self, other, null);
		}

		public static bool SequenceEqual<TKey, TValue>(this IDictionary<TKey, TValue> self, IDictionary<TKey, TValue> other, IEqualityComparer<TValue> comparer)
		{
			Check.Self (self);
			if (other == null)
				throw new ArgumentNullException ("null");
			comparer = comparer ?? EqualityComparer<TValue>.Default;

			if (self.Count != other.Count)
				return false;

			foreach (var k in self.Keys) {
				if (!other.ContainsKey (k))
					return false;
				if (!comparer.Equals (self [k], other [k]))
					return false;
			}

			return true;
		}

		public static TValue UpdateValue<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key, Func<TValue, TValue> valueSelector)
		{
			Check.Self (self);
			Check.ValueSelector (valueSelector);

			TValue value;
			if (!self.TryGetValue (key, out value))
				value = default (TValue);
			value = valueSelector (value);
			self [key] = value;
			return value;
		}

		public static TValue GetValueOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key)
			where TValue : new()
		{
			Check.Self(self);

			TValue value;

			if (!self.TryGetValue (key, out value)) {
				self.Add (key, value = new TValue ());
			}
			return value;
		}

		public static TValue GetValueOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key, Func<TValue> creator)
		{
			Check.Self(self);
			Check.Creator (creator);

			TValue value;

			if (!self.TryGetValue (key, out value)) {
				self.Add (key, value = creator ());
			}
			return value;
		}
	}
}
