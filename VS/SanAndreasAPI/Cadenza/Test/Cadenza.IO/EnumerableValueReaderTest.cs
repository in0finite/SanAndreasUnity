//
// EnumerableValueReaderTest.cs
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
using System.Collections;
using System.Collections.Generic;
using System.IO;

using NUnit.Framework;

using Cadenza.Collections;
using Cadenza.IO;
using Cadenza.Tests;

namespace Cadenza.IO.Tests {

	[TestFixture]
	public class EnumerableValueReaderTest : BaseRocksFixture {

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void EnumerableValueReader_ValuesNull ()
		{
			IEnumerable<int> values = null;
			new EnumerableValueReader<int> (values);
		}

		[Test, ExpectedException (typeof (InvalidCastException))]
		public void Read_UnsupportedT ()
		{
			int n;
			new EnumerableValueReader<DateTime> (new[]{DateTime.Now})
				.Read (out n);
		}

		[Test, ExpectedException (typeof (InvalidOperationException))]
		public void Read_PastEnd ()
		{
			char a, b;
			new[]{1}.ToValueReader ().Read (out a).Read (out b);
		}

		class Foo : IEnumerable<int> {
			public bool WasDisposed;

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return GetEnumerator ();
			}

			public IEnumerator<int> GetEnumerator ()
			{
				try {
					yield return 0;
				}
				finally {
					WasDisposed = true;
				}
			}
		}

		[Test]
		public void Read_EnumeratorDisposed ()
		{
			Foo f = new Foo ();
			using (var r = f.ToValueReader ()) {
				char a;
				r.Read (out a);
			}
			Assert.IsTrue (f.WasDisposed);
		}

		[Test]
		public void Reads ()
		{
			#region Overview
			short a;
			char b;
			decimal c;
			new EnumerableValueReader<int> (new[]{1, 2, 3})
				.Read (out a).Read (out b).Read (out c);
			Assert.AreEqual ((short) 1, a);
			Assert.AreEqual ((char) 2,  b);
			Assert.AreEqual (3m,        c);
			#endregion
		}
	}
}
