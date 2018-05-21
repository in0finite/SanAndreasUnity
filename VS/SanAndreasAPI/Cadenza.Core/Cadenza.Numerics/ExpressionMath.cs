//
// ExpressionMathProvider.cs
//
// Author:
//   Jonathan Pryor <jpryor@novell.com>
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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Cadenza;

namespace Cadenza.Numerics {

	// from: http://www.yoda.arachsys.com/csharp/genericoperators.html
	public class ExpressionMath<T> : Math<T>
	{
		readonly Func<T, T, T> add    = CreateBinaryExpression<T> ((a, b) => Expression.AddChecked (a, b));
		readonly Func<T, T, T> sub    = CreateBinaryExpression<T> ((a, b) =>Expression.SubtractChecked (a, b));
		readonly Func<T, T, T> divide = CreateBinaryExpression<T> ((a, b) => Expression.Divide (a, b));
		readonly Func<T, T, T> mod    = CreateBinaryExpression<T> ((a, b) => Expression.Modulo (a, b));
		readonly Func<T, T, T> mult   = CreateBinaryExpression<T> ((a, b) => Expression.MultiplyChecked (a, b));
		readonly Func<T, T, T> pow    = CreateBinaryExpression<T>((a, b) => Expression.Power (a, b));
		readonly Func<T, T> negate    = CreateUnaryExpression<T, T> (v => Expression.NegateChecked (v));
		readonly Func<T, T, bool> eq  = CreateBinaryExpression<bool> ((a, b) => Expression.Equal (a, b));
		readonly Func<T, T, bool> gt  = CreateBinaryExpression<bool> ((a, b) => Expression.GreaterThan (a, b));
		readonly Func<T, T, bool> gte = CreateBinaryExpression<bool> ((a, b) => Expression.GreaterThanOrEqual (a, b));
		readonly Func<T, T, bool> lt  = CreateBinaryExpression<bool> ((a, b) => Expression.LessThan (a, b));
		readonly Func<T, T, bool> lte = CreateBinaryExpression<bool> ((a, b) => Expression.LessThanOrEqual (a, b));
		readonly Func<T, int> toInt32 = CreateUnaryExpression<T, int> (v => Expression.ConvertChecked (v, typeof (int)));
		readonly Func<int, T> fromInt32 = CreateUnaryExpression<int, T> (v => Expression.ConvertChecked (v, typeof (T)));

		readonly bool isFractional;
		readonly bool haveBounds;
		readonly T maxValue, minValue;
		readonly bool canBeInfinite;
		readonly T negInf, posInf, nan;
		readonly bool twosComplement;
		readonly bool unsigned;

		public ExpressionMath ()
		{
			if (divide != null) {
				try {
					T zero = FromInt32 (0);
					T one  = FromInt32 (1);
					T two  = FromInt32 (2);

					// (1/2) == 0.5; if 0.5 == 0, then it's an integral type
					if (!EqualityComparer<T>.Default.Equals (zero, divide (one, two)))
						isFractional = true;
				}
				catch {
					// ignore
				}
			}

			haveBounds = GetMemberValue ("MaxValue", out maxValue) && GetMemberValue ("MinValue", out minValue);
			canBeInfinite = GetMemberValue ("NegativeInfinity", out negInf) && GetMemberValue ("PositiveInfinity", out posInf) &&
				GetMemberValue ("NaN", out nan);
			if (negate != null && haveBounds) {
				try {
					negate (minValue);
				}
				catch (OverflowException) {
					twosComplement = true;
				}
				catch (Exception) {
					// ignore
				}
			}
			unsigned = haveBounds && EqualityComparer<T>.Default.Equals (minValue, FromInt32 (0));
		}

		static bool GetMemberValue (string name, out T value)
		{
			const BindingFlags flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase;

			try {
				var f = typeof (T).GetField (name, flags);
				if (f != null) {
					value = (T) f.GetValue (null);
					return true;
				}
			}
			catch {
				// ignore
			}

			try {
				var p = typeof (T).GetProperty (name, flags);
				if (p != null) {
					value = (T) p.GetValue (null, null);
					return true;
				}
			}
			catch {
				// ignore
			}

			value = default (T);
			return false;
		}

