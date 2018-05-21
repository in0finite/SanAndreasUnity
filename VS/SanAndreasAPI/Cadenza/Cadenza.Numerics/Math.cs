//
// Math.cs
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
using System.Reflection;

using Cadenza;

namespace Cadenza.Numerics {

	//
	// Operations based on Haskell data type interfaces from:
	//    http://www.haskell.org/ghc/docs/latest/html/libraries/base-4.2.0.0/Prelude.html
	//

	// TODO: support Rational?
	// TODO: how should we support Integer?  it's a variable-sized integer type

	[Flags]
	public enum MathFeatures {
		None            = 0,        // FxDG compliance
		Bounded         = 1 << 0,   // has .MaxValue and .MinValue
		Unsigned        = 1 << 1,   // unsigned type (i.e. not int, short, etc.)
		TwosComplement  = 1 << 2,   // Has more negative values than positive values, e.g. int, NOT decimal
		Fractional      = 1 << 3,   // decimal, float, double, BigDecimal?, Rational...
		FloatingPoint   = 1 << 4,   // float, double; supports .PositiveInfinity, .IsNaN, etc.
	}

	public abstract partial class Math<T> : IComparer<T>, IEqualityComparer<T>
	{
		protected Math ()
		{
		}

		static Dictionary<Type, Type> defaultProviders = new Dictionary<Type, Type> () {
			{ typeof (decimal),   typeof (DecimalMath) },
			{ typeof (double),    typeof (DoubleMath) },
			{ typeof (float),     typeof (SingleMath) },
			{ typeof (byte),      typeof (ByteMath) },
			{ typeof (sbyte),     typeof (SByteMath) },
			{ typeof (short),     typeof (Int16Math) },
			{ typeof (ushort),    typeof (UInt16Math) },
			{ typeof (int),       typeof (Int32Math) },
			{ typeof (uint),      typeof (UInt32Math) },
			{ typeof (long),      typeof (Int64Math) },
			{ typeof (ulong),     typeof (UInt64Math) },
		};

		static Exception defaultProviderError;

		static Math ()
		{
			SetDefault (null, e => defaultProviderError = e);
		}

		static Math<T> defaultProvider;
		public static Math<T> Default {
			get {
				if (defaultProvider == null)
					throw new NotSupportedException (
							string.Format ("Could not find an implementation for '{0}'. " +
								"Try calling Cadenza.Numerics.Math<T>.SetDefault() with a Math<T> implementation.",
								typeof (T).FullName),
							defaultProviderError);
				return defaultProvider;
			}
		}

		public static void SetDefault (Math<T> provider)
		{
			if (provider != null) {
				defaultProvider       = provider;
				defaultProviderError  = null;
				return;
			}

			Type defaultType;
			if (defaultProviders.TryGetValue (typeof (T), out defaultType))
				defaultProvider = (Math<T>) Activator.CreateInstance (defaultType);
			else {
				Assembly  a     = Assembly.Load ("Cadenza.Core");
				Type      gem   = a.GetType ("Cadenza.Numerics.ExpressionMath`1");
				Type      em    = gem.MakeGenericType (typeof (T));
				defaultProvider = (Math<T>) Activator.CreateInstance (em);
			}
		}

		static void SetDefault (Math<T> provider, Action<Exception> handler)
		{
			try {
				SetDefault (provider);
			}
			catch (Exception e) {
				handler (e);
			}
		}

		public virtual MathFeatures Features {
			get {
				return 
					(HasBounds ? MathFeatures.Bounded : 0) |
					(IsUnsigned ? MathFeatures.Unsigned : 0) |
					(IsTwosComplement ? MathFeatures.TwosComplement : 0) |
					(IsFractional ? MathFeatures.Fractional : 0) |
					(IsFloatingPoint ? MathFeatures.FloatingPoint : 0);
			}
		}

		public virtual bool IsUnsigned {
			get {return false;}
		}

		public virtual bool IsTwosComplement {
			get {return false;}
		}

		public virtual bool IsFractional {
			get {return false;}
		}

		public virtual bool IsFloatingPoint {
			get {return false;}
		}

		#region IComparer<T>
		public virtual int Compare (T x, T y)
		{
			return Comparer<T>.Default.Compare (x, y);
		}
		#endregion

		#region IEqualityComparer<T>
		public virtual bool Equals (T x, T y)
		{
			return EqualityComparer<T>.Default.Equals (x, y);
		}

		public virtual int GetHashCode (T obj)
		{
			return EqualityComparer<T>.Default.GetHashCode (obj);
		}
		#endregion

		#region class Eq a => Ord a where
		public virtual bool LessThan (T x, T y)
		{
			var c = Compare (x, y);
			if (c < 0)
				return true;
			return false;
		}

		public virtual bool LessThanOrEqual (T x, T y)
		{
			var c = Compare (x, y);
			if (c <= 0)
				return true;
			return false;
		}

		public virtual bool GreaterThan (T x, T y)
		{
			var c = Compare (x, y);
			if (c > 0)
				return true;
			return false;
		}

		public virtual bool GreaterThanOrEqual (T x, T y)
		{
			var c = Compare (x, y);
			if (c >= 0)
				return true;
			return false;
		}

		public virtual T Max (T x, T y)
		{
			var c = Compare (x, y);
			return c >= 0 ? x : y;
		}

		public virtual T Min (T x, T y)
		{
			var c = Compare (x, y);
			return c <= 0 ? x : y;
		}
		#endregion class Eq a

		#region class Enum a where
		public virtual T Successor (T value)
		{
			return Add (value, FromInt32 (1));
		}

		public virtual T Predecessor (T value)
		{
			return Subtract (value, FromInt32 (1));
		}

		public virtual T FromInt32 (int value)
		{
			var r = Either.TryConvert<int, T>(value);
			return r.Fold (v => v, e => {throw e;});
		}

		public virtual int ToInt32 (T value)
		{
			var r = Either.TryConvert<T, int> (value);
			return r.Fold (v => v, e => {throw e;});
		}

		public virtual IEnumerable<T> EnumerateFrom (T start)
		{
			return Sequence.Iterate (start, v => Successor (v));
		}

		public virtual IEnumerable<T> EnumerateFromThen (T first, T start)
		{
			return new[]{first}.Concat (EnumerateFrom (start));
		}

		public virtual IEnumerable<T> EnumerateFromTo (T start, T end)
		{
			if (GreaterThan (start, end))
				throw new ArgumentException ("Cannot enumerate when end value is greater than start value.");

			return EnumerateFrom (start).TakeWhile (v => LessThanOrEqual (v, end));
		}

