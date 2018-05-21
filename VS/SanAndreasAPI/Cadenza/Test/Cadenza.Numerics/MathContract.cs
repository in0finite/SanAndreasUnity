//
// MathContract.cs
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
using System.Linq;

using NUnit.Framework;

using Cadenza;
using Cadenza.Tests;

namespace Cadenza.Numerics.Tests {

	public class MathContract<T> : BaseRocksFixture {

		[Test]
		public void LessThan ()
		{
			var m = Math<T>.Default;
			Assert.IsTrue (m.LessThan (m.FromInt32 (3), m.FromInt32 (4)));
			Assert.IsFalse (m.LessThan (m.FromInt32 (3), m.FromInt32 (3)));
			Assert.IsFalse (m.LessThan (m.FromInt32 (4), m.FromInt32 (3)));
		}

		[Test]
		public void LessThanOrEqual ()
		{
			var m = Math<T>.Default;
			Assert.IsTrue (m.LessThanOrEqual (m.FromInt32 (3), m.FromInt32 (4)));
			Assert.IsTrue (m.LessThanOrEqual (m.FromInt32 (3), m.FromInt32 (3)));
			Assert.IsFalse (m.LessThanOrEqual (m.FromInt32 (4), m.FromInt32 (3)));
		}

		[Test]
		public void GreaterThan ()
		{
			var m = Math<T>.Default;
			Assert.IsFalse (m.GreaterThan (m.FromInt32 (3), m.FromInt32 (4)));
			Assert.IsFalse (m.GreaterThan (m.FromInt32 (3), m.FromInt32 (3)));
			Assert.IsTrue (m.GreaterThan (m.FromInt32 (4), m.FromInt32 (3)));
		}

		[Test]
		public void GreaterThanOrEqual ()
		{
			var m = Math<T>.Default;
			Assert.IsFalse (m.GreaterThanOrEqual (m.FromInt32 (3), m.FromInt32 (4)));
			Assert.IsTrue (m.GreaterThanOrEqual (m.FromInt32 (3), m.FromInt32 (3)));
			Assert.IsTrue (m.GreaterThanOrEqual (m.FromInt32 (4), m.FromInt32 (3)));
		}

		[Test]
		public void Max ()
		{
			var m = Math<T>.Default;
			Assert.AreEqual (m.FromInt32 (3), m.Max (m.FromInt32 (2), m.FromInt32 (3)));
		}

		[Test]
		public void Min ()
		{
			var m = Math<T>.Default;
			Assert.AreEqual (m.FromInt32 (2), m.Min (m.FromInt32 (2), m.FromInt32 (3)));
		}

		[Test]
		public void Successor ()
		{
			var m = Math<T>.Default;
			Assert.AreEqual (m.FromInt32 (1), m.Successor (m.FromInt32 (0)));
			try {
				var max = m.MaxValue;
				try {
					m.Successor (max);
					Assert.IsTrue (m.IsFloatingPoint);
				}
				catch (OverflowException) {
					Assert.IsFalse (m.IsFloatingPoint);
				}
			}
			catch (NotSupportedException) {
				Assert.IsFalse (m.HasBounds);
			}
		}

		[Test]
		public void Predecessor ()
		{
			var m = Math<T>.Default;
			Assert.AreEqual (m.FromInt32 (0), m.Predecessor (m.FromInt32 (1)));
			try {
				var min = m.MinValue;
				try {
					m.Predecessor (min);
					Assert.IsTrue (m.IsFloatingPoint);
				}
				catch (OverflowException) {
					Assert.IsFalse (m.IsFloatingPoint);
				}
			}
			catch (NotSupportedException) {
				Assert.IsFalse (m.HasBounds);
			}
		}

		[Test]
		public void ToInt32 ()
		{
			var m = Math<T>.Default;
			Assert.AreEqual (5, m.ToInt32 (m.FromInt32 (5)));
		}

