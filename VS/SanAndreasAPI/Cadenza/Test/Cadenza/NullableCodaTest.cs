//
// NullableTest.cs
//
// Author:
//   Jonathan Pryor
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

using NUnit.Framework;

using Cadenza;

namespace Cadenza.Tests {

	[TestFixture]
	public class NullableTest : BaseRocksFixture {

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void Just_SelfNull ()
		{
			int? n = null;
			n.Just ();
		}

		[Test]
		public void Just ()
		{
			Assert.AreEqual (typeof(Maybe<int>),
					42.Just ().GetType ());
			Assert.AreEqual ("42",
					42.Just ().ToString ());
			Assert.IsTrue (
					42.Just ().HasValue);
			Assert.AreEqual (42,
					42.Just ().Value);
		}

		[Test]
		public void ToMaybe ()
		{
			int?       n = null;
			Maybe<int> m = n.ToMaybe ();
			Assert.AreEqual (Maybe<int>.Nothing, m);
			Assert.IsFalse (m.HasValue);

			n = 42;
			m = n.ToMaybe ();
			Assert.AreEqual (42.Just (), m);
			Assert.IsTrue (m.HasValue);
			Assert.AreEqual (42, m.Value);
		}
	}
}
