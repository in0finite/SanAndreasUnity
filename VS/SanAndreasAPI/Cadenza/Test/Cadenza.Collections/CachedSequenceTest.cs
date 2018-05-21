//
// CachedSequenceTest.cs
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
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using Cadenza.Collections;
using Cadenza.Tests;

namespace Cadenza.Collections.Tests {

	[TestFixture]
	public class CachedSequenceTest : EnumerableContract {

		protected override IEnumerable<T> CreateSequence<T>(IEnumerable<T> source)
		{
			return new CachedSequence<T>(source);
		}

		[Test]
		public void Ctor_Head ()
		{
			string h = null;
			var s = new CachedSequence<string> (h);
			Assert.AreEqual (null, s.Head);
			Assert.AreEqual (null, s.Tail);

			s = new CachedSequence<string> ("value");
			Assert.AreEqual ("value", s.Head);
			Assert.AreEqual (null, s.Tail);
		}

		[Test]
		public void Ctor_Head_Tail_NotNull ()
		{
			var t = new CachedSequence<string> ("tail");
			var s = new CachedSequence<string> ("head", t);
			Assert.AreEqual ("head", s.Head);
			Assert.AreEqual (t, s.Tail);
		}

		[Test]
		public void Ctor_Head_Tail_Null ()
		{
			var s = new CachedSequence<string> ("head", null);
			Assert.AreEqual ("head", s.Head);
			Assert.AreEqual (null, s.Tail);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void Ctor_Collection_Null ()
		{
			string[] c = null;
			var s = new CachedSequence<string> (c);
			Ignore (s);
		}

		[Test, ExpectedException (typeof (InvalidOperationException))]
		public void Ctor_Collection_Empty ()
		{
			var s = new CachedSequence<string> (new string [0]);
			Ignore (s);
		}

		[Test]
		public void Ctor_Collection_Single ()
		{
			var s = new CachedSequence<string> (new string [] {"value"});
			Assert.AreEqual ("value", s.Head);
			Assert.AreEqual (null, s.Tail);
		}

		[Test]
		public void Ctor_Collection_Single_CheckDisposed ()
		{
			var d = new DisposedCounter ();
			var s = new CachedSequence<int> (d.Values (1));
			Assert.AreEqual (1, d.Disposed);
			Assert.AreEqual (0, s.Head);
			Assert.AreEqual (null, s.Tail);
		}

		[Test]
		public void Ctor_Collection_Multiple ()
		{
			var d = new DisposedCounter ();
			var s = new CachedSequence<int> (d.Values (100));

			Assert.AreEqual (0, d.Disposed);
			Assert.AreEqual (0, s.Head);
			Assert.IsNotNull (s.Tail);

			var t = s.Tail;
			Assert.AreEqual (0, d.Disposed);
			Assert.AreEqual (1, t.Head);
			Assert.IsNotNull (t.Tail);

			int c = 98;
			while (c-- > 0)
				t = t.Tail;

			Assert.AreEqual (null, t.Tail);
			Assert.AreEqual (1, d.Disposed);
		}

		[Test]
		public void Docs_Ctor_RandomValues ()
		{
			#region RandomValues
			var values = new Random ();
			// Note: wrapping an infinite sequence
			var randSeq = new CachedSequence<int> (EnumerableTest.RandomValues (values, 10));
			Assert.IsTrue (
					randSeq.Take (20).SelectFromEach (randSeq.Take (10),
						(l, r) => l + r)
					.All (x => x % 2 == 0));
			#endregion

			// The above implicitly terminates the infinite sequence (as the
			// enumerator was disposed); there should be ~10 elements in it.
			Assert.AreEqual (11, randSeq.Count ());
		}

		[Test]
		public void GetEnumerator_Dispose_Is_Forwarded ()
		{
			var d = new DisposedCounter ();
			var s = new CachedSequence<int> (d.Values (100));
			var q = s.Take (1);
			int n = 0;
			foreach (var e in q) {
				Assert.AreEqual (n, e);
				++n;
			}
			Assert.AreEqual (1, n);

			// When the CachedSequence<T> enumerator is disposed,
			// the underlying enumerator should also be disposed.
			Assert.AreEqual (1, d.Disposed);

			// Note that we've "lost" 99 elements
			Assert.AreEqual (2, s.Count());
		}

		[Test]
		public void Append ()
		{
			var a = new CachedSequence<int> (1);
			var b = a.Append (2);
			Assert.IsFalse (object.ReferenceEquals (a, b));

			var c = b.Append (3);
			Assert.IsFalse (object.ReferenceEquals (a, c));
			Assert.IsFalse (object.ReferenceEquals (b, c));

			var d = c.Append (4);
			Assert.IsFalse (object.ReferenceEquals (a, d));
			Assert.IsFalse (object.ReferenceEquals (b, d));
			Assert.IsFalse (object.ReferenceEquals (c, d));

			Assert.AreEqual (4, d.Count());
			Assert.AreEqual ("1,2,3,4", d.Implode (","));
		}

		[Test]
		public void Prepend ()
		{
			var a = new CachedSequence<int> (1);
			var b = a.Prepend (2);
			Assert.IsFalse (object.ReferenceEquals (a, b));

			var c = b.Prepend (3);
			Assert.IsFalse (object.ReferenceEquals (a, c));
			Assert.IsFalse (object.ReferenceEquals (b, c));

			var d = c.Prepend (4);
			Assert.IsFalse (object.ReferenceEquals (a, d));
			Assert.IsFalse (object.ReferenceEquals (b, d));
			Assert.IsFalse (object.ReferenceEquals (c, d));

			Assert.AreEqual (4, d.Count());
			Assert.AreEqual ("4,3,2,1", d.Implode (","));
		}

		[Test]
		public void Count ()
		{
			var s = new CachedSequence<int> (new[]{1, 2, 3, 4, 5});
			Assert.AreEqual (5, s.Count ());
		}

		[Test]
		public void LongCount ()
		{
			var s = new CachedSequence<int> (new[]{1, 2, 3, 4, 5});
			Assert.AreEqual (5L, s.LongCount ());
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ElementAt_NegativeIndex ()
		{
			CachedSequence<int> c = new CachedSequence<int> (42);
			c.ElementAt (-1);
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ElementAt_TooLarge ()
		{
			CachedSequence<int> c = new CachedSequence<int> (42);
			c.ElementAt (2);
		}

		[Test]
		public void ElementAt ()
		{
			var a = new CachedSequence<int> (1);
			var b = new CachedSequence<int> (2, a);
			var c = new CachedSequence<int> (3, b);
			var d = new CachedSequence<int> (4, c);

			Assert.AreEqual (4, d.Count());
			Assert.AreEqual (4, d.ElementAt (0));
			Assert.AreEqual (3, d.ElementAt (1));
			Assert.AreEqual (2, d.ElementAt (2));
			Assert.AreEqual (1, d.ElementAt (3));
		}

		[Test]
		public void Reverse ()
		{
			Assert.AreEqual ("4,3,2,1",
					new CachedSequence<int> (new[]{1, 2, 3, 4}).Reverse ().Implode (","));
		}
	}
}