		[Test]
		public void EnumerateFrom ()
		{
			var m = Math<T>.Default;
			AssertAreSame (
					new [] { 3, 4, 5 },
					m.EnumerateFrom (m.FromInt32 (3)).Take (3).Select (v => m.ToInt32 (v)));
		}

		[Test]
		public void EnumerateFromThen ()
		{
			var m = Math<T>.Default;
			AssertAreSame (
					new [] { 0, 3, 4, 5 },
					m.EnumerateFromThen (m.FromInt32 (0), m.FromInt32 (3)).Take (4).Select (v => m.ToInt32 (v)));
		}

		[Test]
		public void EnumerateFromTo ()
		{
			var m = Math<T>.Default;
			Assert.Throws<ArgumentException>(() => m.EnumerateFromTo (m.FromInt32 (5), m.FromInt32 (2)));
			AssertAreSame (
					new [] { 2, 3, 4, 5 },
					m.EnumerateFromTo (m.FromInt32 (2), m.FromInt32 (5)).Select (v => m.ToInt32 (v)));
		}

		[Test]
		public void EnumerateFromThenTo ()
		{
			var m = Math<T>.Default;
			Assert.Throws<ArgumentException>(() => m.EnumerateFromThenTo (m.FromInt32 (10), m.FromInt32 (5), m.FromInt32 (2)));
			AssertAreSame (
					new [] { 0, 2, 3, 4, 5 },
					m.EnumerateFromThenTo (m.FromInt32 (0), m.FromInt32 (2), m.FromInt32 (5)).Select (v => m.ToInt32 (v)));
		}

		[Test]
		public void Add ()
		{
			var m = Math<T>.Default;
			Assert.AreEqual (m.FromInt32 (3), m.Add (m.FromInt32 (1), m.FromInt32 (2)));
			Assert.AreEqual (m.FromInt32 (3), m.Add (m.FromInt32 (2), m.FromInt32 (1)));
			Assert.AreEqual (m.FromInt32 (2), m.Add (m.FromInt32 (2), m.FromInt32 (0)));
			Assert.AreEqual (m.FromInt32 (2), m.Add (m.FromInt32 (0), m.FromInt32 (2)));
			try {
				var max = m.MaxValue;
				var max1 = m.Add (max, max);
				// If we're here, then T should be a floating point type
				Assert.IsTrue (m.IsFloatingPoint);
				Assert.IsTrue (m.IsInfinite (max1));
			}
			catch (OverflowException) {
				// thrown if m.MaxValue+1 can't be represented
				Assert.IsFalse (m.IsFloatingPoint);
			}
			catch (NotSupportedException) {
				// thrown if m.MaxValue doesn't exist
				Assert.IsFalse (m.HasBounds);
			}
		}

		[Test]
		public void Multiply ()
		{
			var m = Math<T>.Default;
			Assert.AreEqual (m.FromInt32 (1), m.Multiply (m.FromInt32 (1), m.FromInt32 (1)));
			Assert.AreEqual (m.FromInt32 (0), m.Multiply (m.FromInt32 (1), m.FromInt32 (0)));
			Assert.AreEqual (m.FromInt32 (0), m.Multiply (m.FromInt32 (0), m.FromInt32 (0)));
			Assert.AreEqual (m.FromInt32 (0), m.Multiply (m.FromInt32 (0), m.FromInt32 (1)));
			Assert.AreEqual (m.FromInt32 (6), m.Multiply (m.FromInt32 (2), m.FromInt32 (3)));

			try {
				var max = m.MaxValue;
				var max1 = m.Multiply (max, max);
				// If we're here, then T should be a floating point type
				Assert.IsTrue (m.IsFloatingPoint);
				Assert.IsTrue (m.IsInfinite (max1));
			}
			catch (OverflowException) {
				// thrown if m.MaxValue+1 can't be represented
				Assert.IsFalse (m.IsFloatingPoint);
			}
			catch (NotSupportedException) {
				// thrown if m.MaxValue doesn't exist
				Assert.IsFalse (m.HasBounds);
			}
		}

