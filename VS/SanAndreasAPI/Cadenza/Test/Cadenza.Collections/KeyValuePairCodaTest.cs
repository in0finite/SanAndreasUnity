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
	public class KeyValuePairTest : BaseRocksFixture {

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Aggregate_FuncNull ()
		{
			Func<int, int, int> f = null;
			new KeyValuePair<int,int> (1, 2).Aggregate (f);
		}

		[Test]
		public void Aggregate ()
		{
			Assert.AreEqual (
					3, 
					new KeyValuePair<int, int>(1, 2).Aggregate ((k, v) => k+v));
			Assert.AreEqual (
					"1,2", 
					new KeyValuePair<int, int>(1, 2).Aggregate ((k, v) => k+","+v));
		}

		[Test]
		public void ToTuple ()
		{
			KeyValuePair<int, string> k = new KeyValuePair<int, string> (42, "42");
			var t = k.ToTuple ();
			Assert.AreEqual (k.Key,   t.Item1);
			Assert.AreEqual (k.Value, t.Item2);
		}
	}
}
