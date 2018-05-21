//
// LambdaTest.cs
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
using System.Linq.Expressions;

using NUnit.Framework;

using Cadenza;

namespace Cadenza.Tests {

	[TestFixture]
	public class LambdaTest : BaseRocksFixture {

		[Test]
		public void Expressions ()
		{
			Assert.AreEqual (typeof(Expression<Action<int, int>>),
				Lambda.XA ((int a, int b) => Console.WriteLine (a+b) ).GetType());
			Assert.AreEqual (typeof(Expression<Func<int, int, int>>),
				Lambda.XF ((int a, int b) => a+b ).GetType());
		}

		[Test]
		public void Funcs ()
		{
			Assert.AreEqual (typeof(Action<int, int>),
				Lambda.A ((int a, int b) => {} ).GetType());
			Assert.AreEqual (typeof(Func<int, int, int>),
				Lambda.F ((int a, int b) => a+b ).GetType());
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void RecFunc_P1_FuncNull ()
		{
			Lambda.RecFunc<int, int> (null);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void RecFunc_P2_FuncNull ()
		{
			Lambda.RecFunc<int, int, int> (null);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void RecFunc_P3_FuncNull ()
		{
			Lambda.RecFunc<int, int, int, int> (null);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void RecFunc_P4_FuncNull ()
		{
			Lambda.RecFunc<int, int, int, int, int> (null);
		}

		[Test]
		public void RecFuncs ()
		{
			var factorial = Lambda.RecFunc<int, int> (
					fac => x => x == 0 ? 1 : x * fac(x-1));
			Assert.AreEqual (120, factorial (5));
		}
	}
}
