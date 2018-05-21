//
// DelegatedEqualityComparerTest.cs
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
using System.Linq;
using NUnit.Framework;

namespace Cadenza.Collections.Tests
{
	[TestFixture]
	public class SelectorDelegatedEqualityComparerTest
	{
		[Test]
		public void Ctor_Null()
		{
			Assert.Throws<ArgumentNullException> (() => new DelegatedEqualityComparer<string, string> (null));
		}

		[Test]
		public void Equals()
		{
			var comparer = new DelegatedEqualityComparer<string, int> (s => s.Length);
			Assert.IsTrue (comparer.Equals ("foo", "bar"));
			Assert.IsFalse (comparer.Equals ("foo", "fo"));
		}

		[Test]
		public void DelegatedHashCode()
		{
			var comparer = new DelegatedEqualityComparer<string, int> (s => s.Length);
			Assert.AreEqual (comparer.GetHashCode ("foo"), comparer.GetHashCode("bar"));
			Assert.AreNotEqual (comparer.GetHashCode ("foo"), comparer.GetHashCode ("fo"));
		}
	}

	[TestFixture]
	public class DelegatedEqualityComparerTest
	{
		[Test]
		public void Ctor_Null()
		{
			Assert.Throws<ArgumentNullException> (() => new DelegatedEqualityComparer<string> (null, s => s.GetHashCode()));
			Assert.Throws<ArgumentNullException> (() => new DelegatedEqualityComparer<string> ((s1, s2) => s1.Equals (s2), null));
		}

		[Test]
		public void Equals()
		{
			var comparer = new DelegatedEqualityComparer<string> ((s1, s2) => s1.Length == s2.Length, s => s.Length.GetHashCode());
			Assert.IsTrue (comparer.Equals ("foo", "bar"));
			Assert.IsFalse (comparer.Equals ("foo", "fo"));
		}

		[Test]
		public void EqualsNull()
		{
			var comparer = new DelegatedEqualityComparer<string> ((s1, s2) => s1.Length == s2.Length, s => s.Length.GetHashCode());
			Assert.IsTrue (comparer.Equals (null, null));
			Assert.IsFalse (comparer.Equals ("foo", null));
		}

		[Test]
		public void EqualsReference()
		{
			string str = "s";

			var comparer = new DelegatedEqualityComparer<string> ((s1, s2) => s1.Length != s2.Length, s => s.Length.GetHashCode());
			Assert.IsTrue (comparer.Equals (str, str));
			Assert.IsTrue (comparer.Equals ("sf", "s"));
		}

		[Test]
		public void DelegatedHashCode()
		{
			var comparer = new DelegatedEqualityComparer<string> ((s1, s2) => s1.Length == s2.Length, s => s.Length.GetHashCode());
			Assert.AreEqual (comparer.GetHashCode ("foo"), comparer.GetHashCode("bar"));
			Assert.AreNotEqual (comparer.GetHashCode ("foo"), comparer.GetHashCode ("fo"));
		}
	}
}