		public virtual IEnumerable<T> EnumerateFromThenTo (T first, T start, T end)
		{
			return new[]{first}.Concat (EnumerateFromTo (start, end));
		}
		#endregion class Enum a

		public virtual bool HasBounds {
			get {return false;}
		}

		#region class Bounded a where
		public virtual T MinValue {
			get {throw new NotSupportedException ();}
		}

		public virtual T MaxValue {
			get {throw new NotSupportedException ();}
		}
		#endregion class Bounded

		#region class (Eq a, Show a) => Num a where
		public abstract T Add (T x, T y);
		public abstract T Multiply (T x, T y);

		public abstract T Subtract (T x, T y);  // could be implemented in terms of Negate, but we need to choose one...

		public virtual T Negate (T value)
		{
			return Subtract (FromInt32 (0), value);
		}

		public virtual T Abs (T value)
		{
			if (LessThan (value, FromInt32 (0)))
				return Negate (value);
			return value;
		}

		public virtual T Sign (T value)
		{
			var zero = FromInt32 (0);
			if (Equals (zero, value))
				return zero;
			if (LessThan (value, zero))
				return FromInt32 (-1);
			return FromInt32 (1);
		}

		[CLSCompliant (false)]
		public virtual T FromIConvertible (IConvertible value)
		{
			var r = Either.TryConvert<T>((object) value);
			return r.Fold (v => v, e => {throw e;});
		}
		#endregion class Num a

		#region class (Num a, Ord a) => Real a where
		#endregion class Real a

		#region class (Real a, Enum a) => Integral a where
		public virtual T Quotient (T x, T y)
		{
			T remainder;
			return QuotientRemainder (x, y, out remainder);
		}

		public virtual T Remainder (T x, T y)
		{
			T remainder;
			QuotientRemainder (x, y, out remainder);
			return remainder;
		}

		public virtual T DivideIntegral (T x, T y)
		{
			T _;
			return DivideIntegralModulus (x, y, out _);
		}

		public virtual T Modulus (T x, T y)
		{
			T modulus;
			DivideIntegralModulus (x, y, out modulus);
			return modulus;
		}

		// returns quotient
		public abstract T QuotientRemainder (T x, T y, out T remainder);

		// returns divide
		public virtual T DivideIntegralModulus (T x, T y, out T modulus)
		{
			var quotient = QuotientRemainder (x, y, out modulus);
			if (!IsUnsigned && Equals (Sign (modulus), Negate (Sign (y)))) {
				quotient  = Predecessor (quotient);
				modulus   = Add (modulus, y);
			}
			return quotient;
		}

		[CLSCompliant (false)]
		public virtual IConvertible ToIConvertible (T value)
		{
			var v = value as IConvertible;
			if (v != null)
				return v;
			throw new NotSupportedException ();
		}
		#endregion class Integral a

		#region class Num a => Fractional a where
		public virtual T Divide (T x, T y)
		{
			return DivideIntegral (x, y);
		}

		public virtual T Reciprocal (T value)
		{
			return Divide (FromInt32 (1), value);
		}
		#endregion class Fractional a

		#region class Fractional a => Floating a where
		public virtual T Pi {
			get {return FromIConvertible (Math.PI);}
		}

		public virtual T E {
			get {return FromIConvertible (Math.E);}
		}

		public virtual T Exp (T value)
		{
			return FromIConvertible (Math.Exp (ToIConvertible (value).ToDouble (null)));
		}

		public virtual T Sqrt (T value)
		{
			return FromIConvertible (Math.Sqrt (ToIConvertible (value).ToDouble (null)));
		}

		public virtual T Log (T value)
		{
			return FromIConvertible (Math.Log (ToIConvertible (value).ToDouble (null)));
		}

		public virtual T Pow (T value, T exp)
		{
			return FromIConvertible (
					Math.Pow (
						ToIConvertible (value).ToDouble (null),
						ToIConvertible (exp).ToDouble (null)));
		}

		public virtual T Log (T value, T newBase)
		{
			return FromIConvertible (
					Math.Log (
						ToIConvertible (value).ToDouble (null),
						ToIConvertible (newBase).ToDouble (null)));
		}

		public virtual T Sin (T value)
		{
			return FromIConvertible (Math.Sin (ToIConvertible (value).ToDouble (null)));
		}

		public virtual T Tan (T value)
		{
			return FromIConvertible (Math.Tan (ToIConvertible (value).ToDouble (null)));
		}

		public virtual T Cos (T value)
		{
			return FromIConvertible (Math.Cos (ToIConvertible (value).ToDouble (null)));
		}

		public virtual T Asin (T value)
		{
			return FromIConvertible (Math.Asin (ToIConvertible (value).ToDouble (null)));
		}

		public virtual T Atan (T value)
		{
			return FromIConvertible (Math.Atan (ToIConvertible (value).ToDouble (null)));
		}

		public virtual T Acos (T value)
		{
			return FromIConvertible (Math.Acos (ToIConvertible (value).ToDouble (null)));
		}

		public virtual T Sinh (T value)
		{
			return FromIConvertible (Math.Sin (ToIConvertible (value).ToDouble (null)));
		}

		public virtual T Tanh (T value)
		{
			return FromIConvertible (Math.Tan (ToIConvertible (value).ToDouble (null)));
		}

		public virtual T Cosh (T value)
		{
			return FromIConvertible (Math.Cos (ToIConvertible (value).ToDouble (null)));
		}
		#endregion classFloating a

		#region class (Real a, Fractional a) => RealFrac a where
		public virtual int FloatRadix {
			get {
				throw new NotSupportedException ();
			}
		}

		public virtual int FloatDigits {
			get {
				throw new NotSupportedException ();
			}
		}

		public virtual Tuple<int, int> FloatRange {
			get {
				throw new NotSupportedException ();
			}
		}

		// skip: decodeFloat, encodeFloat, exponent, significand, scaleFloat

		public virtual bool IsNaN (T value)
		{
			return false;
		}

		public virtual bool IsInfinite (T value)
		{
			return false;
		}

		public virtual bool IsIEEE (T value)
		{
			return false;
		}

		public virtual T Atan2 (T y, T x)
		{
			return FromIConvertible (
					Math.Atan2 (
						ToIConvertible (y).ToDouble (null),
						ToIConvertible (x).ToDouble (null)));
		}
		#endregion class RealFrac a

		#region class (RealFrac a, Floating a) => RealFloat a where
		public virtual T Truncate (T value)
		{
			return FromIConvertible (Math.Truncate (ToIConvertible (value).ToDouble (null)));
		}