		[Test]
		public void Subtract ()
		{
			var m = Math<T>.Default;
			Assert.AreEqual (m.FromInt32 (1), m.Subtract (m.FromInt32 (3), m.FromInt32 (2)));
			try {
				var min = m.MinValue;
				m.Subtract (min, m.FromInt32 (1));
				// If we're here, then T should be a floating point type
				Assert.IsTrue (m.IsFloatingPoint);
				var ninf = m.Subtract (min, m.MaxValue);
				Assert.IsTrue (m.IsInfinite (ninf));
			}
			catch (OverflowException) {
				// thrown if m.MaxValue-1 can't be represented
				Assert.IsFalse (m.IsFloatingPoint);
			}
			catch (NotSupportedException) {
				// thrown if m.MaxValue doesn't exist
				Assert.IsFalse (m.HasBounds);
			}
		}

		[Test]
		public void Negate ()
		{
			var m = Math<T>.Default;
			try {
				Assert.AreEqual (m.FromInt32 (-1), m.Negate (m.FromInt32 (1)));
			}
			catch (OverflowException) {
				// likely unsigned values.
				Assert.IsTrue (m.IsUnsigned);
			}

			try {
				var min = m.MinValue;
				m.Negate (min);
				Assert.IsFalse (m.IsTwosComplement);
			}
			catch (OverflowException) {
				// in 2's complement, `-int.MinValue` can't be held in an int.
				Assert.IsTrue (m.IsTwosComplement);
			}
			catch (NotSupportedException) {
				// no MinValue
				Assert.IsFalse (m.HasBounds);
			}
		}

		[Test]
		public void Abs ()
		{
			var m = Math<T>.Default;

			Assert.AreEqual (m.FromInt32 (2), m.Abs (m.FromInt32 (2)));
			try {
				Assert.AreEqual (m.FromInt32 (2), m.Abs (m.FromInt32 (-2)));
			}
			catch (OverflowException) {
				// unsigned type
				Assert.IsTrue (m.IsUnsigned);
			}
		}

		[Test]
		public void Sign ()
		{
			var m = Math<T>.Default;

			Assert.AreEqual (m.FromInt32 (1), m.Sign (m.FromInt32 (3)));
			Assert.AreEqual (m.FromInt32 (1), m.Sign (m.FromInt32 (2)));
			Assert.AreEqual (m.FromInt32 (1), m.Sign (m.FromInt32 (1)));
			Assert.AreEqual (m.FromInt32 (0), m.Sign (m.FromInt32 (0)));

			var n = m.FromInt32 (2);
			Assert.AreEqual (n, m.Multiply (m.Abs (n), m.Sign (n)));

			n = m.FromInt32 (0);
			Assert.AreEqual (n, m.Multiply (m.Abs (n), m.Sign (n)));

			try {
				Assert.AreEqual (m.FromInt32 (-1), m.Sign (m.FromInt32 (-1)));
				Assert.AreEqual (m.FromInt32 (-1), m.Sign (m.FromInt32 (-2)));
				Assert.AreEqual (m.FromInt32 (-1), m.Sign (m.FromInt32 (-3)));

				n = m.FromInt32 (-2);
				Assert.AreEqual (n, m.Multiply (m.Abs (n), m.Sign (n)));
			}
			catch (OverflowException) {
				// unsigned
				Assert.IsTrue (m.IsUnsigned);
			}
		}

		[Test]
		public void Quotient ()
		{
			// "integer division truncating toward zero"
			var m = Math<T>.Default;

			Assert.AreEqual (m.FromInt32 (2), m.Quotient (m.FromInt32 (5), m.FromInt32 (2)));
			try {
				// -5/2 == -2.5; truncate toward 0 == -2
				Assert.AreEqual (m.FromInt32 (-2), m.Quotient (m.FromInt32 (-5), m.FromInt32 (2)));
			}
			catch (OverflowException) {
				Assert.IsTrue (m.IsUnsigned);
			}

			try {
				var d = m.Quotient (m.FromInt32 (2), m.FromInt32 (0));
				Assert.IsTrue (m.IsFloatingPoint);
				Assert.IsTrue (m.IsInfinite (d));
			}
			catch (DivideByZeroException) {
				Assert.IsFalse (m.IsFloatingPoint);
			}
		}

