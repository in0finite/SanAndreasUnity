//
// CachedSequence.cs: Thread-Safe, Immutable, Singly Linked List.
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Cadenza.Collections {

	public class CachedSequence<T> : IEnumerable<T>
	{
		private T head;
		private Either<CachedSequence<T>, IEnumerator<T>> rest;

		public CachedSequence (T head)
		{
			this.head = head;
		}

		public CachedSequence (T head, CachedSequence<T> tail)
		{
			this.head = head;
			this.rest = tail == null ? null : Either (tail);
		}

		public CachedSequence (IEnumerable<T> collection)
			: this (GetItems (collection))
		{
		}

		static IEnumerator<T> GetItems (IEnumerable<T> collection)
		{
			if (collection == null)
				throw new ArgumentNullException ("collection");

			var iter = collection.GetEnumerator ();
			if (!iter.MoveNext ()) {
				iter.Dispose ();
				throw new InvalidOperationException ("no elements");
			}

			return iter;
		}

		private CachedSequence (IEnumerator<T> iter)
		{
			this.head = iter.Current;
			if (iter.MoveNext ())
				this.rest = Either (iter);
			else
				iter.Dispose ();
		}

		Either<CachedSequence<T>, IEnumerator<T>> Either (CachedSequence<T> tail)
		{
			return Either<CachedSequence<T>, IEnumerator<T>>.A (tail);
		}

		Either<CachedSequence<T>, IEnumerator<T>> Either (IEnumerator<T> iter)
		{
			return Either<CachedSequence<T>, IEnumerator<T>>.B (iter);
		}

		public T Head {
			get {return head;}
		}

		public CachedSequence<T> Tail {
			get {
				if (rest == null)
					return null;
				return rest.Fold (
						tail => tail,
						iter => {
							CachedSequence<T> tail = null;
							lock (iter) {
								this.rest = null;
								tail = new CachedSequence<T> (iter);
								this.rest = Either (tail);
							}
							return tail;
						}
				);
			}
		}

		public CachedSequence<T> Append (T value)
		{
			CachedSequence<T> start, c;
			start = c = new CachedSequence<T> (Head);

			CachedSequence<T> end = Tail;
			while (end != null) {
				var n  = new CachedSequence<T> (end.Head);
				c.rest = Either (n);
				c      = n;
				end    = end.Tail;
			}
			c.rest = Either (new CachedSequence<T> (value));

			return start;
		}

		public CachedSequence<T> Prepend (T value)
		{
			return new CachedSequence<T> (value, this);
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		public IEnumerator<T> GetEnumerator ()
		{
			var c = Tail;
			try {
				yield return Head;
				c = Tail;
				while (c != null) {
					yield return c.Head;
					c = c.Tail;
				}
			}
			finally {
				// If the enumerator is disposed, ensure that the sequence iterator is disposed.
				// However, since CSC-generated iterator methods have IEnumerator<T>.MoveNext()
				// return 'true' even after IDisposable.Dispose() has been called on the
				// iterator, we need to set the last .rest field to null.  Thus, find it:
				while (c != null && c.rest != null) {
					// disposeTail should only be invoked on the last item in the
					// sequence, as rest will hold a CachedSequence<T> for prior items.
					Func<IEnumerator<T>, bool> disposeTail = i => {
						lock (i) {
							if (i != null) {
								i.Dispose ();
							}
						}
						return true;
					};
					if (c.rest.Fold (t => false, disposeTail)) {
						c.rest = null;
						break;
					}
					c = c.Tail;
				}
			}
		}

#region LINQ optimizations
		public int Count ()
		{
			return checked((int) LongCount ());
		}

		public T ElementAt (int index)
		{
			if (index < 0)
				throw new ArgumentOutOfRangeException ("index");
			CachedSequence<T> c = this;
			while (--index >= 0) {
				c = c.Tail;
				if (c == null)
					throw new ArgumentOutOfRangeException ("index", "`index` is larger than the collection size.");
			}
			return c.Head;
		}

		public T First ()
		{
			return Head;
		}

		public T FirstOrDefault ()
		{
			return Head;
		}

		public long LongCount ()
		{
			long count = 1;
			var t = Tail;
			while (t != null) {
				++count;
				t = t.Tail;
			}
			return count;
		}

		public CachedSequence<T> Reverse ()
		{
			CachedSequence<T> newHead = new CachedSequence<T> (Head);
			CachedSequence<T> t = Tail;
			while (t != null) {
				newHead = new CachedSequence<T> (t.Head, newHead);
				t       = t.Tail;
			}
			return newHead;
		}
#endregion
	}
}