		public virtual T Round (T value)
		{
			return FromIConvertible (Math.Round (ToIConvertible (value).ToDouble (null)));
		}

		public virtual T Ceiling (T value)
		{
			return FromIConvertible (Math.Ceiling (ToIConvertible (value).ToDouble (null)));
		}

		public virtual T Floor (T value)
		{
			return FromIConvertible (Math.Floor (ToIConvertible (value).ToDouble (null)));
		}
		#endregion class RealFloat a

		#region Numeric functions
		// subtract?
		#if false
		public abstract bool Even (T value);
		public abstract bool Odd (T value);
		public abstract T LeastCommonMultiple (T a, T b);
		#endif
		// TODO: ^, ^^, fromIntegral, realToFrac
		#endregion

		public virtual T IEEERemainder (T x, T y)
		{
			throw new NotSupportedException ();
		}
	}

	internal class DecimalMath : Math<decimal> {

		public override bool    IsUnsigned                                  {get {return false;}}
		public override bool    IsTwosComplement                            {get {return false;}}
		public override bool    IsFractional                                {get {return true;}}
		public override bool    IsFloatingPoint                             {get {return false;}}
		public override bool    LessThan            (decimal x, decimal y)  {return x < y;}
		public override bool    LessThanOrEqual     (decimal x, decimal y)  {return x <= y;}
		public override bool    GreaterThan         (decimal x, decimal y)  {return x > y;}
		public override bool    GreaterThanOrEqual  (decimal x, decimal y)  {return x >= y;}
		public override decimal Max                 (decimal x, decimal y)  {return Math.Max (x, y);}
		public override decimal Min                 (decimal x, decimal y)  {return Math.Min (x, y);}
		public override decimal Successor           (decimal value)         {return checked (value+1);}
		public override decimal Predecessor         (decimal value)         {return checked (value-1);}
		public override decimal FromInt32           (int value)             {return value;}
		public override int     ToInt32             (decimal value)         {return (int) value;}
		public override bool    HasBounds                                   {get {return true;}}
		public override decimal MinValue                                    {get {return decimal.MinValue;}}
		public override decimal MaxValue                                    {get {return decimal.MaxValue;}}
		public override decimal Add                 (decimal x, decimal y)  {return checked (x + y);}
		public override decimal Multiply            (decimal x, decimal y)  {return checked (x * y);}
		public override decimal Subtract            (decimal x, decimal y)  {return checked (x - y);}
		public override decimal Negate              (decimal value)         {return checked (-value);}
		public override decimal Abs                 (decimal value)         {return Math.Abs (value);}
		public override decimal Sign                (decimal value)         {return Math.Sign (value);}
		public override decimal FromIConvertible    (IConvertible value)    {Check.Value (value); return value.ToDecimal (null);}
		public override decimal Quotient            (decimal x, decimal y)  {return (int) (x / y);}       // truncates toward 0
		public override decimal Remainder           (decimal x, decimal y)  {return x % y;}
		public override decimal DivideIntegral      (decimal x, decimal y)  {return Math.Floor (x / y);}  // truncates toward -inf
		public override decimal Modulus             (decimal x, decimal y)  {return Math.Abs (x % y);}
		public override decimal QuotientRemainder   (decimal x, decimal y, out decimal remainder) {remainder = x % y; return (int) (x / y);}
		public override decimal DivideIntegralModulus (decimal x, decimal y, out decimal modulus) {modulus = Math.Abs (x % y); return DivideIntegral (x, y);}
		public override IConvertible
		                        ToIConvertible   (decimal value)            {return value;}
		public override decimal Divide              (decimal x, decimal y)  {return x / y;}
		public override decimal Reciprocal          (decimal value)         {return 1.0m / value;}
		public override decimal Pi                                          {get {return new decimal (Math.PI);}}
		public override decimal E                                           {get {return new decimal (Math.E);}}
		public override decimal Exp (decimal value)                         {return new decimal (Math.Exp (decimal.ToDouble (value)));}
		public override decimal Sqrt (decimal value)                        {return new decimal (Math.Sqrt (decimal.ToDouble (value)));}
		public override decimal Log (decimal value)                         {return new decimal (Math.Log (decimal.ToDouble (value)));}
		public override decimal Pow (decimal value, decimal exp)            {return new decimal (Math.Pow (decimal.ToDouble (value), decimal.ToDouble (exp)));}
		public override decimal Log (decimal value, decimal newBase)        {return new decimal (Math.Log (decimal.ToDouble (value), decimal.ToDouble (newBase)));}
		public override decimal Sin (decimal value)                         {return new decimal (Math.Sin (decimal.ToDouble (value)));}
		public override decimal Tan (decimal value)                         {return new decimal (Math.Tan (decimal.ToDouble (value)));}
		public override decimal Cos (decimal value)                         {return new decimal (Math.Cos (decimal.ToDouble (value)));}
		public override decimal Asin (decimal value)                        {return new decimal (Math.Asin (decimal.ToDouble (value)));}
		public override decimal Atan (decimal value)                        {return new decimal (Math.Atan (decimal.ToDouble (value)));}
		public override decimal Acos (decimal value)                        {return new decimal (Math.Acos (decimal.ToDouble (value)));}
		public override decimal Sinh (decimal value)                        {return new decimal (Math.Sinh (decimal.ToDouble (value)));}
		public override decimal Tanh (decimal value)                        {return new decimal (Math.Tanh (decimal.ToDouble (value)));}
		public override decimal Cosh (decimal value)                        {return new decimal (Math.Cosh (decimal.ToDouble (value)));}
		public override int     FloatRadix                                  {get {return 10;}}
		public override int     FloatDigits                                 {get {return 96;}}
		public override Tuple<int, int>
		                        FloatRange                                  {get {throw new InvalidOperationException ("No idea what to do w/ decimal.");}}	// TODO
		public override bool    IsNaN               (decimal value)         {return false;}
		public override bool    IsInfinite          (decimal value)         {return false;}
		public override bool    IsIEEE              (decimal value)         {return false;}
		public override decimal Atan2               (decimal y, decimal x)  {return (decimal) Math.Atan2 (decimal.ToDouble (y), decimal.ToDouble (x));}
		public override decimal Truncate            (decimal value)         {return Math.Truncate (value);}
		public override decimal Round               (decimal value)         {return Math.Round (value);}
		public override decimal Ceiling             (decimal value)         {return Math.Ceiling (value);}
		public override decimal Floor               (decimal value)         {return Math.Floor (value);}
		public override decimal IEEERemainder       (decimal x, decimal y)  {return (decimal) Math.IEEERemainder (decimal.ToDouble (x), decimal.ToDouble (y));}
	}