		[Test]
		public void Remainder ()
		{
			var m = Math<T>.Default;

			var x = m.FromInt32 (5);
			var y = m.FromInt32 (2);

			var r = m.Remainder (x, y);
			Assert.AreEqual (m.FromInt32 (1), r);

			// Must satisfy: (x `quote` y)*y + (x `rem` y) == x
			var q = m.Quotient (x, y);
			Assert.AreEqual (x, m.Add (m.Multiply (q, y), r));

			try {
				x = m.FromInt32 (-5);
				q = m.Quotient (x, y);
				r = m.Remainder (x, y);
				Assert.AreEqual (m.FromInt32 (-1), r);
				Assert.AreEqual (x, m.Add (m.Multiply (q, y), r));
			}
			catch (OverflowException) {
				Assert.IsTrue (m.IsUnsigned);
			}

			try {
				var d = m.Remainder (m.FromInt32 (2), m.FromInt32 (0));
				Assert.IsTrue (m.IsFloatingPoint);
				Assert.IsTrue (m.IsNaN (d));
			}
			catch (DivideByZeroException) {
				Assert.IsFalse (m.IsFloatingPoint);
			}
		}

		[Test]
		public void DivideIntegral ()
		{
			// "integer division truncating toward negative infinity"
			var m = Math<T>.Default;

			var d = m.DivideIntegral (m.FromInt32 (5), m.FromInt32 (2));
			Assert.AreEqual (m.FromInt32 (2), d);
			try {
				// -5/2 == -2.5; truncate toward -inf == -3
				Assert.AreEqual (m.FromInt32 (-3), m.DivideIntegral (m.FromInt32 (-5), m.FromInt32 (2)));
			}
			catch (OverflowException) {
				Assert.IsTrue (m.IsUnsigned);
			}

			try {
				d = m.DivideIntegral (m.FromInt32 (5), m.FromInt32 (0));
				Assert.IsTrue (m.IsFloatingPoint);
				Assert.IsTrue (m.IsInfinite (d));
			}
			catch (DivideByZeroException) {
				Assert.IsFalse (m.IsFloatingPoint);
			}
		}

		[Test]
		public void Modulus ()
		{
			var m = Math<T>.Default;

			var x = m.FromInt32 (5);
			var y = m.FromInt32 (2);

			var mod = m.Modulus (x, y);
			Assert.AreEqual (m.FromInt32 (1), mod);

			// Must satisfy: (x `div` y)*y + (x `mod` y) == x
			var div = m.DivideIntegral (x, y);
			Assert.AreEqual (x, m.Add (m.Multiply (div, y), mod));

			try {
				x = m.FromInt32 (-5);
				div = m.DivideIntegral (x, y);
				mod = m.Modulus (x, y);
				Assert.AreEqual (m.FromInt32 (1), mod);
				Assert.AreEqual (x, m.Add (m.Multiply (div, y), mod));
			}
			catch (OverflowException) {
				Assert.IsTrue (m.IsUnsigned);
			}

			try {
				div = m.Modulus (m.FromInt32 (5), m.FromInt32 (0));
				Assert.IsTrue (m.IsFloatingPoint);
				Assert.IsTrue (m.IsNaN (div));
			}
			catch (DivideByZeroException) {
				Assert.IsFalse (m.IsFloatingPoint);
			}
		}

