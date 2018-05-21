//
// Maybe.cs: Nullable<T> for any type.
//
// Author:
//   Jonathan Pryor  <jpryor@novell.com>
//
// Copyright (c) 2008 Novell, Inc. (http://www.novell.com)
//
// Based on ideas from: 
//  http://blogs.msdn.com/wesdyer/archive/2008/01/11/the-marvels-of-monads.aspx
//    (Turns Maybe into a struct, add some helpers, and make 
//    Maybe<T>.SelectMany actually work on current compilers.)
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
using System.ComponentModel;
using System.Linq.Expressions;

namespace Cadenza {

	public static class Maybe {

		public static Maybe<T> TryParse<T> (string value)
		{
			return TryConvert<string, T> (value);
		}

		public static Maybe<TResult> TryConvert<TSource, TResult> (TSource value)
		{
			Either<TResult, Exception> e = Either.TryConvert<TSource, TResult> (value);
			return e.Fold<Maybe<TResult>> (v => new Maybe<TResult> (v), v => Maybe<TResult>.Nothing);
		}

		public static Maybe<T> When<T> (bool condition, T value)
		{
			if (condition)
				return value.Just ();
			return Maybe<T>.Nothing;
		}

		public static Maybe<T> When<T> (bool condition, Func<T> creator)
		{
			if (creator == null)
				throw new ArgumentNullException ("creator");

			if (condition)
				return creator ().Just ();
			return Maybe<T>.Nothing;
		}
	}

	public struct Maybe<T> : IEquatable<Maybe<T>> {
		private T value;
		private bool has_value;

		public Maybe (T value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");

			this.value = value;
			has_value  = true;
		}

		public static readonly Maybe<T> Nothing = new Maybe<T> ();

		public bool HasValue {
			get {return has_value;}
		}

		public T Value {
			get {
				if (!has_value)
					throw new InvalidOperationException ("Maybe object must have a value.");
				return value;
			}
		}

		public override bool Equals (object obj)
		{
			if (obj == null)
				return HasValue == false;
			if (!(obj is Maybe<T>))
				return false;
			return Equals ((Maybe<T>) obj);
		}

		public bool Equals (Maybe<T> obj)
		{
			if (obj.HasValue != HasValue)
				return false;
			if (!HasValue)
				return true;
			return EqualityComparer<T>.Default.Equals (Value, obj.Value);
		}

		public override int GetHashCode ()
		{
			if (!HasValue)
				return 0;
			return Value.GetHashCode ();
		}

		public T GetValueOrDefault ()
		{
			return GetValueOrDefault (default (T));
		}

		public T GetValueOrDefault (T defaultValue)
		{
			if (!HasValue)
				return defaultValue;
			return Value;
		}

		public override string ToString ()
		{
			if (HasValue)
				return Value.ToString ();
			return string.Empty;
		}

		public static bool operator== (Maybe<T> a, Maybe<T> b)
		{
			return a.Equals (b);
		}

		public static bool operator!= (Maybe<T> a, Maybe<T> b)
		{
			return !a.Equals (b);
		}

		public Maybe<TResult> Select<TResult>(Func<T, TResult> selector)
		{
			Check.Selector (selector);

			if (!HasValue)
				return Maybe<TResult>.Nothing;
			return selector (Value).ToMaybe ();
		}

		public Maybe<TResult> SelectMany<TCollection, TResult>(
				Func<T, Maybe<TCollection>> selector,
				Func<T, TCollection, TResult> resultSelector)
		{
			Check.Selector (selector);
			Check.ResultSelector (resultSelector);

			if (!HasValue)
				return Maybe<TResult>.Nothing;
			Maybe<TCollection> n = selector (Value);
			if (!n.HasValue)
				return Maybe<TResult>.Nothing;
			return resultSelector(Value, n.Value).ToMaybe ();
		}
	}
}