	internal class DoubleMath : Math<double> {

		public override bool    IsUnsigned                                {get {return false;}}
		public override bool    IsTwosComplement                          {get {return false;}}
		public override bool    IsFractional                              {get {return true;}}
		public override bool    IsFloatingPoint                           {get {return true;}}
		public override bool    LessThan            (double x, double y)  {return x < y;}
		public override bool    LessThanOrEqual     (double x, double y)  {return x <= y;}
		public override bool    GreaterThan         (double x, double y)  {return x > y;}
		public override bool    GreaterThanOrEqual  (double x, double y)  {return x >= y;}
		public override double  Max                 (double x, double y)  {return Math.Max (x, y);}
		public override double  Min                 (double x, double y)  {return Math.Min (x, y);}
		public override double  Successor           (double value)        {return checked (value+1);}
		public override double  Predecessor         (double value)        {return checked (value-1);}
		public override double  FromInt32           (int value)           {return value;}
		public override int     ToInt32             (double value)        {return (int) value;}
		public override bool    HasBounds                                 {get {return true;}}
		public override double  MinValue                                  {get {return double.MinValue;}}
		public override double  MaxValue                                  {get {return double.MaxValue;}}
		public override double  Add                 (double x, double y)  {return checked (x + y);}
		public override double  Multiply            (double x, double y)  {return checked (x * y);}
		public override double  Subtract            (double x, double y)  {return checked (x - y);}
		public override double  Negate              (double value)        {return checked (-value);}
		public override double  Abs                 (double value)        {return Math.Abs (value);}
		public override double  Sign                (double value)        {return Math.Sign (value);}
		public override double  FromIConvertible    (IConvertible value)  {Check.Value (value); return value.ToDouble (null);}
		public override double  Quotient            (double x, double y)  {double q = (x / y); return IsNaN (q) || IsInfinite (q) ? q : (double) (int) q;}  // truncates toward 0
		public override double  Remainder           (double x, double y)  {return x % y;}
		public override double  DivideIntegral      (double x, double y)  {return Math.Floor (x / y);}  // truncates toward -inf
		public override double  Modulus             (double x, double y)  {return Math.Abs (x % y);}
		public override double  QuotientRemainder   (double x, double y, out double remainder) {remainder = Remainder (x, y); return Quotient (x, y);}
		public override double  DivideIntegralModulus (double x, double y, out double modulus) {modulus = Math.Abs (x % y); return DivideIntegral (x, y);}
		public override IConvertible
		                           ToIConvertible   (double value)        {return value;}
		public override double  Divide              (double x, double y)  {return x / y;}
		public override double  Reciprocal          (double value)        {return 1.0 / value;}
		public override double  Pi                                        {get {return Math.PI;}}
		public override double  E                                         {get {return Math.E;}}
		public override double  Exp (double value)                        {return Math.Exp (value);}
		public override double  Sqrt (double value)                       {return Math.Sqrt (value);}
		public override double  Log (double value)                        {return Math.Log (value);}
		public override double  Pow (double value, double exp)            {return Math.Pow (value, exp);}
		public override double  Log (double value, double newBase)        {return Math.Log (value, newBase);}
		public override double  Sin (double value)                        {return Math.Sin (value);}
		public override double  Tan (double value)                        {return Math.Tan (value);}
		public override double  Cos (double value)                        {return Math.Cos (value);}
		public override double  Asin (double value)                       {return Math.Asin (value);}
		public override double  Atan (double value)                       {return Math.Atan (value);}
		public override double  Acos (double value)                       {return Math.Acos (value);}
		public override double  Sinh (double value)                       {return Math.Sinh (value);}
		public override double  Tanh (double value)                       {return Math.Tanh (value);}
		public override double  Cosh (double value)                       {return Math.Cosh (value);}
		public override int     FloatRadix                                {get {return 2;}}
		public override int     FloatDigits                               {get {return 53;}}
		public override Tuple<int, int>
		                        FloatRange                                {get {return Tuple.Create (-1022, 1023);}}  // TODO: valid?
		public override bool    IsNaN               (double value)        {return double.IsNaN (value);}
		public override bool    IsInfinite          (double value)        {return double.IsInfinity (value);}
		public override bool    IsIEEE              (double value)        {return true;}
		public override double  Atan2               (double y, double x)  {return Math.Atan2 (y, x);}
		public override double  Truncate            (double value)        {return Math.Truncate (value);}
		public override double  Round               (double value)        {return Math.Round (value);}
		public override double  Ceiling             (double value)        {return Math.Ceiling (value);}
		public override double  Floor               (double value)        {return Math.Floor (value);}
		public override double  IEEERemainder       (double x, double y)  {return Math.IEEERemainder (x, y);}
	}

	internal class SingleMath : Math<float> {

