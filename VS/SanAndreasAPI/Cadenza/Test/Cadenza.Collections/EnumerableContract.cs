//
// IEnumerableContract.cs
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

	public abstract class EnumerableContract : BaseRocksFixture {

		protected abstract IEnumerable<T> CreateSequence<T> (IEnumerable<T> source);

		protected class DisposedCounter {
			public int Disposed;
			public IEnumerable<int> Values (int max)
			{
				int v = 0;
				try {
					for (int i = 0; i < max; ++i)
						yield return v++;
				}
				finally {
					Disposed++;
				}
			}
		}

		[Test]
		public void Create_SequencEqual ()
		{
			var a = new List<int> ();
			for (int i = 0; i < 10; ++i) {
				a.Add (i);
				Assert.IsTrue (CreateSequence (a).SequenceEqual (a), "Count=" + (i+1));
			}
		}

		[Test]
		public void GetEnumerator_DisposeDisposesIterator ()
		{
			var d = new DisposedCounter ();
			var s = CreateSequence (d.Values (2));
			var i = s.GetEnumerator ();
			Assert.IsTrue (i.MoveNext ());
			i.Dispose ();
			Assert.AreEqual (1, d.Disposed);
		}
	}
}