		static Func<T, T, TRet> CreateBinaryExpression<TRet> (Func<ParameterExpression, ParameterExpression, BinaryExpression> operation)
		{
			var a = Expression.Parameter (typeof (T), "a");
			var b = Expression.Parameter (typeof (T), "b");
			try {
				var body = operation (a, b);
				return Expression.Lambda<Func<T, T, TRet>> (body, a, b).Compile ();
			}
			catch {
				// operation not supported.
				return null;
			}
		}

		static Func<T1, TRet> CreateUnaryExpression<T1, TRet> (Func<ParameterExpression, UnaryExpression> operation)
		{
			var value = Expression.Parameter (typeof (T1), "value");
			try {
				var body = operation (value);
				return Expression.Lambda<Func<T1, TRet>> (body, value).Compile ();
			}
			catch {
				// operation not supported.
				return null;
			}
		}

		public override bool IsUnsigned {
			get {return unsigned;}
		}

		public override bool IsTwosComplement {
			get {return twosComplement;}
		}

		public override bool IsFractional {
			get {return isFractional;}
		}

		public override bool IsFloatingPoint {
			get {return canBeInfinite;}
		}

		public override T FromInt32 (int value)
		{
			if (fromInt32 != null)
				return fromInt32 (value);
			return base.FromInt32 (value);
		}

		public override int ToInt32 (T value)
		{
			if (toInt32 != null)
				return toInt32 (value);
			return base.ToInt32 (value);
		}

		public override bool Equals (T x, T y)
		{
			if (eq == null)
				return base.Equals (x, y);
			return eq (x, y);
		}

		public override bool LessThan (T x, T y)
		{
			if (lt == null)
				return base.LessThan (x, y);
			return lt (x, y);
		}

		public override bool LessThanOrEqual (T x, T y)
		{
			if (lte == null)
				return base.LessThan (x, y);
			return lte (x, y);
		}

		public override bool GreaterThan (T x, T y)
		{
			if (gt == null)
				return base.GreaterThan (x, y);
			return gt (x, y);
		}

		public override bool GreaterThanOrEqual (T x, T y)
		{
			if (gte == null)
				return base.GreaterThan (x, y);
			return gte (x, y);
		}

		public override T Add (T x, T y)
		{
			if (add == null)
				throw new NotSupportedException ();
			return add (x, y);
		}

		public override T Multiply (T x, T y)
		{
			if (mult == null)
				throw new NotSupportedException ();
			return mult (x, y);
		}

		public override T Subtract (T x, T y)
		{
			if (sub == null)
				throw new NotSupportedException ();
			return sub (x, y);
		}

		public override T Negate (T value)
		{
			if (negate == null)
				return base.Negate (value);
			return negate (value);
		}

		public override T QuotientRemainder (T x, T y, out T remainder)
		{
			if (mod == null)
				throw new NotSupportedException ("Need Expression.Modulus() support to implement Math<T>.QuotientRemainder().");
			if (divide == null)
				throw new NotSupportedException ("Need Expression.Divide() support to implement Math<T>.QuotientRemainder().");
			remainder = mod (x, y);
			T d = divide (x, y);
			if (IsInfinite (d) || IsNaN (d))
				return d;
			return FromInt32 (ToInt32 (d));
		}

		public override T Pow (T x, T y)
		{
			if (pow == null)
				return base.Pow (x, y);
			return pow (x, y);
		}

		public override T Divide (T x, T y)
		{
			if (divide == null)
				throw new NotSupportedException ();
			return divide (x, y);
		}

		public override bool HasBounds {
			get {return haveBounds;}
		}

		public override T MaxValue {
			get {return maxValue;}
		}

		public override T MinValue {
			get {return minValue;}
		}

		public override bool IsInfinite (T value)
		{
			if (!canBeInfinite)
				return base.IsInfinite (value);
			return negInf.Equals (value) || posInf.Equals (value);
		}

		public override bool IsNaN (T value)
		{
			if (!canBeInfinite)
				return base.IsInfinite (value);
			return nan.Equals (value);
		}
	}
}