		public override bool    IsUnsigned                                {get {return false;}}
		public override bool    IsTwosComplement                          {get {return false;}}
		public override bool    IsFractional                              {get {return true;}}
		public override bool    IsFloatingPoint                           {get {return true;}}
		public override bool    LessThan            (float x, float y)    {return x < y;}
		public override bool    LessThanOrEqual     (float x, float y)    {return x <= y;}
		public override bool    GreaterThan         (float x, float y)    {return x > y;}
		public override bool    GreaterThanOrEqual  (float x, float y)    {return x >= y;}
		public override float   Max                 (float x, float y)    {return Math.Max (x, y);}
		public override float   Min                 (float x, float y)    {return Math.Min (x, y);}
		public override float   Successor           (float value)         {return checked (value+1);}
		public override float   Predecessor         (float value)         {return checked (value-1);}
		public override float   FromInt32           (int value)           {return value;}
		public override int     ToInt32             (float value)         {return (int) value;}
		public override bool    HasBounds                                 {get {return true;}}
		public override float   MinValue                                  {get {return float.MinValue;}}
		public override float   MaxValue                                  {get {return float.MaxValue;}}
		public override float   Add                 (float x, float y)    {return checked (x + y);}
		public override float   Multiply            (float x, float y)    {return checked (x * y);}
		public override float   Subtract            (float x, float y)    {return checked (x - y);}
		public override float   Negate              (float value)         {return checked (-value);}
		public override float   Abs                 (float value)         {return Math.Abs (value);}
		public override float   Sign                (float value)         {return Math.Sign (value);}
		public override float   FromIConvertible    (IConvertible value)  {Check.Value (value); return value.ToSingle (null);}
		public override float   Quotient            (float x, float y)    {float q = (x / y); return IsNaN (q) || IsInfinite (q) ? q : (float) (int) q;}  // truncates toward 0
		public override float   Remainder           (float x, float y)    {return x % y;}
		public override float   DivideIntegral      (float x, float y)    {return (float) Math.Floor (x / y);}  // truncates toward -inf
		public override float   Modulus             (float x, float y)    {return Math.Abs (x % y);}
		public override float   QuotientRemainder   (float x, float y, out float remainder) {remainder = Remainder (x, y); return Quotient (x, y);}
		public override float   DivideIntegralModulus (float x, float y, out float modulus) {modulus = Math.Abs (x % y); return DivideIntegral (x, y);}
		public override IConvertible
		                           ToIConvertible   (float value)         {return value;}
		public override float   Divide              (float x, float y)    {return x / y;}
		public override float   Reciprocal          (float value)         {return 1.0f / value;}
		public override float   Pi                                        {get {return (float) Math.PI;}}
		public override float   E                                         {get {return (float) Math.E;}}
		public override float   Exp (float value)                         {return (float) Math.Exp (value);}
		public override float   Sqrt (float value)                        {return (float) Math.Sqrt (value);}
		public override float   Log (float value)                         {return (float) Math.Log (value);}
		public override float   Pow (float value, float exp)              {return (float) Math.Pow (value, exp);}
		public override float   Log (float value, float newBase)          {return (float) Math.Log (value, newBase);}
		public override float   Sin (float value)                         {return (float) Math.Sin (value);}
		public override float   Tan (float value)                         {return (float) Math.Tan (value);}
		public override float   Cos (float value)                         {return (float) Math.Cos (value);}
		public override float   Asin (float value)                        {return (float) Math.Asin (value);}
		public override float   Atan (float value)                        {return (float) Math.Atan (value);}
		public override float   Acos (float value)                        {return (float) Math.Acos (value);}
		public override float   Sinh (float value)                        {return (float) Math.Sinh (value);}
		public override float   Tanh (float value)                        {return (float) Math.Tanh (value);}
		public override float   Cosh (float value)                        {return (float) Math.Cosh (value);}
		public override int     FloatRadix                                {get {return 2;}}
		public override int     FloatDigits                               {get {return 24;}}
		public override Tuple<int, int>
		                        FloatRange                                {get {return Tuple.Create (-126, 127);}}  // TODO: valid?
		public override bool    IsNaN               (float value)         {return float.IsNaN (value);}
		public override bool    IsInfinite          (float value)         {return float.IsInfinity (value);}
		public override bool    IsIEEE              (float value)         {return true;}
		public override float   Atan2               (float y, float x)    {return (float) Math.Atan2 (y, x);}
		public override float   Truncate            (float value)         {return (float) Math.Truncate (value);}
		public override float   Round               (float value)         {return (float) Math.Round (value);}
		public override float   Ceiling             (float value)         {return (float) Math.Ceiling (value);}
		public override float   Floor               (float value)         {return (float) Math.Floor (value);}
		public override float   IEEERemainder       (float x, float y)    {return (float) Math.IEEERemainder (x, y);}
	}

	internal class ByteMath : Math<byte> {

		public override bool  IsUnsigned                            {get {return true;}}
		public override bool  IsTwosComplement                      {get {return false;}}
		public override bool  IsFractional                          {get {return false;}}
		public override bool  IsFloatingPoint                       {get {return false;}}
		public override bool  LessThan            (byte x, byte y)  {return x < y;}
		public override bool  LessThanOrEqual     (byte x, byte y)  {return x <= y;}
		public override bool  GreaterThan         (byte x, byte y)  {return x > y;}
		public override bool  GreaterThanOrEqual  (byte x, byte y)  {return x >= y;}
		public override byte  Max                 (byte x, byte y)  {return Math.Max (x, y);}
		public override byte  Min                 (byte x, byte y)  {return Math.Min (x, y);}
		public override byte  Successor           (byte value)      {return checked ((byte) (value+1));}
		public override byte  Predecessor         (byte value)      {return checked ((byte) (value-1));}
		public override byte  FromInt32           (int value)       {return checked ((byte) value);}
		public override int   ToInt32             (byte value)      {return value;}
		public override bool  HasBounds                             {get {return true;}}
		public override byte  MinValue                              {get {return byte.MinValue;}}
		public override byte  MaxValue                              {get {return byte.MaxValue;}}
		public override byte  Add                 (byte x, byte y)  {return checked ((byte) (x + y));}
		public override byte  Multiply            (byte x, byte y)  {return checked ((byte) (x * y));}
		public override byte  Subtract            (byte x, byte y)  {return checked ((byte) (x - y));}
		public override byte  Negate              (byte value)      {return checked ((byte) (-value));}
		public override byte  Abs                 (byte value)      {return (byte) Math.Abs (value);}
		public override byte  Sign                (byte value)      {return (byte) Math.Sign (value);}
		public override byte  FromIConvertible    (IConvertible value)  {Check.Value (value); return value.ToByte (null);}
		public override byte  Quotient            (byte x, byte y)  {return checked ((byte) (x / y));} // truncates toward 0
		public override byte  Remainder           (byte x, byte y)  {return checked ((byte) (x % y));}
		public override byte  DivideIntegral      (byte x, byte y)  {return (byte) (((x >= 0) ? x : checked ((byte) (x-1))) / y);} // truncates toward -inf
		public override byte  Modulus             (byte x, byte y)  {return (byte) Math.Abs (x % y);} // TODO?
		public override byte  QuotientRemainder   (byte x, byte y, out byte remainder) {remainder = checked ((byte) (x % y)); return checked ((byte) (x / y));}
		public override byte  DivideIntegralModulus (byte x, byte y, out byte modulus) {modulus = (byte) Math.Abs (x % y); return DivideIntegral (x, y);}
		public override byte  Divide              (byte x, byte y)  {return (byte) (x / y);}
		public override byte  Reciprocal          (byte value)      {return checked ((byte) (0 / value));}
	}

	internal class Int16Math : Math<short> {

