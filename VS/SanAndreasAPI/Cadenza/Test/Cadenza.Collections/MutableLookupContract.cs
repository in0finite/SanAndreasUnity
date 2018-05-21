//
// MutableLookupContract.cs
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
using Cadenza.Collections;
using NUnit.Framework;

namespace Cadenza.Collections.Tests
{
	public abstract class MutableLookupContract
	{
		protected abstract IMutableLookup<T, TE> GetLookupImplementation<T, TE>();
		protected abstract IMutableLookup<T, TE> GetLookupImplementation<T, TE> (IEqualityComparer<T> keyEquality);

		protected IMutableLookup<string, string> GetTestLookup()
		{
			IMutableLookup<string, string> lookup = GetLookupImplementation<string, string>();
			lookup.Add (null, (string)null);
			lookup.Add (null, "blah");
			lookup.Add (null, "monkeys");
			lookup.Add ("F", "Foo");
			lookup.Add ("F", "Foobar");
			lookup.Add ("B", "Bar");
			return lookup;
		}

		[Test]
		public void Add()
		{
			var lookup = GetLookupImplementation<string, string>();
			lookup.Add ("F", "Foo");
			Assert.AreEqual (1, lookup.Count);
			Assert.AreEqual ("Foo", lookup["F"].First());
			lookup.Add ("F", "Foobar");
			Assert.AreEqual (1, lookup.Count);
			Assert.AreEqual (2, lookup["F"].Count());
		}
		
		[Test]
		public void AddNull()
		{
			var lookup = GetLookupImplementation<string, string>();
			lookup.Add (null, "Foo");
			Assert.AreEqual (1, lookup.Count);
			Assert.AreEqual (1, lookup[null].Count());
			lookup.Add (null, (string)null);
			Assert.AreEqual (1, lookup.Count);
			Assert.AreEqual (2, lookup[null].Count());
		}

		[Test]
		public void AddMultiple()
		{
			var values = new [] { "Foo", "Foobar" };
			var lookup = GetLookupImplementation<string, string>();
			lookup.Add ("key", values);
			Assert.AreEqual (1, lookup.Count);
			Assert.Contains (values[0], lookup["key"].ToList());
			Assert.Contains (values[1], lookup["key"].ToList());
			lookup.Add ("key2", values);
			Assert.AreEqual (2, lookup.Count);
			Assert.Contains (values[0], lookup["key2"].ToList());
			Assert.Contains (values[1], lookup["key2"].ToList());
		}

		[Test]
		public void AddMultipleNull()
		{
			var values = new [] { "Foo", "Foobar" };
			var lookup = GetLookupImplementation<string, string>();
			lookup.Add (null, values);
			Assert.AreEqual (1, lookup.Count);
			Assert.IsTrue (values.SequenceEqual (lookup[null]), "S");

			Assert.Throws<ArgumentNullException> (() => lookup.Add ("foo", (IEnumerable<string>)null));
		}

		[Test]
		public void AddCustomKeyEquality()
		{
			var lookup = GetLookupImplementation<string, string> (new DelegatedEqualityComparer<string> ((s1, s2) => s1.Length == s2.Length, s => s.Length.GetHashCode()));
			lookup.Add ("s1", "foo");
			lookup.Add ("s2", "bar");

			CollectionAssert.Contains (lookup["s1"], "foo");
			CollectionAssert.Contains (lookup["s1"], "bar");
		}

		[Test]
		public void CountRefType()
		{
			var lookup = GetLookupImplementation<string, string>();
			Assert.AreEqual (0, lookup.Count);
			lookup.Add (null, "blah");
			Assert.AreEqual (1, lookup.Count);
			lookup.Add ("F", "Foo");
			Assert.AreEqual (2, lookup.Count);
			lookup.Add ("F", "Foobar");
			Assert.AreEqual (2, lookup.Count);

			lookup.Remove (null, "blah");
			Assert.AreEqual (1, lookup.Count);
		}

