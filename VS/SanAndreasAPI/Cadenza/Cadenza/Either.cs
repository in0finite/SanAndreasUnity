//
// Either.cs: Either utility methods.
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

using Cadenza.Reflection;

namespace Cadenza {

	public static class Either {

		public static Either<T, Exception> TryParse<T> (string value)
		{
			return TryConvert<string, T>(value, typeof (string), typeof (T));
		}

		public static Either<TResult, Exception> TryConvert<TSource, TResult> (TSource value)
		{
			return TryConvert<TSource, TResult>(value, typeof (TSource), typeof (TResult));
		}

		static Either<TResult, Exception> TryConvert<TSource, TResult>(TSource value, Type sourceType, Type resultType)
		{
			try {
				TypeConverter c = GetConverter (resultType);
				if (c != null && c.CanConvertFrom (sourceType))
					return Either<TResult, Exception>.A ((TResult) c.ConvertFrom (value));

				c = GetConverter (sourceType);
				if (c != null && c.CanConvertTo (resultType))
					return Either<TResult, Exception>.A ((TResult) c.ConvertTo (value, resultType));

				// Convert.ChangeType uses IConvertible for type conversions;
				// throws InvalidCastException if type could not be converted.

				// Convert.ChangeType() doesn't handle nullable types; remove nullable if appropriate.
				if (resultType.IsNullable ())
					resultType = Nullable.GetUnderlyingType (resultType);
				return Either<TResult, Exception>.A ((TResult) Convert.ChangeType (value, resultType));
			}
			catch (Exception e) {
				return Either<TResult, Exception>.B (WrapException (e, sourceType, resultType));
			}
		}

		// In many circumstances, it's easier for invoking code to deal with the
		// original exception, not the wrapped exception.  
		//
		// See e.g. TargetInvocationException, or what prompted the change,
		// MathContract<T> + ExpressionMath<T> + double.NaN (where NaN->int
		// results in OverflowException, which is wrapped, which complicates the
		// tests...).
		//
		// If we can't find a (string, Exception) constructor on the original
		// exception type, we use NotSupportedException, but this should rarely
		// happen.
		static Exception WrapException (Exception e, Type sourceType, Type resultType)
		{
			string message = string.Format ("Conversion from {0} to {1} is not supported.",
					sourceType.FullName, resultType.FullName);
			Type t = e.GetType ();
			var  c = t.GetConstructor (new Type[]{typeof (string), typeof (Exception)});
			if (c == null)
				return new NotSupportedException (message, e);
			return (Exception) c.Invoke (new object[]{message, e});
		}

		static TypeConverter GetConverter (Type type)
		{
#if !SILVERLIGHT
			return TypeDescriptor.GetConverter (type);
#else
			if (type.IsNullable ())
				type = Nullable.GetUnderlyingType (type);
			var tca = type.GetCustomAttribute<TypeConverterAttribute> ();
			if (tca == null)
				return null;
			if (string.IsNullOrEmpty (tca.ConverterTypeName))
				return null;
			var converterType = Type.GetType (tca.ConverterTypeName, false);
			if (converterType == null)
				return null;
			var ctor = converterType.GetConstructor (new Type[]{typeof (Type)});
			if (ctor != null)
				return (TypeConverter) ctor.Invoke (new object[]{type});
			return (TypeConverter) Activator.CreateInstance (converterType);
#endif
		}

		public static Either<TResult, Exception> TryConvert<TResult> (object value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			return TryConvert<object, TResult>(value, value.GetType (), typeof (TResult));
		}
	}
}