		public override bool  IsUnsigned                              {get {return false;}}
		public override bool  IsTwosComplement                        {get {return true;}}
		public override bool  IsFractional                            {get {return false;}}
		public override bool  IsFloatingPoint                         {get {return false;}}
		public override bool  LessThan            (short x, short y)  {return x < y;}
		public override bool  LessThanOrEqual     (short x, short y)  {return x <= y;}
		public override bool  GreaterThan         (short x, short y)  {return x > y;}
		public override bool  GreaterThanOrEqual  (short x, short y)  {return x >= y;}
		public override short Max                 (short x, short y)  {return Math.Max (x, y);}
		public override short Min                 (short x, short y)  {return Math.Min (x, y);}
		public override short Successor           (short value)       {return checked ((short) (value+1));}
		public override short Predecessor         (short value)       {return checked ((short) (value-1));}
		public override short FromInt32           (int value)         {return checked ((short) value);}
		public override int   ToInt32             (short value)       {return checked ((int) value);}
		public override bool  HasBounds                               {get {return true;}}
		public override short MinValue                                {get {return short.MinValue;}}
		public override short MaxValue                                {get {return short.MaxValue;}}
		public override short Add                 (short x, short y)  {return checked ((short) (x + y));}
		public override short Multiply            (short x, short y)  {return checked ((short) (x * y));}
		public override short Subtract            (short x, short y)  {return checked ((short) (x - y));}
		public override short Negate              (short value)       {return checked ((short) (-value));}
		public override short Abs                 (short value)       {return checked ((short) Math.Abs (value));}
		public override short Sign                (short value)       {return (short) Math.Sign (value);}
		public override short FromIConvertible    (IConvertible value)  {Check.Value (value); return value.ToInt16 (null);}
		public override short Quotient            (short x, short y)  {return checked ((short) (x / y));} // truncates toward 0
		public override short Remainder           (short x, short y)  {return checked ((short) (x % y));}
		public override short DivideIntegral      (short x, short y)  {return (short) (((x >= 0) ? x : checked ((short) (x-1))) / y);} // truncates toward -inf
		public override short Modulus             (short x, short y)  {return (short) Math.Abs (x % y);} // TODO?
		public override short QuotientRemainder   (short x, short y, out short remainder) {remainder = checked ((short) (x % y)); return checked ((short) (x / y));}
		public override short DivideIntegralModulus (short x, short y, out short modulus) {modulus = (short) Math.Abs (x % y); return DivideIntegral (x, y);}
		public override short Divide              (short x, short y)  {return (short) (x / y);}
		public override short Reciprocal          (short value)       {return checked ((short) (0 / value));}
	}

	internal class Int32Math : Math<int> {

		public override bool  IsUnsigned                          {get {return false;}}
		public override bool  IsTwosComplement                    {get {return true;}}
		public override bool  IsFractional                        {get {return false;}}
		public override bool  IsFloatingPoint                     {get {return false;}}
		public override bool  LessThan            (int x, int y)  {return x < y;}
		public override bool  LessThanOrEqual     (int x, int y)  {return x <= y;}
		public override bool  GreaterThan         (int x, int y)  {return x > y;}
		public override bool  GreaterThanOrEqual  (int x, int y)  {return x >= y;}
		public override int   Max                 (int x, int y)  {return Math.Max (x, y);}
		public override int   Min                 (int x, int y)  {return Math.Min (x, y);}
		public override int   Successor           (int value)     {return checked (value+1);}
		public override int   Predecessor         (int value)     {return checked (value-1);}
		public override int   FromInt32           (int value)     {return value;}
		public override int   ToInt32             (int value)     {return value;}
		public override bool  HasBounds                           {get {return true;}}
		public override int   MinValue                            {get {return int.MinValue;}}
		public override int   MaxValue                            {get {return int.MaxValue;}}
		public override int   Add                 (int x, int y)  {return checked (x + y);}
		public override int   Multiply            (int x, int y)  {return checked (x * y);}
		public override int   Subtract            (int x, int y)  {return checked (x - y);}
		public override int   Negate              (int value)     {return checked (-value);}
		public override int   Abs                 (int value)     {return Math.Abs (value);}
		public override int   Sign                (int value)     {return Math.Sign (value);}
		public override int   FromIConvertible    (IConvertible value)  {Check.Value (value); return value.ToInt32 (null);}
		public override int   Quotient            (int x, int y)  {return x / y;} // truncates toward 0
		public override int   Remainder           (int x, int y)  {return x % y;}
		public override int   DivideIntegral      (int x, int y)  {return ((x >= 0) ? x : checked (x-1))/ y;} // truncates toward -inf
		public override int   Modulus             (int x, int y)  {return Math.Abs (x % y);} // TODO?
		public override int   QuotientRemainder   (int x, int y, out int remainder) {remainder = x % y; return x / y;}
		public override int   DivideIntegralModulus (int x, int y, out int modulus) {modulus = Math.Abs (x % y); return DivideIntegral (x, y);}
		public override int   Divide              (int x, int y)  {return x / y;}
		public override int   Reciprocal          (int value)     {return checked (0 / value);}
	}

	internal class Int64Math : Math<long> {

		public override bool  IsUnsigned                            {get {return false;}}
		public override bool  IsTwosComplement                      {get {return true;}}
		public override bool  IsFractional                          {get {return false;}}
		public override bool  IsFloatingPoint                       {get {return false;}}
		public override bool  LessThan            (long x, long y)  {return x < y;}
		public override bool  LessThanOrEqual     (long x, long y)  {return x <= y;}
		public override bool  GreaterThan         (long x, long y)  {return x > y;}
		public override bool  GreaterThanOrEqual  (long x, long y)  {return x >= y;}
		public override long  Max                 (long x, long y)  {return Math.Max (x, y);}
		public override long  Min                 (long x, long y)  {return Math.Min (x, y);}
		public override long  Successor           (long value)      {return checked ((long) (value+1));}
		public override long  Predecessor         (long value)      {return checked ((long) (value-1));}
		public override long  FromInt32           (int value)       {return checked ((long) value);}
		public override int   ToInt32             (long value)      {return checked ((int) value);}
		public override bool  HasBounds                             {get {return true;}}
		public override long  MinValue                              {get {return long.MinValue;}}
		public override long  MaxValue                              {get {return long.MaxValue;}}
		public override long  Add                 (long x, long y)  {return checked ((long) (x + y));}
		public override long  Multiply            (long x, long y)  {return checked ((long) (x * y));}
		public override long  Subtract            (long x, long y)  {return checked ((long) (x - y));}
		public override long  Negate              (long value)      {return checked ((long) (-value));}
		public override long  Abs                 (long value)      {return checked ((long) Math.Abs (value));}
		public override long  Sign                (long value)      {return (long) Math.Sign (value);}
		public override long  FromIConvertible    (IConvertible value)  {Check.Value (value); return value.ToInt64 (null);}
		public override long  Quotient            (long x, long y)  {return checked ((long) (x / y));} // truncates toward 0
		public override long  Remainder           (long x, long y)  {return checked ((long) (x % y));}
		public override long  DivideIntegral      (long x, long y)  {return (long) (((x >= 0) ? x : checked ((long) (x-1))) / y);} // truncates toward -inf
		public override long  Modulus             (long x, long y)  {return (long) Math.Abs (x % y);} // TODO?
		public override long  QuotientRemainder   (long x, long y, out long remainder) {remainder = checked ((long) (x % y)); return checked ((long) (x / y));}
		public override long  DivideIntegralModulus (long x, long y, out long modulus) {modulus = (long) Math.Abs (x % y); return DivideIntegral (x, y);}
		public override long  Divide              (long x, long y)  {return (long) (x / y);}
		public override long  Reciprocal          (long value)      {return checked ((long) (0 / value));}
	}