		[Test]
		public void QuotientRemainder ()
		{
			// "integer division truncating toward zero"
			var m = Math<T>.Default;

			T q, r;
			var x = m.FromInt32 (5);
			var y = m.FromInt32 (2);

			q = m.QuotientRemainder (x, y, out r);
			Assert.AreEqual (m.FromInt32 (2), q);
			Assert.AreEqual (m.FromInt32 (1), r);

			// Must satisfy: (x `quote` y)*y + (x `rem` y) == x
			Assert.AreEqual (x, m.Add (m.Multiply (q, y), r));


			try {
				x = m.FromInt32 (-5);

				q = m.QuotientRemainder (x, y, out r);
				Assert.AreEqual (m.FromInt32 (-2), q);
				Assert.AreEqual (m.FromInt32 (-1), r);

				Assert.AreEqual (x, m.Add (m.Multiply (q, y), r));
			}
			catch (OverflowException) {
				Assert.IsTrue (m.IsUnsigned);
			}

			try {
				q = m.QuotientRemainder(m.FromInt32 (5), m.FromInt32 (0), out r);
				Assert.IsTrue (m.IsFloatingPoint);
				Assert.IsTrue (m.IsInfinite (q));
				Assert.IsTrue (m.IsNaN (r));
			}
			catch (DivideByZeroException) {
				Assert.IsFalse (m.IsFloatingPoint);
			}
		}

		[Test]
		public void DivideIntegralModulus ()
		{
			// "integer division truncating toward negative infinity"
			var m = Math<T>.Default;

			T div, mod;
			var x = m.FromInt32 (5);
			var y = m.FromInt32 (2);

			div = m.DivideIntegralModulus (x, y, out mod);
			Assert.AreEqual (m.FromInt32 (2), div);
			Assert.AreEqual (m.FromInt32 (1), mod);
			Assert.AreEqual (x, m.Add (m.Multiply (div, y), mod));

			try {
				// -5/2 == -2.5; truncate toward -inf == -3
				x = m.FromInt32 (-5);
				div = m.DivideIntegralModulus (x, y, out mod);

				Assert.AreEqual (m.FromInt32 (-3), div);
				Assert.AreEqual (m.FromInt32 (1), mod);
				Assert.AreEqual (x, m.Add (m.Multiply (div, y), mod));
			}
			catch (OverflowException) {
				Assert.IsTrue (m.IsUnsigned);
			}

			try {
				div = m.DivideIntegralModulus (m.FromInt32 (5), m.FromInt32 (0), out mod);
				Assert.IsTrue (m.IsFloatingPoint);
				Assert.IsTrue (m.IsInfinite (div));
				Assert.IsTrue (m.IsNaN (mod));
			}
			catch (DivideByZeroException) {
				Assert.IsFalse (m.IsFloatingPoint);
			}
		}

		[Test]
		public void ToIConvertible ()
		{
			var m = Math<T>.Default;

			var v = m.FromInt32 (1);
			try {
				var c = m.ToIConvertible (v);
				Assert.IsNotNull (c);
				var e = c as IEquatable<T>;
				if (e != null)
					// Type opted into value equality contract
					Assert.AreEqual (v, e);
			}
			catch (NotSupportedException) {
				// default implementation
			}
		}

		[Test]
		public void Divide ()
		{
			var m = Math<T>.Default;

			Assert.AreEqual (m.FromInt32 (3), m.Divide (m.FromInt32 (6), m.FromInt32 (2)));

			var r = m.Divide (m.FromInt32 (5), m.FromInt32 (2));
			if (m.IsFractional)
				Assert.AreEqual (m.FromIConvertible (2.5), r);
			else
				Assert.AreEqual (m.FromInt32 (2), r);

			try {
				r = m.Divide (m.FromInt32 (5), m.FromInt32 (0));
				Assert.IsTrue (m.IsFloatingPoint);
				Assert.IsTrue (m.IsInfinite (r));
			}
			catch (DivideByZeroException) {
				Assert.IsFalse (m.IsFloatingPoint);
			}
		}

		[Test]
		public void Reciprocal ()
		{
			var m = Math<T>.Default;

			var r = m.Reciprocal (m.FromInt32 (2));
			if (m.IsFractional)
				Assert.AreEqual (m.FromIConvertible (0.5), r);
			else
				Assert.AreEqual (m.FromInt32 (0), r);

			try {
				r = m.Reciprocal (m.FromInt32 (0));
				Assert.IsTrue (m.IsFloatingPoint);
				Assert.IsTrue (m.IsInfinite (r));
			}
			catch (DivideByZeroException) {
				Assert.IsFalse (m.IsFloatingPoint);
			}
		}

