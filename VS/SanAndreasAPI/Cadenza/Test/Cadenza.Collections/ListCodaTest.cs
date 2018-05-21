//
// ListCodaTest.cs
//
// Author:
//   Chris Chilvers <chilversc@googlemail.com>
//
// Copyright (c) 2010 Chris Chilvers
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
using System.Text;
using NUnit.Framework;
using Cadenza.Collections;

namespace Cadenza.Collections.Tests
{
	[TestFixture]
	public class ListCodaTest
	{
		[Test]
		public void BinarySearch_SelfNull ()
		{
			List<int> items = null;
			Func<int, int, int> comparer = (x, y) => x.CompareTo (y);

			var ex = Assert.Throws<ArgumentNullException> (() => items.BinarySearch (5, comparer));
			Assert.AreEqual ("self", ex.ParamName);
		}

		[Test]
		public void BinarySearch_ItemNull ()
		{
			List<int> items = new List<int> {1, 2, 4, 8};
			Func<int, object, int> comparer = (x, y) => 1;

			Assert.AreEqual (~0, items.BinarySearch (null, comparer));
		}

		[Test]
		public void BinarySearch_ComparerNull ()
		{
			List<int> items = new List<int> {1, 2, 4, 8};
			Func<int, int, int> comparer = null;

			var ex = Assert.Throws<ArgumentNullException> (() => items.BinarySearch (5, comparer));
			Assert.AreEqual ("comparer", ex.ParamName);
		}

		[Test]
		public void BinarySearch ()
		{
			List<int> items = new List<int> {1, 2, 4, 8};
			Func<int, int, int> comparer = (x, y) => x.CompareTo (y);

			Assert.AreEqual (~0, items.BinarySearch (0, comparer));
			Assert.AreEqual ( 0, items.BinarySearch (1, comparer));
			Assert.AreEqual ( 1, items.BinarySearch (2, comparer));
			Assert.AreEqual (~2, items.BinarySearch (3, comparer));
			Assert.AreEqual ( 3, items.BinarySearch (8, comparer));
			Assert.AreEqual (~4, items.BinarySearch (9, comparer));
		}

		[Test]
		public void BinarySearch_EmptyList ()
		{
			List<int> items = new List<int> ();
			Func<int, int, int> comparer = (x, y) => x.CompareTo (y);
			
			Assert.AreEqual (~0, items.BinarySearch (5, comparer));
		}
	}
}