	internal class SByteMath : Math<sbyte> {

		public override bool  IsUnsigned                              {get {return false;}}
		public override bool  IsTwosComplement                        {get {return true;}}
		public override bool  IsFractional                            {get {return false;}}
		public override bool  IsFloatingPoint                         {get {return false;}}
		public override bool  LessThan            (sbyte x, sbyte y)  {return x < y;}
		public override bool  LessThanOrEqual     (sbyte x, sbyte y)  {return x <= y;}
		public override bool  GreaterThan         (sbyte x, sbyte y)  {return x > y;}
		public override bool  GreaterThanOrEqual  (sbyte x, sbyte y)  {return x >= y;}
		public override sbyte Max                 (sbyte x, sbyte y)  {return Math.Max (x, y);}
		public override sbyte Min                 (sbyte x, sbyte y)  {return Math.Min (x, y);}
		public override sbyte Successor           (sbyte value)       {return checked ((sbyte) (value+1));}
		public override sbyte Predecessor         (sbyte value)       {return checked ((sbyte) (value-1));}
		public override sbyte FromInt32           (int value)         {return checked ((sbyte) value);}
		public override int   ToInt32             (sbyte value)       {return value;}
		public override bool  HasBounds                               {get {return true;}}
		public override sbyte MinValue                                {get {return sbyte.MinValue;}}
		public override sbyte MaxValue                                {get {return sbyte.MaxValue;}}
		public override sbyte Add                 (sbyte x, sbyte y)  {return checked ((sbyte) (x + y));}
		public override sbyte Multiply            (sbyte x, sbyte y)  {return checked ((sbyte) (x * y));}
		public override sbyte Subtract            (sbyte x, sbyte y)  {return checked ((sbyte) (x - y));}
		public override sbyte Negate              (sbyte value)       {return checked ((sbyte) (-value));}
		public override sbyte Abs                 (sbyte value)       {return (sbyte) Math.Abs (value);}
		public override sbyte Sign                (sbyte value)       {return (sbyte) Math.Sign (value);}
		public override sbyte FromIConvertible    (IConvertible value)  {Check.Value (value); return value.ToSByte (null);}
		public override sbyte Quotient            (sbyte x, sbyte y)  {return checked ((sbyte) (x / y));} // truncates toward 0
		public override sbyte Remainder           (sbyte x, sbyte y)  {return checked ((sbyte) (x % y));}
		public override sbyte DivideIntegral      (sbyte x, sbyte y)  {return (sbyte) (((x >= 0) ? x : checked ((sbyte) (x-1))) / y);} // truncates toward -inf
		public override sbyte Modulus             (sbyte x, sbyte y)  {return (sbyte) Math.Abs (x % y);} // TODO?
		public override sbyte QuotientRemainder   (sbyte x, sbyte y, out sbyte remainder) {remainder = checked ((sbyte) (x % y)); return checked ((sbyte) (x / y));}
		public override sbyte DivideIntegralModulus (sbyte x, sbyte y, out sbyte modulus) {modulus = (sbyte) Math.Abs (x % y); return DivideIntegral (x, y);}
		public override sbyte Divide              (sbyte x, sbyte y)  {return (sbyte) (x / y);}
		public override sbyte Reciprocal          (sbyte value)       {return checked ((sbyte) (0 / value));}
	}

	internal class UInt16Math : Math<ushort> {

		public override bool    IsUnsigned                                {get {return true;}}
		public override bool    IsTwosComplement                          {get {return false;}}
		public override bool    IsFractional                              {get {return false;}}
		public override bool    IsFloatingPoint                           {get {return false;}}
		public override bool    LessThan            (ushort x, ushort y)  {return x < y;}
		public override bool    LessThanOrEqual     (ushort x, ushort y)  {return x <= y;}
		public override bool    GreaterThan         (ushort x, ushort y)  {return x > y;}
		public override bool    GreaterThanOrEqual  (ushort x, ushort y)  {return x >= y;}
		public override ushort  Max                 (ushort x, ushort y)  {return Math.Max (x, y);}
		public override ushort  Min                 (ushort x, ushort y)  {return Math.Min (x, y);}
		public override ushort  Successor           (ushort value)        {return checked ((ushort) (value+1));}
		public override ushort  Predecessor         (ushort value)        {return checked ((ushort) (value-1));}
		public override ushort  FromInt32           (int value)           {return checked ((ushort) value);}
		public override int     ToInt32             (ushort value)        {return checked ((int) value);}
		public override bool    HasBounds                                 {get {return true;}}
		public override ushort  MinValue                                  {get {return ushort.MinValue;}}
		public override ushort  MaxValue                                  {get {return ushort.MaxValue;}}
		public override ushort  Add                 (ushort x, ushort y)  {return checked ((ushort) (x + y));}
		public override ushort  Multiply            (ushort x, ushort y)  {return checked ((ushort) (x * y));}
		public override ushort  Subtract            (ushort x, ushort y)  {return checked ((ushort) (x - y));}
		public override ushort  Negate              (ushort value)        {return checked ((ushort) (-value));}
		public override ushort  Abs                 (ushort value)        {return checked ((ushort) Math.Abs (new decimal (value)));}
		public override ushort  Sign                (ushort value)        {return (ushort) Math.Sign (new decimal (value));}
		public override ushort  FromIConvertible    (IConvertible value)  {Check.Value (value); return value.ToUInt16 (null);}
		public override ushort  Quotient            (ushort x, ushort y)  {return checked ((ushort) (x / y));} // truncates toward 0
		public override ushort  Remainder           (ushort x, ushort y)  {return checked ((ushort) (x % y));}
		public override ushort  DivideIntegral      (ushort x, ushort y)  {return (ushort) (((x >= 0) ? x : checked ((ushort) (x-1))) / y);} // truncates toward -inf
		public override ushort  Modulus             (ushort x, ushort y)  {return (ushort) Math.Abs (new decimal (x % y));} // TODO?
		public override ushort  QuotientRemainder   (ushort x, ushort y, out ushort remainder) {remainder = checked ((ushort) (x % y)); return checked ((ushort) (x / y));}
		public override ushort  DivideIntegralModulus (ushort x, ushort y, out ushort modulus) {modulus = (ushort) Math.Abs (new decimal (x % y)); return DivideIntegral (x, y);}
		public override ushort  Divide              (ushort x, ushort y)  {return (ushort) (x / y);}
		public override ushort  Reciprocal          (ushort value)        {return checked ((ushort) (0 / value));}
	}

