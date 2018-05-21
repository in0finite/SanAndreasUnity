//
// KeyValuePairTest.cs
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
	public class LinkedListCodaTest : BaseRocksFixture {

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void NodeAt_SelfNull ()
		{
			LinkedList<int> list = null;
			list.ElementAt (0);
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void NodeAt_IndexIsNegative ()
		{
			LinkedList<int> list = new LinkedList<int>();
			list.ElementAt (-1);
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void NodeAt_IndexIsTooLarge ()
		{
			LinkedList<int> list = new LinkedList<int>();
			list.AddFirst (1);
			list.ElementAt (1);
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void NodeAt_IndexZeroInEmptyList ()
		{
			LinkedList<int> list = new LinkedList<int>();
			list.NodeAt (0);
		}

		[Test]
		public void NodeAt_IndexZeroIsFirst ()
		{
			LinkedList<int> list = new LinkedList<int>();
			list.AddFirst (1);
			var first = list.NodeAt (0);
			Assert.AreSame (list.First, first);
		}

		[Test]
		public void NodeAt ()
		{
			#region NodeAt
			LinkedList<int> list = new LinkedList<int>();
			list.AddLast (1); // first node;  index=0
			list.AddLast (2); // middle node; index=1
			list.AddLast (3); // last node;   index=2

			Assert.AreSame (list.First,         list.NodeAt (0));
			Assert.AreSame (list.First.Next,    list.NodeAt (1));
			Assert.AreSame (list.Last.Previous, list.NodeAt (1));
			Assert.AreSame (list.Last,          list.NodeAt (2));
			Assert.AreEqual (3, list.NodeAt (2).Value);
			#endregion
		}
	}
}
