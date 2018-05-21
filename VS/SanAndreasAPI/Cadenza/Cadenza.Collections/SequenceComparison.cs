//
// SequenceComparison.cs
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
	public class SequenceComparison<T>
	{
		public SequenceComparison (IEnumerable<T> original, IEnumerable<T> update)
			: this (original, update, null)
		{
		}

		public SequenceComparison (IEnumerable<T> original, IEnumerable<T> update, IEqualityComparer<T> comparer)
		{
			if (original == null)
				throw new ArgumentNullException ("original");
			if (update == null)
				throw new ArgumentNullException ("update");

			Comparer = comparer ?? EqualityComparer<T>.Default;
			Compare (original, update);
		}

		public IEqualityComparer<T> Comparer
		{
			get;
			private set;
		}

		public IEnumerable<T> Added
		{
			get;
			private set;
		}

		public IEnumerable<T> Removed
		{
			get;
			private set;
		}

		public IEnumerable<T> Stayed
		{
			get;
			private set;
		}

		private void Compare (IEnumerable<T> original, IEnumerable<T> update)
		{
			HashSet<T> stayed = new HashSet<T>();
			HashSet<T> removed = new HashSet<T>();

			HashSet<T> items = new HashSet<T> (update, Comparer);

			foreach (T item in original)
			{
				if (items.Remove (item))
					stayed.Add (item);
				else
					removed.Add (item);
			}

			this.Stayed = stayed;
			this.Removed = removed;
			this.Added = items;
		}
	}
}