	internal class UInt32Math : Math<uint> {

		public override bool IsUnsigned                            {get {return true;}}
		public override bool IsTwosComplement                      {get {return false;}}
		public override bool IsFractional                          {get {return false;}}
		public override bool IsFloatingPoint                       {get {return false;}}
		public override bool LessThan            (uint x, uint y)  {return x < y;}
		public override bool LessThanOrEqual     (uint x, uint y)  {return x <= y;}
		public override bool GreaterThan         (uint x, uint y)  {return x > y;}
		public override bool GreaterThanOrEqual  (uint x, uint y)  {return x >= y;}
		public override uint Max                 (uint x, uint y)  {return Math.Max (x, y);}
		public override uint Min                 (uint x, uint y)  {return Math.Min (x, y);}
		public override uint Successor           (uint value)      {return checked ((uint) (value+1));}
		public override uint Predecessor         (uint value)      {return checked ((uint) (value-1));}
		public override uint FromInt32           (int value)       {return checked ((uint) value);}
		public override int  ToInt32             (uint value)      {return checked ((int) value);}
		public override bool HasBounds                             {get {return true;}}
		public override uint MinValue                              {get {return uint.MinValue;}}
		public override uint MaxValue                              {get {return uint.MaxValue;}}
		public override uint Add                 (uint x, uint y)  {return checked ((uint) (x + y));}
		public override uint Multiply            (uint x, uint y)  {return checked ((uint) (x * y));}
		public override uint Subtract            (uint x, uint y)  {return checked ((uint) (x - y));}
		public override uint Negate              (uint value)      {return checked ((uint) (-value));}
		public override uint Abs                 (uint value)      {return checked ((uint) Math.Abs (value));}
		public override uint Sign                (uint value)      {return (uint) Math.Sign (value);}
		public override uint FromIConvertible    (IConvertible value)  {Check.Value (value); return value.ToUInt32 (null);}
		public override uint Quotient            (uint x, uint y)  {return checked ((uint) (x / y));} // truncates toward 0
		public override uint Remainder           (uint x, uint y)  {return checked ((uint) (x % y));}
		public override uint DivideIntegral      (uint x, uint y)  {return (uint) (((x >= 0) ? x : checked ((uint) (x-1))) / y);} // truncates toward -inf
		public override uint Modulus             (uint x, uint y)  {return (uint) Math.Abs (x % y);} // TODO?
		public override uint QuotientRemainder   (uint x, uint y, out uint remainder) {remainder = checked ((uint) (x % y)); return checked ((uint) (x / y));}
		public override uint DivideIntegralModulus (uint x, uint y, out uint modulus) {modulus = (uint) Math.Abs (x % y); return DivideIntegral (x, y);}
		public override uint Divide              (uint x, uint y)  {return (uint) (x / y);}
		public override uint Reciprocal          (uint value)      {return checked ((uint) (0 / value));}
	}

	internal class UInt64Math : Math<ulong> {

		public override bool  IsUnsigned                              {get {return true;}}
		public override bool  IsTwosComplement                        {get {return false;}}
		public override bool  IsFractional                            {get {return false;}}
		public override bool  IsFloatingPoint                         {get {return false;}}
		public override bool  LessThan            (ulong x, ulong y)  {return x < y;}
		public override bool  LessThanOrEqual     (ulong x, ulong y)  {return x <= y;}
		public override bool  GreaterThan         (ulong x, ulong y)  {return x > y;}
		public override bool  GreaterThanOrEqual  (ulong x, ulong y)  {return x >= y;}
		public override ulong Max                 (ulong x, ulong y)  {return Math.Max (x, y);}
		public override ulong Min                 (ulong x, ulong y)  {return Math.Min (x, y);}
		public override ulong Successor           (ulong value)       {return checked ((ulong) (value+1));}
		public override ulong Predecessor         (ulong value)       {return checked ((ulong) (value-1));}
		public override ulong FromInt32           (int value)         {return checked ((ulong) value);}
		public override int   ToInt32             (ulong value)       {return checked ((int) value);}
		public override bool  HasBounds                               {get {return true;}}
		public override ulong MinValue                                {get {return ulong.MinValue;}}
		public override ulong MaxValue                                {get {return ulong.MaxValue;}}
		public override ulong Add                 (ulong x, ulong y)  {return checked ((ulong) (x + y));}
		public override ulong Multiply            (ulong x, ulong y)  {return checked ((ulong) (x * y));}
		public override ulong Subtract            (ulong x, ulong y)  {return checked ((ulong) (x - y));}
		public override ulong Negate              (ulong value)       {if (value == 0UL) return value; throw new OverflowException ();}
		public override ulong Abs                 (ulong value)       {return checked ((ulong) Math.Abs (new decimal (value)));}
		public override ulong Sign                (ulong value)       {return (ulong) Math.Sign (new decimal (value));}
		public override ulong FromIConvertible    (IConvertible value)  {Check.Value (value); return value.ToUInt64 (null);}
		public override ulong Quotient            (ulong x, ulong y)  {return checked ((ulong) (x / y));} // truncates toward 0
		public override ulong Remainder           (ulong x, ulong y)  {return checked ((ulong) (x % y));}
		public override ulong DivideIntegral      (ulong x, ulong y)  {return (ulong) (((x >= 0) ? x : checked ((ulong) (x-1))) / y);} // truncates toward -inf
		public override ulong Modulus             (ulong x, ulong y)  {return (ulong) Math.Abs (new decimal (x % y));} // TODO?
		public override ulong QuotientRemainder   (ulong x, ulong y, out ulong remainder) {remainder = checked ((ulong) (x % y)); return checked ((ulong) (x / y));}
		public override ulong DivideIntegralModulus (ulong x, ulong y, out ulong modulus) {modulus = (ulong) Math.Abs (new decimal (x % y)); return DivideIntegral (x, y);}
		public override ulong Divide              (ulong x, ulong y)  {return (ulong) (x / y);}
		public override ulong Reciprocal          (ulong value)       {return checked ((ulong) (0 / value));}
	}
}