		[Test]
		public void Exp ()
		{
			var m = Math<T>.Default;

			TryUnlessConversionFromDoubleNotSupported (() => {
				Assert.AreEqual (m.FromInt32 (1), m.Exp (m.FromInt32 (0)));
				Assert.AreEqual (m.E, m.Exp (m.FromInt32 (1)));
			});
		}

		static void TryUnlessConversionFromDoubleNotSupported (Action action)
		{
			try {
				action ();
			}
			catch (InvalidCastException) {
				// Type doesn't support conversion to double
				Assert.Throws<InvalidCastException>(() => Math<T>.Default.FromIConvertible (1.0));
			}
			catch (NotSupportedException) {
				// Type doesn't support conversion to IConvertible
				var m = Math<T>.Default;
				Assert.Throws<NotSupportedException>(() => m.ToIConvertible (m.FromInt32 (0)));
			}
		}

		[Test]
		public void Sqrt ()
		{
			var m = Math<T>.Default;

			TryUnlessConversionFromDoubleNotSupported (() => {
				Assert.AreEqual (m.FromInt32 (0), m.Sqrt (m.FromInt32 (0)));
				Assert.AreEqual (m.FromInt32 (1), m.Sqrt (m.FromInt32 (1)));
				Assert.AreEqual (m.FromInt32 (2), m.Sqrt (m.FromInt32 (4)));
				Assert.AreEqual (m.FromInt32 (3), m.Sqrt (m.FromInt32 (9)));
			});

			CheckSqrtNegative1 ();
		}

		void CheckSqrtNegative1 ()
		{
			var m = Math<T>.Default;

			T neg1;
			try {
				neg1 = m.FromInt32 (-1);
			}
			catch (OverflowException) {
				// Unsigned types don't support negative values.
				Assert.IsTrue (m.IsUnsigned);
				return;
			}

			try {
				T sqrtNeg1 = m.Sqrt (neg1);
				Assert.IsTrue (m.IsFloatingPoint);
				Assert.IsTrue (m.IsNaN (sqrtNeg1));
			}
			catch (NotSupportedException) {
				// or doesn't support conversion to IConvertible
				Assert.Throws<NotSupportedException>(() => m.ToIConvertible (m.FromInt32 (0)));
			}
			catch (OverflowException) {
				// NaN->int conversion throws under .NET
				Assert.IsFalse (m.IsFloatingPoint);
				return;
			}
			catch (InvalidCastException) {
				// Could not perform double->T conversion?
				Assert.Throws<InvalidCastException>(() => m.FromIConvertible (1.0));
				return;
			}
		}

		[Test]
		public void Log ()
		{
			var m = Math<T>.Default;

			TryUnlessConversionFromDoubleNotSupported(() => {
				Assert.AreEqual (m.FromInt32 (0), m.Log (m.FromInt32 (1)));

				try {
					Assert.IsTrue (m.IsInfinite (m.Log (m.FromInt32 (0))));
					Assert.IsTrue (m.IsFloatingPoint);
				}
				catch (OverflowException) {
					Assert.IsFalse (m.IsFloatingPoint);
				}
			});


			try {
				Assert.IsTrue (m.IsNaN (m.Log (m.FromInt32 (-1))));
				Assert.IsTrue (m.IsFloatingPoint);
			}
			catch (OverflowException) {
				Assert.IsFalse (m.IsFloatingPoint);
			}
			catch (NotSupportedException) {
				// Unsigned type
				if (!m.IsUnsigned) {
					// or doesn't support conversion to IConvertible
					Assert.Throws<NotSupportedException>(() => m.ToIConvertible (m.FromInt32 (0)));
				}
			}
			catch (InvalidCastException) {
				Assert.Throws<InvalidCastException>(() => m.FromIConvertible (1.0));
			}
		}
	}
}
