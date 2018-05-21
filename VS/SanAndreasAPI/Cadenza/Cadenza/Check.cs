//
// Check.cs
//
// Author:
//   Jb Evain (jbevain@novell.com)
//
// Copyright (c) 2007 Novell, Inc. (http://www.novell.com)
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

namespace Cadenza {

	static class Check {

		public static void Accumulate (object accumulate)
		{
			if (accumulate == null)
				throw new ArgumentNullException ("accumulate");
		}

		public static void Categories (object categories)
		{
			if (categories == null)
				throw new ArgumentNullException ("categories");
		}

		public static void ChildrenSelector (object childrenSelector)
		{
			if (childrenSelector == null)
				throw new ArgumentNullException ("childrenSelector");
		}

		public static void Comparer (object comparer)
		{
			if (comparer == null)
				throw new ArgumentNullException ("comparer");
		}

		public static void Composer (object composer)
		{
			if (composer == null)
				throw new ArgumentNullException ("composer");
		}

		public static void Creator (object comparer)
		{
			if (comparer == null)
				throw new ArgumentNullException ("creator");
		}

		public static void Destination (object destination)
		{
			if (destination == null)
				throw new ArgumentNullException ("destination");
		}

		public static void List (object list)
		{
			if (list == null)
				throw new ArgumentNullException ("list");
		}

		public static void Random (object random)
		{
			if (random == null)
				throw new ArgumentNullException ("random");
		}

		public static void ResultSelector (object resultSelector)
		{
			if (resultSelector == null)
				throw new ArgumentNullException ("resultSelector");
		}

		public static void Selector (object selector)
		{
			if (selector == null)
				throw new ArgumentNullException ("selector");
		}

		public static void Self (object self)
		{
			if (self == null)
				throw new ArgumentNullException ("self");
		}

		public static void Func (object func)
		{
			if (func == null)
				throw new ArgumentNullException ("func");
		}

		public static void SelfAndFunc (object self, object func)
		{
			Self (self);
			Func (func);
		}

		public static void Predicate (object predicate)
		{
			if (predicate == null)
				throw new ArgumentNullException ("predicate");
		}

		public static void Value (object value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
		}

		public static void Values (object values)
		{
			if (values == null)
				throw new ArgumentNullException ("values");
		}

		public static void ValueSelector (object valueSelector)
		{
			if (valueSelector == null)
				throw new ArgumentNullException ("valueSelector");
		}
	}
}