		[Test]
		public void CountValueType()
		{
			var lookup = GetLookupImplementation<int, int>();
			Assert.AreEqual (0, lookup.Count);
			lookup.Add (1, 10);
			Assert.AreEqual (1, lookup.Count);
			lookup.Add (2, 20);
			Assert.AreEqual (2, lookup.Count);
			lookup.Add (2, 21);
			Assert.AreEqual (2, lookup.Count);

			lookup.Remove (1, 10);
			Assert.AreEqual(1, lookup.Count);
		}

		[Test]
		public void RemoveExistingKey()
		{
			var lookup = GetLookupImplementation<string, string>();
			lookup.Add (null, "blah");
			lookup.Add (null, "monkeys");
			lookup.Add ("F", "Foo");
			lookup.Add ("F", "Foobar");
			lookup.Add ("B", "Bar");

			Assert.AreEqual (3, lookup.Count);

			Assert.IsTrue (lookup.Remove (null));
			Assert.AreEqual (2, lookup.Count);
			CollectionAssert.IsEmpty (lookup[null]);

			Assert.IsTrue (lookup.Remove ("F"));
			Assert.AreEqual (1, lookup.Count);
			CollectionAssert.IsEmpty (lookup["F"]);
		}

		[Test]
		public void RemoveNonExistingKey()
		{
			var lookup = GetLookupImplementation<string, string>();
			lookup.Add ("F", "Foo");
			lookup.Add ("F", "Foobar");
			lookup.Add ("B", "Bar");

			Assert.AreEqual (2, lookup.Count);

			Assert.IsFalse (lookup.Remove (null));
			Assert.AreEqual (2, lookup.Count);

			Assert.IsFalse (lookup.Remove ("A"));
			Assert.AreEqual (2, lookup.Count);
		}

		[Test]
		public void RemoveExistingElement()
		{
			var lookup = GetLookupImplementation<string, string>();
			lookup.Add (null, "blah");
			lookup.Add (null, "monkeys");
			lookup.Add ("F", "Foo");
			lookup.Add ("F", "Foobar");
			lookup.Add ("B", "Bar");

			Assert.AreEqual (3, lookup.Count);

			Assert.IsTrue (lookup.Remove (null, "blah"));
			Assert.AreEqual (3, lookup.Count);
			Assert.IsTrue (lookup.Remove (null, "monkeys"));
			Assert.AreEqual (2, lookup.Count);

			Assert.IsTrue (lookup.Remove ("F"));
			Assert.AreEqual (1, lookup.Count);
		}

		[Test]
		public void RemoveNonExistingElement()
		{
			var lookup = GetLookupImplementation<string, string>();
			lookup.Add(null, "blah");
			lookup.Add(null, "monkeys");
			lookup.Add("F", "Foo");
			lookup.Add("F", "Foobar");
			lookup.Add("B", "Bar");

			Assert.IsFalse (lookup.Remove ("D"));
			Assert.AreEqual(3, lookup.Count);

			Assert.IsFalse (lookup.Remove ("F", "asdf"));
			Assert.AreEqual(3, lookup.Count);

			lookup.Remove (null);
			Assert.IsFalse (lookup.Remove (null));
			Assert.AreEqual (2, lookup.Count);
		}

		[Test]
		public void ClearWithNull()
		{
			var lookup = GetLookupImplementation<string, string>();
			lookup.Add (null, "blah");
			lookup.Add ("F", "Foo");

			lookup.Clear();

			Assert.AreEqual (0, lookup.Count);
			Assert.IsFalse (lookup.Contains (null));
			Assert.IsFalse (lookup.Contains ("F"));
		}

		[Test]
		public void ClearWithoutNull()
		{
			var lookup = GetLookupImplementation<string, string>();
			lookup.Add("F", "Foo");
			lookup.Add("F", "Foobar");
			lookup.Add("B", "Bar");

			lookup.Clear();
			Assert.AreEqual (0, lookup.Count);
			Assert.IsFalse (lookup.Contains ("F"));
			Assert.IsFalse (lookup.Contains ("B"));
		}

		[Test]
		public void ClearValueType()
		{
			var lookup = GetLookupImplementation<int, int>();
			lookup.Add (1, 10);
			lookup.Add (1, 12);
			lookup.Add (1, 13);
			lookup.Add (2, 21);
			lookup.Add (2, 22);
			lookup.Add (2, 23);

			lookup.Clear();
			Assert.AreEqual (0, lookup.Count);
			Assert.IsFalse (lookup.Contains (1));
			Assert.IsFalse (lookup.Contains (2));
		}

