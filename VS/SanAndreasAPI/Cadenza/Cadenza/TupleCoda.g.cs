// 
// TuplesCoda.cs: Tuple extension methods.
// 
// GENERATED CODE: DO NOT EDIT.
// 
// To regenerate this code, execute: Tuples.exe -n 4 -o Cadenza/Tuple.g.cs
// 
// Copyright (c) 2011 Novell, Inc. (http://www.novell.com)
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
namespace Cadenza
{
    using System;
    
    
    /// <summary>
    ///   Extension methods on <c>Tuple</c> types.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public partial class TupleCoda
    {
        
        /// <typeparam name="T">
        ///   The first value type.
        /// </typeparam>
        /// <typeparam name="TResult">
        ///   The return value type.
        /// </typeparam>
        /// <param name="self">
        ///   A <see cref="T:Cadenza.Tuple{T}" /> to aggregate the values of.
        /// </param>
        /// <param name="func">
        ///   A <see cref="T:System.Func{T,TResult}" /> which will be invoked, providing the values
        ///   <see cref="P:Cadenza.Tuple`1.Item1"/>
        ///   to <paramref name="func"/> and 
        ///   returning the value returned by <paramref name="func"/>.
        /// </param>
        /// <summary>
        ///   Converts the <see cref="T:Cadenza.Tuple{T}" /> into a <typeparamref name="TResult"/>.
        /// </summary>
        /// <returns>
        ///   The <typeparamref name="TResult"/> returned by <paramref name="func"/>.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///    <block subset="none" type="behaviors">
        ///     Passes the values <see cref="P:Cadenza.Tuple`1.Item1"/> to 
        ///     <paramref name="func"/>, returning the value produced by 
        ///     <paramref name="func"/>.
        ///    </block>
        ///   </para>
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <para>
        ///     <paramref name="self" /> is <see langword="null" />.
        ///   </para>
        ///   <para>-or-</para>
        ///   <para>
        ///     <paramref name="func" /> is <see langword="null" />.
        ///   </para>
        /// </exception>
        public static TResult Aggregate<T, TResult>(this Tuple<T> self, System.Func<T, TResult> func)
        
        
        {
            Check.Self(self);
            Check.Func(func);
            return func(self.Item1);
        }
        
        /// <typeparam name="T">
        ///   The first value type.
        /// </typeparam>
        /// <typeparam name="TResult">
        ///   The return value type.
        /// </typeparam>
        /// <param name="self">
        ///   A <see cref="T:Cadenza.Tuple{T}" /> to match against.
        /// </param>
        /// <param name="matchers">
        ///   A <see cref="T:System.Func{T,Cadenza.Maybe{TResult}}" />
        ///   array containing the conversion routines to use to convert 
        ///   the current <see cref="T:Cadenza.Tuple{T}" /> instance into a 
        ///   <typeparamref name="TResult" /> value.
        /// </param>
        /// <summary>
        ///   Converts the current <see cref="T:Cadenza.Tuple{T}" /> instance into a <typeparamref name="TResult"/>.
        /// </summary>
        /// <returns>
        ///   The <typeparamref name="TResult"/> returned by one of the <paramref name="matchers"/>.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///    <block subset="none" type="behaviors">
        ///     <para>
        ///      The current <see cref="T:Cadenza.Tuple{T}" /> instance is converted into a 
        ///      <typeparamref name="TResult" /> instance by trying each
        ///      <see cref="T:System.Func{T,Cadenza.Maybe{TResult}}" />
        ///      within <paramref name="matchers" />.
        ///     </para>
        ///     <para>
        ///      This method returns 
        ///      <see cref="P:Cadenza.Maybe{TResult}.Value" /> 
        ///      for the first delegate to return a
        ///      <see cref="T:Cadenza.Maybe{TResult}" /> instance
        ///      where <see cref="P:Cadenza.Maybe{TResult}.HasValue" />
        ///      is <see langword="true" />.
        ///     </para>
        ///     <para>
        ///      If no <see cref="T:System.Func{T,Cadenza.Maybe{TResult}}" />
        ///      returns a 
        ///      <see cref="T:Cadenza.Maybe{TResult}" /> instance
        ///      where <see cref="P:Cadenza.Maybe{TResult}.HasValue" />
        ///      is <see langword="true" />, then an
        ///      <see cref="T:System.InvalidOperationException" /> is thrown.
        ///     </para>
        ///    </block>
        ///    <code lang="C#">
        ///   var    a = Tuple.Create (1, 2);
        ///   string b = a.Match (
        ///       (t, v) =&gt; Match.When ( t + v == 3, "foo!"),
        ///       (t, v) =&gt; "*default*".Just ());
        ///   Console.WriteLine (b);  // prints "foo!"</code>
        ///   </para>
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <para>
        ///     <paramref name="self" /> is <see langword="null" />.
        ///   </para>
        ///   <para>-or-</para>
        ///   <para>
        ///     <paramref name="matchers" /> is <see langword="null" />.
        ///   </para>
        /// </exception>
        /// <exception cref="T:System.InvalidOperationException">
        ///   None of the 
        ///   <see cref="T:System.Func{TSource,Cadenza.Maybe{TResult}}" />
        ///   delegates within <paramref name="matchers" /> returned a 
        ///   <see cref="T:Cadenza.Maybe{TResult}" /> instance where
        ///   <see cref="P:Cadenza.Maybe{TResult}.HasValue" /> was
        ///   <see langword="true" />.
        /// </exception>
        public static TResult Match<T, TResult>(this Tuple<T> self, params System.Func<T, Cadenza.Maybe<TResult>>[] matchers)
        
        
        {
            Check.Self(self);
            if ((matchers == null))
            {
                throw new System.ArgumentNullException("matchers");
            }
            foreach (var m in matchers) {
              var r = m (self.Item1);
              if (r.HasValue)
                return r.Value;
            }
            throw new System.InvalidOperationException("no match");
        }
        
        /// <typeparam name="T">
        ///   The first value type.
        /// </typeparam>
        /// <param name="self">
        ///   A <see cref="T:Cadenza.Tuple{T}" /> to convert into an <see cref="T:System.Collections.Generic.IEnumerable{System.Object}"/>.
        /// </param>
        /// <summary>
        ///   Converts the <see cref="T:Cadenza.Tuple{T}" /> into a <see cref="T:System.Collections.Generic.IEnumerable{System.Object}"/>.
        /// </summary>
        /// <returns>
        ///   A <see cref="T:System.Collections.Generic.IEnumerable{System.Object}"/>.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///    <block subset="none" type="behaviors">
        ///     Passes the values <see cref="P:Cadenza.Tuple`1.Item1"/> to 
        ///     <paramref name="func"/>, returning the value produced by 
        ///     <paramref name="func"/>.
        ///    </block>
        ///   </para>
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Collections.Generic.IEnumerable<object> ToEnumerable<T>(this Tuple<T> self)
        
        {
            Check.Self(self);
            return TupleCoda.CreateToEnumerableIterator(self);
        }
        
        private static System.Collections.Generic.IEnumerable<object> CreateToEnumerableIterator<T>(Tuple<T> self)
        
        {
            yield return self.Item1;
        }
        
        /// <typeparam name="T1">
        ///   The first value type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   The second value type.
        /// </typeparam>
        /// <typeparam name="TResult">
        ///   The return value type.
        /// </typeparam>
        /// <param name="self">
        ///   A <see cref="T:Cadenza.Tuple{T1,T2}" /> to aggregate the values of.
        /// </param>
        /// <param name="func">
        ///   A <see cref="T:System.Func{T1,T2,TResult}" /> which will be invoked, providing the values
        ///   <see cref="P:Cadenza.Tuple`2.Item1"/>, <see cref="P:Cadenza.Tuple`2.Item2"/>
        ///   to <paramref name="func"/> and 
        ///   returning the value returned by <paramref name="func"/>.
        /// </param>
        /// <summary>
        ///   Converts the <see cref="T:Cadenza.Tuple{T1,T2}" /> into a <typeparamref name="TResult"/>.
        /// </summary>
        /// <returns>
        ///   The <typeparamref name="TResult"/> returned by <paramref name="func"/>.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///    <block subset="none" type="behaviors">
        ///     Passes the values <see cref="P:Cadenza.Tuple`2.Item1"/>, <see cref="P:Cadenza.Tuple`2.Item2"/> to 
        ///     <paramref name="func"/>, returning the value produced by 
        ///     <paramref name="func"/>.
        ///    </block>
        ///   </para>
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <para>
        ///     <paramref name="self" /> is <see langword="null" />.
        ///   </para>
        ///   <para>-or-</para>
        ///   <para>
        ///     <paramref name="func" /> is <see langword="null" />.
        ///   </para>
        /// </exception>
        public static TResult Aggregate<T1, T2, TResult>(this Tuple<T1, T2> self, System.Func<T1, T2, TResult> func)
        
        
        
        {
            Check.Self(self);
            Check.Func(func);
            return func(self.Item1, self.Item2);
        }
        
        /// <typeparam name="T1">
        ///   The first value type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   The second value type.
        /// </typeparam>
        /// <typeparam name="TResult">
        ///   The return value type.
        /// </typeparam>
        /// <param name="self">
        ///   A <see cref="T:Cadenza.Tuple{T1,T2}" /> to match against.
        /// </param>
        /// <param name="matchers">
        ///   A <see cref="T:System.Func{T1,T2,Cadenza.Maybe{TResult}}" />
        ///   array containing the conversion routines to use to convert 
        ///   the current <see cref="T:Cadenza.Tuple{T1,T2}" /> instance into a 
        ///   <typeparamref name="TResult" /> value.
        /// </param>
        /// <summary>
        ///   Converts the current <see cref="T:Cadenza.Tuple{T1,T2}" /> instance into a <typeparamref name="TResult"/>.
        /// </summary>
        /// <returns>
        ///   The <typeparamref name="TResult"/> returned by one of the <paramref name="matchers"/>.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///    <block subset="none" type="behaviors">
        ///     <para>
        ///      The current <see cref="T:Cadenza.Tuple{T1,T2}" /> instance is converted into a 
        ///      <typeparamref name="TResult" /> instance by trying each
        ///      <see cref="T:System.Func{T1,T2,Cadenza.Maybe{TResult}}" />
        ///      within <paramref name="matchers" />.
        ///     </para>
        ///     <para>
        ///      This method returns 
        ///      <see cref="P:Cadenza.Maybe{TResult}.Value" /> 
        ///      for the first delegate to return a
        ///      <see cref="T:Cadenza.Maybe{TResult}" /> instance
        ///      where <see cref="P:Cadenza.Maybe{TResult}.HasValue" />
        ///      is <see langword="true" />.
        ///     </para>
        ///     <para>
        ///      If no <see cref="T:System.Func{T1,T2,Cadenza.Maybe{TResult}}" />
        ///      returns a 
        ///      <see cref="T:Cadenza.Maybe{TResult}" /> instance
        ///      where <see cref="P:Cadenza.Maybe{TResult}.HasValue" />
        ///      is <see langword="true" />, then an
        ///      <see cref="T:System.InvalidOperationException" /> is thrown.
        ///     </para>
        ///    </block>
        ///    <code lang="C#">
        ///   var    a = Tuple.Create (1, 2);
        ///   string b = a.Match (
        ///       (t, v) =&gt; Match.When ( t + v == 3, "foo!"),
        ///       (t, v) =&gt; "*default*".Just ());
        ///   Console.WriteLine (b);  // prints "foo!"</code>
        ///   </para>
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <para>
        ///     <paramref name="self" /> is <see langword="null" />.
        ///   </para>
        ///   <para>-or-</para>
        ///   <para>
        ///     <paramref name="matchers" /> is <see langword="null" />.
        ///   </para>
        /// </exception>
        /// <exception cref="T:System.InvalidOperationException">
        ///   None of the 
        ///   <see cref="T:System.Func{TSource,Cadenza.Maybe{TResult}}" />
        ///   delegates within <paramref name="matchers" /> returned a 
        ///   <see cref="T:Cadenza.Maybe{TResult}" /> instance where
        ///   <see cref="P:Cadenza.Maybe{TResult}.HasValue" /> was
        ///   <see langword="true" />.
        /// </exception>
        public static TResult Match<T1, T2, TResult>(this Tuple<T1, T2> self, params System.Func<T1, T2, Cadenza.Maybe<TResult>>[] matchers)
        
        
        
        {
            Check.Self(self);
            if ((matchers == null))
            {
                throw new System.ArgumentNullException("matchers");
            }
            foreach (var m in matchers) {
              var r = m (self.Item1, self.Item2);
              if (r.HasValue)
                return r.Value;
            }
            throw new System.InvalidOperationException("no match");
        }
        
        /// <typeparam name="T1">
        ///   The first value type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   The second value type.
        /// </typeparam>
        /// <param name="self">
        ///   A <see cref="T:Cadenza.Tuple{T1,T2}" /> to convert into an <see cref="T:System.Collections.Generic.IEnumerable{System.Object}"/>.
        /// </param>
        /// <summary>
        ///   Converts the <see cref="T:Cadenza.Tuple{T1,T2}" /> into a <see cref="T:System.Collections.Generic.IEnumerable{System.Object}"/>.
        /// </summary>
        /// <returns>
        ///   A <see cref="T:System.Collections.Generic.IEnumerable{System.Object}"/>.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///    <block subset="none" type="behaviors">
        ///     Passes the values <see cref="P:Cadenza.Tuple`2.Item1"/>, <see cref="P:Cadenza.Tuple`2.Item2"/> to 
        ///     <paramref name="func"/>, returning the value produced by 
        ///     <paramref name="func"/>.
        ///    </block>
        ///   </para>
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Collections.Generic.IEnumerable<object> ToEnumerable<T1, T2>(this Tuple<T1, T2> self)
        
        
        {
            Check.Self(self);
            return TupleCoda.CreateToEnumerableIterator(self);
        }
        
        private static System.Collections.Generic.IEnumerable<object> CreateToEnumerableIterator<T1, T2>(Tuple<T1, T2> self)
        
        
        {
            yield return self.Item1;
            yield return self.Item2;
        }
        
        /// <typeparam name="T1">
        ///   The first value type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   The second value type.
        /// </typeparam>
        /// <typeparam name="T3">
        ///   The third value type.
        /// </typeparam>
        /// <typeparam name="TResult">
        ///   The return value type.
        /// </typeparam>
        /// <param name="self">
        ///   A <see cref="T:Cadenza.Tuple{T1,T2,T3}" /> to aggregate the values of.
        /// </param>
        /// <param name="func">
        ///   A <see cref="T:System.Func{T1,T2,T3,TResult}" /> which will be invoked, providing the values
        ///   <see cref="P:Cadenza.Tuple`3.Item1"/>, <see cref="P:Cadenza.Tuple`3.Item2"/>, <see cref="P:Cadenza.Tuple`3.Item3"/>
        ///   to <paramref name="func"/> and 
        ///   returning the value returned by <paramref name="func"/>.
        /// </param>
        /// <summary>
        ///   Converts the <see cref="T:Cadenza.Tuple{T1,T2,T3}" /> into a <typeparamref name="TResult"/>.
        /// </summary>
        /// <returns>
        ///   The <typeparamref name="TResult"/> returned by <paramref name="func"/>.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///    <block subset="none" type="behaviors">
        ///     Passes the values <see cref="P:Cadenza.Tuple`3.Item1"/>, <see cref="P:Cadenza.Tuple`3.Item2"/>, <see cref="P:Cadenza.Tuple`3.Item3"/> to 
        ///     <paramref name="func"/>, returning the value produced by 
        ///     <paramref name="func"/>.
        ///    </block>
        ///   </para>
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <para>
        ///     <paramref name="self" /> is <see langword="null" />.
        ///   </para>
        ///   <para>-or-</para>
        ///   <para>
        ///     <paramref name="func" /> is <see langword="null" />.
        ///   </para>
        /// </exception>
        public static TResult Aggregate<T1, T2, T3, TResult>(this Tuple<T1, T2, T3> self, System.Func<T1, T2, T3, TResult> func)
        
        
        
        
        {
            Check.Self(self);
            Check.Func(func);
            return func(self.Item1, self.Item2, self.Item3);
        }
        
        /// <typeparam name="T1">
        ///   The first value type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   The second value type.
        /// </typeparam>
        /// <typeparam name="T3">
        ///   The third value type.
        /// </typeparam>
        /// <typeparam name="TResult">
        ///   The return value type.
        /// </typeparam>
        /// <param name="self">
        ///   A <see cref="T:Cadenza.Tuple{T1,T2,T3}" /> to match against.
        /// </param>
        /// <param name="matchers">
        ///   A <see cref="T:System.Func{T1,T2,T3,Cadenza.Maybe{TResult}}" />
        ///   array containing the conversion routines to use to convert 
        ///   the current <see cref="T:Cadenza.Tuple{T1,T2,T3}" /> instance into a 
        ///   <typeparamref name="TResult" /> value.
        /// </param>
        /// <summary>
        ///   Converts the current <see cref="T:Cadenza.Tuple{T1,T2,T3}" /> instance into a <typeparamref name="TResult"/>.
        /// </summary>
        /// <returns>
        ///   The <typeparamref name="TResult"/> returned by one of the <paramref name="matchers"/>.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///    <block subset="none" type="behaviors">
        ///     <para>
        ///      The current <see cref="T:Cadenza.Tuple{T1,T2,T3}" /> instance is converted into a 
        ///      <typeparamref name="TResult" /> instance by trying each
        ///      <see cref="T:System.Func{T1,T2,T3,Cadenza.Maybe{TResult}}" />
        ///      within <paramref name="matchers" />.
        ///     </para>
        ///     <para>
        ///      This method returns 
        ///      <see cref="P:Cadenza.Maybe{TResult}.Value" /> 
        ///      for the first delegate to return a
        ///      <see cref="T:Cadenza.Maybe{TResult}" /> instance
        ///      where <see cref="P:Cadenza.Maybe{TResult}.HasValue" />
        ///      is <see langword="true" />.
        ///     </para>
        ///     <para>
        ///      If no <see cref="T:System.Func{T1,T2,T3,Cadenza.Maybe{TResult}}" />
        ///      returns a 
        ///      <see cref="T:Cadenza.Maybe{TResult}" /> instance
        ///      where <see cref="P:Cadenza.Maybe{TResult}.HasValue" />
        ///      is <see langword="true" />, then an
        ///      <see cref="T:System.InvalidOperationException" /> is thrown.
        ///     </para>
        ///    </block>
        ///    <code lang="C#">
        ///   var    a = Tuple.Create (1, 2);
        ///   string b = a.Match (
        ///       (t, v) =&gt; Match.When ( t + v == 3, "foo!"),
        ///       (t, v) =&gt; "*default*".Just ());
        ///   Console.WriteLine (b);  // prints "foo!"</code>
        ///   </para>
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <para>
        ///     <paramref name="self" /> is <see langword="null" />.
        ///   </para>
        ///   <para>-or-</para>
        ///   <para>
        ///     <paramref name="matchers" /> is <see langword="null" />.
        ///   </para>
        /// </exception>
        /// <exception cref="T:System.InvalidOperationException">
        ///   None of the 
        ///   <see cref="T:System.Func{TSource,Cadenza.Maybe{TResult}}" />
        ///   delegates within <paramref name="matchers" /> returned a 
        ///   <see cref="T:Cadenza.Maybe{TResult}" /> instance where
        ///   <see cref="P:Cadenza.Maybe{TResult}.HasValue" /> was
        ///   <see langword="true" />.
        /// </exception>
        public static TResult Match<T1, T2, T3, TResult>(this Tuple<T1, T2, T3> self, params System.Func<T1, T2, T3, Cadenza.Maybe<TResult>>[] matchers)
        
        
        
        
        {
            Check.Self(self);
            if ((matchers == null))
            {
                throw new System.ArgumentNullException("matchers");
            }
            foreach (var m in matchers) {
              var r = m (self.Item1, self.Item2, self.Item3);
              if (r.HasValue)
                return r.Value;
            }
            throw new System.InvalidOperationException("no match");
        }
        
        /// <typeparam name="T1">
        ///   The first value type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   The second value type.
        /// </typeparam>
        /// <typeparam name="T3">
        ///   The third value type.
        /// </typeparam>
        /// <param name="self">
        ///   A <see cref="T:Cadenza.Tuple{T1,T2,T3}" /> to convert into an <see cref="T:System.Collections.Generic.IEnumerable{System.Object}"/>.
        /// </param>
        /// <summary>
        ///   Converts the <see cref="T:Cadenza.Tuple{T1,T2,T3}" /> into a <see cref="T:System.Collections.Generic.IEnumerable{System.Object}"/>.
        /// </summary>
        /// <returns>
        ///   A <see cref="T:System.Collections.Generic.IEnumerable{System.Object}"/>.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///    <block subset="none" type="behaviors">
        ///     Passes the values <see cref="P:Cadenza.Tuple`3.Item1"/>, <see cref="P:Cadenza.Tuple`3.Item2"/>, <see cref="P:Cadenza.Tuple`3.Item3"/> to 
        ///     <paramref name="func"/>, returning the value produced by 
        ///     <paramref name="func"/>.
        ///    </block>
        ///   </para>
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Collections.Generic.IEnumerable<object> ToEnumerable<T1, T2, T3>(this Tuple<T1, T2, T3> self)
        
        
        
        {
            Check.Self(self);
            return TupleCoda.CreateToEnumerableIterator(self);
        }
        
        private static System.Collections.Generic.IEnumerable<object> CreateToEnumerableIterator<T1, T2, T3>(Tuple<T1, T2, T3> self)
        
        
        
        {
            yield return self.Item1;
            yield return self.Item2;
            yield return self.Item3;
        }
        
        /// <typeparam name="T1">
        ///   The first value type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   The second value type.
        /// </typeparam>
        /// <typeparam name="T3">
        ///   The third value type.
        /// </typeparam>
        /// <typeparam name="T4">
        ///   The fourth value type.
        /// </typeparam>
        /// <typeparam name="TResult">
        ///   The return value type.
        /// </typeparam>
        /// <param name="self">
        ///   A <see cref="T:Cadenza.Tuple{T1,T2,T3,T4}" /> to aggregate the values of.
        /// </param>
        /// <param name="func">
        ///   A <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> which will be invoked, providing the values
        ///   <see cref="P:Cadenza.Tuple`4.Item1"/>, <see cref="P:Cadenza.Tuple`4.Item2"/>, <see cref="P:Cadenza.Tuple`4.Item3"/>, <see cref="P:Cadenza.Tuple`4.Item4"/>
        ///   to <paramref name="func"/> and 
        ///   returning the value returned by <paramref name="func"/>.
        /// </param>
        /// <summary>
        ///   Converts the <see cref="T:Cadenza.Tuple{T1,T2,T3,T4}" /> into a <typeparamref name="TResult"/>.
        /// </summary>
        /// <returns>
        ///   The <typeparamref name="TResult"/> returned by <paramref name="func"/>.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///    <block subset="none" type="behaviors">
        ///     Passes the values <see cref="P:Cadenza.Tuple`4.Item1"/>, <see cref="P:Cadenza.Tuple`4.Item2"/>, <see cref="P:Cadenza.Tuple`4.Item3"/>, <see cref="P:Cadenza.Tuple`4.Item4"/> to 
        ///     <paramref name="func"/>, returning the value produced by 
        ///     <paramref name="func"/>.
        ///    </block>
        ///   </para>
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <para>
        ///     <paramref name="self" /> is <see langword="null" />.
        ///   </para>
        ///   <para>-or-</para>
        ///   <para>
        ///     <paramref name="func" /> is <see langword="null" />.
        ///   </para>
        /// </exception>
        public static TResult Aggregate<T1, T2, T3, T4, TResult>(this Tuple<T1, T2, T3, T4> self, System.Func<T1, T2, T3, T4, TResult> func)
        
        
        
        
        
        {
            Check.Self(self);
            Check.Func(func);
            return func(self.Item1, self.Item2, self.Item3, self.Item4);
        }
        
        /// <typeparam name="T1">
        ///   The first value type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   The second value type.
        /// </typeparam>
        /// <typeparam name="T3">
        ///   The third value type.
        /// </typeparam>
        /// <typeparam name="T4">
        ///   The fourth value type.
        /// </typeparam>
        /// <typeparam name="TResult">
        ///   The return value type.
        /// </typeparam>
        /// <param name="self">
        ///   A <see cref="T:Cadenza.Tuple{T1,T2,T3,T4}" /> to match against.
        /// </param>
        /// <param name="matchers">
        ///   A <see cref="T:System.Func{T1,T2,T3,T4,Cadenza.Maybe{TResult}}" />
        ///   array containing the conversion routines to use to convert 
        ///   the current <see cref="T:Cadenza.Tuple{T1,T2,T3,T4}" /> instance into a 
        ///   <typeparamref name="TResult" /> value.
        /// </param>
        /// <summary>
        ///   Converts the current <see cref="T:Cadenza.Tuple{T1,T2,T3,T4}" /> instance into a <typeparamref name="TResult"/>.
        /// </summary>
        /// <returns>
        ///   The <typeparamref name="TResult"/> returned by one of the <paramref name="matchers"/>.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///    <block subset="none" type="behaviors">
        ///     <para>
        ///      The current <see cref="T:Cadenza.Tuple{T1,T2,T3,T4}" /> instance is converted into a 
        ///      <typeparamref name="TResult" /> instance by trying each
        ///      <see cref="T:System.Func{T1,T2,T3,T4,Cadenza.Maybe{TResult}}" />
        ///      within <paramref name="matchers" />.
        ///     </para>
        ///     <para>
        ///      This method returns 
        ///      <see cref="P:Cadenza.Maybe{TResult}.Value" /> 
        ///      for the first delegate to return a
        ///      <see cref="T:Cadenza.Maybe{TResult}" /> instance
        ///      where <see cref="P:Cadenza.Maybe{TResult}.HasValue" />
        ///      is <see langword="true" />.
        ///     </para>
        ///     <para>
        ///      If no <see cref="T:System.Func{T1,T2,T3,T4,Cadenza.Maybe{TResult}}" />
        ///      returns a 
        ///      <see cref="T:Cadenza.Maybe{TResult}" /> instance
        ///      where <see cref="P:Cadenza.Maybe{TResult}.HasValue" />
        ///      is <see langword="true" />, then an
        ///      <see cref="T:System.InvalidOperationException" /> is thrown.
        ///     </para>
        ///    </block>
        ///    <code lang="C#">
        ///   var    a = Tuple.Create (1, 2);
        ///   string b = a.Match (
        ///       (t, v) =&gt; Match.When ( t + v == 3, "foo!"),
        ///       (t, v) =&gt; "*default*".Just ());
        ///   Console.WriteLine (b);  // prints "foo!"</code>
        ///   </para>
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <para>
        ///     <paramref name="self" /> is <see langword="null" />.
        ///   </para>
        ///   <para>-or-</para>
        ///   <para>
        ///     <paramref name="matchers" /> is <see langword="null" />.
        ///   </para>
        /// </exception>
        /// <exception cref="T:System.InvalidOperationException">
        ///   None of the 
        ///   <see cref="T:System.Func{TSource,Cadenza.Maybe{TResult}}" />
        ///   delegates within <paramref name="matchers" /> returned a 
        ///   <see cref="T:Cadenza.Maybe{TResult}" /> instance where
        ///   <see cref="P:Cadenza.Maybe{TResult}.HasValue" /> was
        ///   <see langword="true" />.
        /// </exception>
        public static TResult Match<T1, T2, T3, T4, TResult>(this Tuple<T1, T2, T3, T4> self, params System.Func<T1, T2, T3, T4, Cadenza.Maybe<TResult>>[] matchers)
        
        
        
        
        
        {
            Check.Self(self);
            if ((matchers == null))
            {
                throw new System.ArgumentNullException("matchers");
            }
            foreach (var m in matchers) {
              var r = m (self.Item1, self.Item2, self.Item3, self.Item4);
              if (r.HasValue)
                return r.Value;
            }
            throw new System.InvalidOperationException("no match");
        }
        
        /// <typeparam name="T1">
        ///   The first value type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   The second value type.
        /// </typeparam>
        /// <typeparam name="T3">
        ///   The third value type.
        /// </typeparam>
        /// <typeparam name="T4">
        ///   The fourth value type.
        /// </typeparam>
        /// <param name="self">
        ///   A <see cref="T:Cadenza.Tuple{T1,T2,T3,T4}" /> to convert into an <see cref="T:System.Collections.Generic.IEnumerable{System.Object}"/>.
        /// </param>
        /// <summary>
        ///   Converts the <see cref="T:Cadenza.Tuple{T1,T2,T3,T4}" /> into a <see cref="T:System.Collections.Generic.IEnumerable{System.Object}"/>.
        /// </summary>
        /// <returns>
        ///   A <see cref="T:System.Collections.Generic.IEnumerable{System.Object}"/>.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///    <block subset="none" type="behaviors">
        ///     Passes the values <see cref="P:Cadenza.Tuple`4.Item1"/>, <see cref="P:Cadenza.Tuple`4.Item2"/>, <see cref="P:Cadenza.Tuple`4.Item3"/>, <see cref="P:Cadenza.Tuple`4.Item4"/> to 
        ///     <paramref name="func"/>, returning the value produced by 
        ///     <paramref name="func"/>.
        ///    </block>
        ///   </para>
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Collections.Generic.IEnumerable<object> ToEnumerable<T1, T2, T3, T4>(this Tuple<T1, T2, T3, T4> self)
        
        
        
        
        {
            Check.Self(self);
            return TupleCoda.CreateToEnumerableIterator(self);
        }
        
        private static System.Collections.Generic.IEnumerable<object> CreateToEnumerableIterator<T1, T2, T3, T4>(Tuple<T1, T2, T3, T4> self)
        
        
        
        
        {
            yield return self.Item1;
            yield return self.Item2;
            yield return self.Item3;
            yield return self.Item4;
        }
    }
}
