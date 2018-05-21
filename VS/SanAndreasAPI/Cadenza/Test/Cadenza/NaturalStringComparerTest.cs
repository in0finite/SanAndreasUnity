//
// NaturalStringComparerTest.cs
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
	public class NaturalStringComparerTest : BaseRocksFixture {

		[Test]
		public void Compare ()
		{
			string[] expected = {
				"a.1.b.2.c.3.d.4.e.5.f.6.g.7.h.8.i.9.j.10.k.11",
				"a.1.b.2.c.3.d.4.e.5.f.6.g.7.h.8.i.9.j.10.k.12",
				"bar",
				"foo",
				"foo",
				"foo1",
				"foo2",
				"foo3",
				"foo4",
				"foo5",
				"foo6",
				"foo7",
				"foo8",
				"foo9",
				"foo10",
			};

			List<string> actual = new List<string> {
				"foo",
				"foo",
				"foo10",
				"foo1",
				"foo4",
				"foo2",
				"foo3",
				"foo9",
				"foo5",
				"foo7",
				"foo8",
				"foo6",
				"bar",
				"a.1.b.2.c.3.d.4.e.5.f.6.g.7.h.8.i.9.j.10.k.12",
				"a.1.b.2.c.3.d.4.e.5.f.6.g.7.h.8.i.9.j.10.k.11",
			};
			actual.Sort (NaturalStringComparer.Default);

			AssertAreSame (expected, actual);
		}
	}
}
