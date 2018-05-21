//
// BidirectionalLookupTest.cs
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

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Cadenza.Collections.Tests
{
	[TestFixture]
	public class BidirectionalLookupTest
		: MutableLookupContract
	{
		protected override IMutableLookup<T, TE> GetLookupImplementation<T, TE>()
		{
			return new BidirectionalLookup<T, TE>();
		}

		protected override IMutableLookup<T, TE> GetLookupImplementation<T, TE>(System.Collections.Generic.IEqualityComparer<T> keyEquality)
		{
			return new BidirectionalLookup<T, TE> (keyEquality, EqualityComparer<TE>.Default);
		}

		[Test]
		public void AddCustomValueComparer()
		{
			var comparer = new DelegatedEqualityComparer<string, int> (s => s.Length);

			var lookup = new BidirectionalLookup<string, string> (comparer, comparer);
			lookup.Add ("foo", "bar1");
			lookup.Add ("bar", "baz1");

			Assert.IsTrue (lookup.Contains ("baz"));
			Assert.IsTrue (lookup.Inverse.Contains ("foo1"));
			CollectionAssert.Contains (lookup["foo"], "bar1");
			CollectionAssert.Contains (lookup["foo"], "baz1");
			CollectionAssert.Contains (lookup["bar"], "bar1");
			CollectionAssert.Contains (lookup["bar"], "bar1");
		}

		[Test]
		public void AddInverse()
		{
			var lookup = new BidirectionalLookup<string, string>();
			lookup.Add ("foo", "bar");
			lookup.Add ("foo", "baz");
			lookup.Add ("baz", "bar");

			Assert.AreEqual (2, lookup.Count);
			Assert.AreEqual (2, lookup.Count);
			Assert.IsTrue (lookup.Contains ("foo"));
			Assert.IsTrue (lookup.Contains ("baz"));
			Assert.IsFalse (lookup.Contains ("bar"));
			Assert.IsFalse (lookup.Inverse.Contains ("foo"));
			Assert.IsTrue (lookup.Inverse.Contains ("bar"));
			Assert.IsTrue (lookup.Inverse.Contains ("baz"));
			CollectionAssert.Contains (lookup["foo"], "bar");
			CollectionAssert.Contains (lookup["foo"], "baz");
			CollectionAssert.Contains (lookup.Inverse["bar"], "foo");
			CollectionAssert.Contains (lookup.Inverse["bar"], "baz");
			CollectionAssert.Contains (lookup["baz"], "bar");
		}

		[Test]
		public void ClearInverse()
		{
			var lookup = new BidirectionalLookup<string, string>();
			lookup.Add ("foo", "bar");
			lookup.Clear();

			Assert.AreEqual (0, lookup.Count);
			Assert.AreEqual (0, lookup.Inverse.Count);
			Assert.IsFalse (lookup.Contains ("foo"));
			CollectionAssert.IsEmpty (lookup["foo"]);
			CollectionAssert.IsEmpty (lookup.Inverse["bar"]);
		}

		[Test]
		public void RemoveKeyInverse()
		{
			var lookup = new BidirectionalLookup<string, string>();
			lookup.Add ("baz", "bar");
			lookup.Add ("foo", "bar");
			lookup.Add ("foo", "baz");

			lookup.Inverse.Remove ("bar");

			Assert.IsFalse (lookup.Contains ("baz"));
			Assert.IsFalse (lookup.Inverse.Contains ("bar"));
			CollectionAssert.Contains (lookup["foo"], "baz");
			CollectionAssert.DoesNotContain (lookup["foo"], "bar");
			CollectionAssert.IsEmpty (lookup["baz"]);
			CollectionAssert.IsEmpty (lookup.Inverse["bar"]);
			CollectionAssert.Contains (lookup.Inverse["baz"], "foo");
		}

		[Test]
		public void RemoveElementInverse()
		{
			var lookup = new BidirectionalLookup<string, string>();
			lookup.Add ("baz", "bar");
			lookup.Add ("foo", "bar");
			lookup.Add ("foo", "baz");

			lookup.Remove ("foo", "bar");

			CollectionAssert.DoesNotContain (lookup["foo"], "bar");
			CollectionAssert.DoesNotContain (lookup.Inverse["bar"], "foo");
			CollectionAssert.Contains (lookup["foo"], "baz");
			CollectionAssert.Contains (lookup.Inverse["bar"], "baz");
		}
	}
}