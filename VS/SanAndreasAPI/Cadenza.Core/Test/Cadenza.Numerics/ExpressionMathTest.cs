//
// ExpressionMathTest.cs
//
// Author:
//   Jonathan Pryor
//
// Copyright (c) 2010 Novell, Inc. (http://www.novell.com)
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

using Cadenza.Numerics;

namespace Cadenza.Numerics.Tests
{
	public class ExpressionMathContract<T> : MathContract<T>
	{
		Math<T> current;

		[TestFixtureSetUp]
		public void TestFixtureSetUp ()
		{
			current = Math<T>.Default;
			Math<T>.SetDefault (new ExpressionMath<T> ());
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown ()
		{
			Math<T>.SetDefault (current);
		}
	}

	[TestFixture]
	public class DecimalExpressionMathTests : ExpressionMathContract<decimal> {
	}

	[TestFixture]
	public class DoubleExpressionMathTests : ExpressionMathContract<double> {
	}

	[TestFixture]
	public class SingleExpressionMathTests : ExpressionMathContract<float> {
	}

	[TestFixture]
	public class Int32ExpressionMathTests : ExpressionMathContract<int> {
	}

	[TestFixture]
	public class UInt32ExpressionMathTests : ExpressionMathContract<uint> {
	}

	[TestFixture]
	public class UInt64ExpressionMathTests : ExpressionMathContract<ulong> {
	}
}
