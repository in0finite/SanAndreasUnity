//
// IEquatableContract.cs
//
// Author:
//   Jonathan Pryor  <jpryor@novell.com>
//
// Copyright (c) 2009 Novell, Inc. (http://www.novell.com)
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

namespace Cadenza.Tests {

	public abstract class EquatableContract<T> : BaseRocksFixture
		where T : IEquatable<T>
	{
		protected abstract T CreateValueX ();
		protected abstract T CreateValueY ();
		protected abstract T CreateValueZ ();

		[Test]
		public void Equals_CompareToObjectNull ()
		{
			var x = CreateValueX ();
			Assert.IsFalse (x.Equals ((object) null));
		}

		[Test]
		public void Equals_CompareToValueNull ()
		{
			// This test only makes sense for reference types.
			if (typeof (T).IsValueType)
				return;
			var x = CreateValueX ();
			T value = default (T);
			Assert.IsFalse (x.Equals (value));
		}

		[Test]
		public void Equals_Reflexive ()
		{
			var x = CreateValueX ();
			Assert.IsTrue (x.Equals (x));
		}

		[Test]
		public void Equals_Symmetric ()
		{
			var x = CreateValueX ();
			var y = CreateValueX ();

			Assert.IsTrue (x.Equals (y));
			Assert.IsTrue (y.Equals (x));

			y = CreateValueY ();
			Assert.IsFalse (x.Equals (y));
			Assert.IsFalse (y.Equals (x));
		}

		[Test]
		public void Equals_Transitive ()
		{
			var x = CreateValueX ();
			var y = CreateValueX ();
			var z = CreateValueX ();

			Assert.IsTrue (x.Equals (y));
			Assert.IsTrue (y.Equals (z));
			Assert.IsTrue (x.Equals (z));

			y = CreateValueY ();
			Assert.IsFalse (x.Equals (y));
			Assert.IsFalse (y.Equals (z));
			Assert.IsTrue (x.Equals (z));

			z = CreateValueZ ();
			Assert.IsFalse (x.Equals (y));
			Assert.IsFalse (y.Equals (z));
			Assert.IsFalse (x.Equals (z));
		}
	}
}
