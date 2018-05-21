//
// Int32Test.cs
//
// Author:
//   Jb Evain (jbevain@novell.com)
//
// Copyright (c) 2007 Novell, Inc. (http://www.novell.com)
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

using Cadenza;
using Cadenza.Collections;

namespace Cadenza.Tests {

	[TestFixture]
	public class Int32Test : BaseRocksFixture {

		[Test]
		public void Times ()
		{
			var data = new int [6];
			var result = new int [] { 0, 1, 2, 3, 4, 5 };

			6.Times ().ForEach (i => data [i] = i);

			AssertAreSame (result, data);
		}

		[Test]
		public void UpTo ()
		{
			var data = new int [7];
			var result = new int [] { 6, 7, 8, 9, 10, 11, 12 };

			int j = 0;
			6.UpTo (12).ForEach (i => { data [j++] = i; });

			AssertAreSame (result, data);
		}

		[Test]
		public void DownTo ()
		{
			var data = new int [7];
			var result = new int [] { 12, 11, 10, 9, 8, 7, 6 };

			int j = 0;
			12.DownTo (6).ForEach (i => data [j++] = i);

			AssertAreSame (result, data);
		}

		[Test]
		public void Step ()
		{
			var data = new int [5];
			var result = new int [] { 1, 3, 5, 7, 9 };

			int j = 0;
			1.Step (9, 2).ForEach (i => data [j++] = i);

			AssertAreSame (result, data);
		}

		[Test]
		public void IsEven ()
		{
			Assert.IsTrue (2.IsEven ());
			Assert.IsFalse (3.IsEven ());
		}

		[Test]
		public void IsOdd ()
		{
			Assert.IsFalse (2.IsOdd ());
			Assert.IsTrue (3.IsOdd ());
		}
	}
}
