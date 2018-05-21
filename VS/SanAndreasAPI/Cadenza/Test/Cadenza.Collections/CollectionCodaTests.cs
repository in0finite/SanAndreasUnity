//
// CollectionCodaTests.cs
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
using NUnit.Framework;
using Cadenza.Collections;

namespace Cadenza.Tests
{
	[TestFixture]
	public class CollectionCodaTests
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void RemoveAll_SelfNull()
		{
			CollectionCoda.RemoveAll<int> (null, i => i == 1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void RemoveAll_PredicateNull()
		{
			ICollection<int> c = Enumerable.Range (0, 1).ToList();
			c.RemoveAll (null);
		}

		[Test]
		public void RemoveAll_SingleList()
		{
			IList<int> list = new List<int> { 1, 2, 3 };
			Assert.IsTrue (list.RemoveAll (i => i == 2));

			Assert.AreEqual (2, list.Count);
			Assert.IsFalse (list.Contains (2), "List still contains removed item");
		}

		[Test]
		public void RemoveAll_MultipleList()
		{
			IList<int> list = new List<int> { 1, 2, 3, 4, 1, 2, 5 };
			Assert.IsTrue (list.RemoveAll (i => i == 2));

			Assert.AreEqual (5, list.Count);
			Assert.IsFalse (list.Contains (2), "List still contains removed items");
		}

		[Test]
		public void RemoveAll_SingleCollection()
		{
			var collection = new Dictionary<int, int> { { 1, 1 }, { 2, 2 }, { 3, 3 } };
			Assert.IsTrue (collection.RemoveAll (kvp => kvp.Value == 2));

			Assert.AreEqual (2, collection.Count);
			Assert.IsFalse (collection.ContainsValue (2), "Collection still contains removed items");
		}

		[Test]
		public void RemoveAll_MultipleCollection()
		{
			var collection = new Dictionary<int, int> { { 1, 1 }, { 2, 2 }, { 3, 3 }, { 4, 2 }, { 5, 3 } };
			Assert.IsTrue (collection.RemoveAll (kvp => kvp.Value == 2));

			Assert.AreEqual (3, collection.Count);
			Assert.IsFalse (collection.ContainsValue (2), "Collection still contains removed items");
		}

		[Test]
		public void RemoveAll_Collection_NotFound()
		{
			var collection = new Dictionary<int, int> { { 1, 1 }, { 3, 3 }, { 5, 3 } };
			Assert.IsFalse (collection.RemoveAll (kvp => kvp.Value == 2));

			Assert.AreEqual (3, collection.Count);
		}

		[Test]
		public void RemoveAll_List_NotFound()
		{
			IList<int> list = new List<int> { 1, 3 };
			Assert.IsFalse (list.RemoveAll (i => i == 2));

			Assert.AreEqual (2, list.Count);
		}
	}
}