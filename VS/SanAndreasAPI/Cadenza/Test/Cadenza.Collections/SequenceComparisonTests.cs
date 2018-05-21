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
using NUnit.Framework;
using System.Linq;
using System.Collections.Generic;

namespace Cadenza.Collections.Tests
{
	[TestFixture]
	public class SequenceComparisonTests
	{
		private class CaseInsensitiveStringComparer
			: IEqualityComparer<string>
		{
			private static readonly CaseInsensitiveStringComparer _default = new CaseInsensitiveStringComparer();
			public static CaseInsensitiveStringComparer Default
			{
				get { return _default; }
			}

			public bool Equals (string x, string y)
			{
				return x.ToLower() == y.ToLower();
			}

			public int GetHashCode (string obj)
			{
				return obj.ToLower().GetHashCode();
			}
		}

		[Test]
		public void CtorEqualityComparer()
		{
			var c = new SequenceComparison<string> (Enumerable.Empty<string>(), Enumerable.Empty<string>(), CaseInsensitiveStringComparer.Default);
			Assert.AreEqual (CaseInsensitiveStringComparer.Default, c.Comparer);
		}

		[Test]
		public void CtorEqualityComparerNull()
		{
			var c = new SequenceComparison<int> (Enumerable.Empty<int>(), Enumerable.Empty<int>());
			Assert.AreEqual (EqualityComparer<int>.Default, c.Comparer);

			c = new SequenceComparison<int> (Enumerable.Empty<int>(), Enumerable.Empty<int>(), null);
			Assert.AreEqual (EqualityComparer<int>.Default, c.Comparer);
		}

		[Test]
		public void CtorNull()
		{
			Assert.AreEqual ("original", Assert.Throws<ArgumentNullException> (() => new SequenceComparison<int> (null, Enumerable.Empty<int>())).ParamName);
			Assert.AreEqual ("update", Assert.Throws<ArgumentNullException> (() => new SequenceComparison<int> (Enumerable.Empty<int>(), null)).ParamName);

			Assert.AreEqual ("original", Assert.Throws<ArgumentNullException> (() => new SequenceComparison<int> (null, Enumerable.Empty<int>(), null)).ParamName);
			Assert.AreEqual ("update", Assert.Throws<ArgumentNullException> (() => new SequenceComparison<int> (Enumerable.Empty<int>(), null, null)).ParamName);
		}

		[Test]
		public void JustAdded()
		{
			var c = new SequenceComparison<int> (Enumerable.Empty<int>(), new int[] { 1, 2, 3, 4 });

			Assert.AreEqual (0, c.Stayed.Count());
			Assert.AreEqual (4, c.Added.Count());
			Assert.Contains (1, c.Added.ToList());
			Assert.Contains (2, c.Added.ToList());
			Assert.Contains (3, c.Added.ToList());
			Assert.Contains (4, c.Added.ToList());

			Assert.AreEqual (0, c.Removed.Count());
		}

		[Test]
		public void JustStayed()
		{
			var c = new SequenceComparison<int> (new int[] { 1, 2, 3, 4 }, new int[] { 1, 2, 3, 4 });

			Assert.AreEqual (4, c.Stayed.Count());
			Assert.Contains (1, c.Stayed.ToList());
			Assert.Contains (2, c.Stayed.ToList());
			Assert.Contains (3, c.Stayed.ToList());
			Assert.Contains (4, c.Stayed.ToList());

			Assert.AreEqual (0, c.Added.Count());
			Assert.AreEqual (0, c.Removed.Count());
		}

		[Test]
		public void JustRemoved()
		{
			var c = new SequenceComparison<int> (new int[] { 1, 2, 3, 4 }, Enumerable.Empty<int>());

			Assert.AreEqual (0, c.Stayed.Count());
			Assert.AreEqual (0, c.Added.Count());

			Assert.AreEqual (4, c.Removed.Count());
			Assert.Contains (1, c.Removed.ToList());
			Assert.Contains (2, c.Removed.ToList());
			Assert.Contains (3, c.Removed.ToList());
			Assert.Contains (4, c.Removed.ToList());
		}

		[Test]
		public void None()
		{
			var c = new SequenceComparison<int> (Enumerable.Empty<int>(), Enumerable.Empty<int>());

			Assert.AreEqual (0, c.Stayed.Count());
			Assert.AreEqual (0, c.Added.Count());
			Assert.AreEqual (0, c.Removed.Count());
		}

		[Test]
		public void AddedStayed()
		{
			var c = new SequenceComparison<int> (new int[] { 1, 2 }, new int[] { 1, 2, 3, 4 });

			Assert.AreEqual (2, c.Stayed.Count());
			Assert.Contains (1, c.Stayed.ToList());
			Assert.Contains (2, c.Stayed.ToList());

			Assert.AreEqual (2, c.Added.Count());
			Assert.Contains (3, c.Added.ToList());
			Assert.Contains (4, c.Added.ToList());

			Assert.AreEqual (0, c.Removed.Count());
		}

		[Test]
		public void StayedRemoved()
		{
			var c = new SequenceComparison<int> (new int[] { 1, 2, 3, 4 }, new int[] { 1, 2 });

			Assert.AreEqual (2, c.Stayed.Count());
			Assert.Contains (1, c.Stayed.ToList());
			Assert.Contains (2, c.Stayed.ToList());

			Assert.AreEqual (0, c.Added.Count());

			Assert.AreEqual (2, c.Removed.Count());
			Assert.Contains (3, c.Removed.ToList());
			Assert.Contains (4, c.Removed.ToList());
		}

		[Test]
		public void AddedRemoved()
		{
			var c = new SequenceComparison<int> (new int[] { 1, 2 }, new int[] { 3, 4 });

			Assert.AreEqual (0, c.Stayed.Count());

			Assert.AreEqual (2, c.Added.Count());
			Assert.Contains (3, c.Added.ToList());
			Assert.Contains (4, c.Added.ToList());

			Assert.AreEqual (2, c.Removed.Count());
			Assert.Contains (1, c.Removed.ToList());
			Assert.Contains (2, c.Removed.ToList());
		}

		[Test]
		public void AddedStayedRemoved()
		{
			var c = new SequenceComparison<int> (new int[] { 1, 2, 3 }, new int[] { 1, 2, 4, 5 });

			Assert.AreEqual (2, c.Stayed.Count());
			Assert.Contains (1, c.Stayed.ToList());
			Assert.Contains (2, c.Stayed.ToList());

			Assert.AreEqual (2, c.Added.Count());
			Assert.Contains (4, c.Added.ToList());
			Assert.Contains (5, c.Added.ToList());

			Assert.AreEqual (1, c.Removed.Count());
			Assert.Contains (3, c.Removed.ToList());
		}

		[Test]
		public void EqualityComparer()
		{
			var c = new SequenceComparison<string> (new string[] { "HI", "hello" }, new string[] { "hi", "blah" },
																							CaseInsensitiveStringComparer.Default);

			Assert.AreEqual (1, c.Stayed.Count());
			Assert.Contains ("HI", c.Stayed.ToList());

			Assert.AreEqual (1, c.Removed.Count());
			Assert.Contains ("hello", c.Removed.ToList());

			Assert.AreEqual (1, c.Added.Count());
			Assert.Contains ("blah", c.Added.ToList());
		}
	}
}