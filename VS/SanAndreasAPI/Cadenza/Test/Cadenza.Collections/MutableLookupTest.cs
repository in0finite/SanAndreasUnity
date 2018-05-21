//
// MutableLookupTest.cs
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
using NUnit.Framework;

namespace Cadenza.Collections.Tests
{
	[TestFixture]
	public class MutableLookupTest
		: MutableLookupContract
	{
		protected override IMutableLookup<T, TE> GetLookupImplementation<T, TE>()
		{
			return new MutableLookup<T, TE>();
		}

		protected override IMutableLookup<T, TE> GetLookupImplementation<T, TE> (IEqualityComparer<T> keyEquality)
		{
			return new MutableLookup<T, TE> (keyEquality);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CtorNull()
		{
			new MutableLookup<string, string> ((ILookup<string,string>)null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CtorEqualityComparerNull()
		{
			new MutableLookup<string, string> ((IEqualityComparer<string>)null);
		}

		[Test]
		public void CtorILookup()
		{
			List<int> ints = new List<int> { 10, 20, 21, 30 };
			var lookup = new MutableLookup <int, int> (ints.ToLookup (i => Int32.Parse (i.ToString()[0].ToString())));

			Assert.AreEqual (3, lookup.Count);
			Assert.AreEqual (1, lookup[1].Count());
			Assert.AreEqual (2, lookup[2].Count());
			Assert.AreEqual (1, lookup[3].Count());
		}

		[Test]
		public void CtorILookupWithNulls()
		{
			List<string> strs = new List<string> { "Foo", "Foos", "Foobar", "Monkeys", "Bar", "Ban", "Barfoo" };

			var lookup = new MutableLookup<string, string> (strs.ToLookup (s => (s[0] != 'F' && s[0] != 'B') ? null : s[0].ToString()));
			Assert.AreEqual (3, lookup.Count);
			Assert.AreEqual (3, lookup["F"].Count());
			Assert.AreEqual (3, lookup["B"].Count());
			Assert.AreEqual (1, lookup[null].Count());
		}

		[Test]
		public void TryGetValuesNull()
		{
			var lookup = (MutableLookup<string,string>)GetTestLookup();

			IEnumerable<string> values;
			Assert.IsTrue (lookup.TryGetValues (null, out values));
			Assert.IsNotNull (values);

			var v = values.ToList();
			Assert.AreEqual (3, v.Count);
			Assert.Contains ("blah", v);
			Assert.Contains ("monkeys", v);
			Assert.Contains (null, v);
		}

		[Test]
		public void TryGetValues()
		{
			var lookup = (MutableLookup<string,string>)GetTestLookup();

			IEnumerable<string> values;
			Assert.IsTrue (lookup.TryGetValues ("F", out values));
			Assert.IsNotNull (values);

			var v = values.ToList();
			Assert.AreEqual (2, v.Count);
			Assert.Contains ("Foo", v);
			Assert.Contains ("Foobar", v);
		}

		[Test]
		public void TryGetValuesFail()
		{
			var lookup = (MutableLookup<string,string>)GetTestLookup();

			IEnumerable<string> values;
			Assert.IsFalse (lookup.TryGetValues ("notfound", out values));
			Assert.IsNull (values);
		}

		[Test]
		public void TryGetValuesNullFail()
		{
			var lookup = (MutableLookup<string,string>)GetLookupImplementation<string,string>();

			IEnumerable<string> values;
			Assert.IsFalse (lookup.TryGetValues (null, out values));
			Assert.IsNull (values);
		}
	}
}
