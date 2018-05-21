//
//  DelegatedEqualityComparer.cs
//
// Author:
//   Eric Maupin  <me@ermau.com>
//
// Copyright (c) 2011 Eric Maupin (http://www.ermau.com)
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

namespace Cadenza.Collections
{
	/// <summary>
	/// A <see cref="IEqualityComparer{T}"/> that uses a selector on the <typeparamref name="TSource"/> with <see cref="EqualityComparer{T}.Default"/>.
	/// </summary>
	/// <typeparam name="TSource">The source type for comparison</typeparam>
	/// <typeparam name="T">The actual type to compare</typeparam>
	/// <example>
	/// <code>
	/// var comparer = new DelegatedEqualityComparer<string, int> (s => s.Length);
	/// comparer.Equals ("foo", "bar"); // returns true
	/// </code>
	/// </example>
	public class DelegatedEqualityComparer<TSource, T>
		: IEqualityComparer<TSource>
	{
		/// <summary>
		/// Creates a new instance of <see cref="DelegatedEqualityComparer{TSource,T}"/>.
		/// </summary>
		/// <param name="selector">The selector to execute on <typeparamref name="TSource"/> to obtain <typeparamref name="T"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="selector"/> is <c>null</c>.</exception>
		public DelegatedEqualityComparer (Func<TSource, T> selector)
		{
			if (selector == null)
				throw new ArgumentNullException ("selector");

			this.selector = selector;
		}

		public bool Equals (TSource x, TSource y)
		{
			return EqualityComparer<T>.Default.Equals (this.selector (x), this.selector (y));
		}

		public int GetHashCode (TSource obj)
		{
			return EqualityComparer<T>.Default.GetHashCode (this.selector (obj));
		}

		private readonly Func<TSource, T> selector;
	}

	/// <summary>
	/// An <see cref="IEqualityComparer{T}"/> that is implemented by constructor provided delegates.
	/// </summary>
	/// <typeparam name="T">The type to compare</typeparam>
	public class DelegatedEqualityComparer<T>
		: IEqualityComparer<T>
	{
		/// <summary>
		/// Creates a new instance of <see cref="DelegatedEqualityComparer{TSource,T}"/>
		/// </summary>
		/// <param name="equalsGetter">The <see cref="IEqualityComparer{T}.Equals(T,T)"/> implementation. Reference comparison and null checking are provided for you.</param>
		/// <param name="hashCodeGetter">The <see cref="IEqualityComparer{T}.GetHashCode(T)"/> implementation.</param>
		public DelegatedEqualityComparer (Func<T, T, bool> equalsGetter, Func<T, int> hashCodeGetter)
		{
			if (equalsGetter == null)
				throw new ArgumentNullException ("equalsGetter");
			if (hashCodeGetter == null)
				throw new ArgumentNullException ("hashCodeGetter");

			this.getEquals = equalsGetter;
			this.getHashCode = hashCodeGetter;
		}

		public bool Equals (T x, T y)
		{
			if (ReferenceEquals (x, y))
				return true;
			if (ReferenceEquals (x, null) || ReferenceEquals (y, null))
				return false;

			return this.getEquals (x, y);
		}

		public int GetHashCode (T obj)
		{
			return this.getHashCode (obj);
		}

		private readonly Func<T, T, bool> getEquals;
		private readonly Func<T, int> getHashCode;
	}
}