		[Test]
		public void Contains()
		{
			var lookup = GetLookupImplementation<string, string>();
			lookup.Add(null, "blah");
			lookup.Add(null, "monkeys");
			lookup.Add("F", "Foo");
			lookup.Add("F", "Foobar");
			lookup.Add("B", "Bar");

			Assert.IsTrue (lookup.Contains ("B"));
		}

		[Test]
		public void DoesNotContain()
		{
			var lookup = GetLookupImplementation<string, string>();
			lookup.Add(null, "blah");
			lookup.Add(null, "monkeys");
			lookup.Add("F", "Foo");
			lookup.Add("F", "Foobar");
			lookup.Add("B", "Bar");

			Assert.IsFalse (lookup.Contains ("D"));
		}

		[Test]
		public void ContainsNull()
		{
			var lookup = GetTestLookup();

			Assert.IsTrue (lookup.Contains (null));
		}

		[Test]
		public void DoesNotContainNull()
		{
			var lookup = GetLookupImplementation<string, string>();
			lookup.Add("F", "Foo");
			lookup.Add("F", "Foobar");
			lookup.Add("B", "Bar");

			Assert.IsFalse (lookup.Contains (null));
		}

		[Test]
		public void Indexer()
		{
			var lookup = GetTestLookup();

			Assert.AreEqual (2, lookup["F"].Count());
		}

		[Test]
		public void IndexerNull()
		{
			var lookup = GetTestLookup();

			Assert.AreEqual (3, lookup[null].Count());
		}

		[Test]
		public void IndexerNotFound()
		{
			var lookup = GetTestLookup();

			Assert.AreEqual (0, lookup["D"].Count());
		}

		[Test]
		public void Enumerator()
		{
			var lookup = GetLookupImplementation<int, int>();
			lookup.Add (1, 10);
			lookup.Add (2, 20);
			lookup.Add (2, 21);
			lookup.Add (3, 30);

			Assert.AreEqual (3, lookup.Count());
			Assert.IsTrue (lookup.Any (g => g.Key == 1));
			Assert.IsTrue (lookup.Any (g => g.Key == 2));
			Assert.IsTrue (lookup.Any (g => g.Key == 3));
		}

		[Test]
		public void EnumeratorNotNull()
		{
			var lookup = GetLookupImplementation<string, string>();
			lookup.Add ("h", "hi");
			lookup.Add ("h", "hai");
			lookup.Add ("b", "bai");
			lookup.Add ("b", "bye");

			Assert.AreEqual (2, lookup.Count);
			Assert.IsTrue (lookup.Any (g => g.Key == "h"));
			Assert.IsTrue (lookup.Any (g => g.Key == "b"));
			Assert.IsFalse (lookup.Any (g => g.Key == null));
		}

		[Test]
		public void EnumeratorNull()
		{
			var lookup = GetTestLookup();

			Assert.AreEqual (3, lookup.Count());
			Assert.IsTrue (lookup.Any (g => g.Key == null));
			Assert.IsTrue (lookup.Any (g => g.Key == "F"));
			Assert.IsTrue (lookup.Any (g => g.Key == "B"));
		}

		[Test]
		public void NullGroupingEnumerator()
		{
			var lookup = GetTestLookup();

			Assert.AreEqual (3, lookup[null].Count());
			Assert.IsTrue (lookup[null].Any (s => s == "blah"));
			Assert.IsTrue (lookup[null].Any (s => s == "monkeys"));
			Assert.IsTrue (lookup[null].Any (s => s == null));
		}

		[Test]
		public void GroupingEnumerator()
		{
			var lookup = GetLookupImplementation<int, int>();
			lookup.Add (1, 10);
			lookup.Add (2, 20);
			lookup.Add (2, 21);
			lookup.Add (3, 30);

			Assert.AreEqual (2, lookup[2].Count());
			Assert.IsTrue (lookup[2].Any (i => i == 20));
			Assert.IsTrue (lookup[2].Any(i => i == 21));
		}
	}
}
