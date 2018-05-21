// 
// FuncCoda.g.cs: Extension methods for Func<...> delegate types.
// 
// GENERATED CODE: DO NOT EDIT.
// 
// To regenerate this code, execute: Delegates.exe -n 4 -o Cadenza/Delegates.cs
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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq.Expressions;
    
    
    /// <summary>
    ///   Provides extension methods on <see cref="T:System.Func{TResult}"/>
    ///   and related delegates.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///    <see cref="T:Cadenza.FuncCoda" /> provides methods methods for:
    ///   </para>
    ///   <list type="bullet">
    ///    <item><term>
    ///     Delegate currying and partial application (<see cref="M:Cadenza.DelegateCoda.Curry" />)
    ///    </term></item>
    ///    <item><term>
    ///     Delegate composition (<see cref="M:Cadenza.DelegateCoda.Compose" />)
    ///    </term></item>
    ///    <item><term>
    ///     Timing generation (<see cref="M:Cadenza.DelegateCoda.Timings" />)
    ///    </term></item>
    ///   </list>
    ///   <para>
    ///    Currying via partial application is a way to easily transform 
    ///    functions which accept N arguments into functions which accept 
    ///    N-1 arguments, by "fixing" arguments with a value.
    ///   </para>
    ///   <code lang="C#">
    ///   // partial application:
    ///   Func&lt;int,int,int,int&gt; function = (int a, int b, int c) => a + b + c;
    ///   Func&lt;int,int,int&gt;     f_3      = function.Curry (3);
    ///   Func&lt;int&gt;             f_321    = function.Curry (3, 2, 1);
    ///   Console.WriteLine (f_3 (2, 1));  // prints (3 + 2 + 1) == "6"
    ///   Console.WriteLine (f_321 ());    // prints (3 + 2 + 1) == "6"</code>
    ///   <para>
    ///    "Traditional" currying converts a delegate that accepts N arguments
    ///    into a delegate which accepts only one argument, but when invoked may 
    ///    return a further delegate (etc.) until the final value is returned.
    ///   </para>
    ///   <code lang="C#">
    ///   // traditional currying:
    ///   Func&lt;int, Func&lt;int, Func&lt;int, int&gt;&gt;&gt; curry = function.Curry ();
    ///   Func&lt;int, Func&lt;int, int&gt;&gt;            fc_1  = curry (1);
    ///   Func&lt;int, int&gt;                       fc_12 = fc_1 (2);
    ///   Console.WriteLine (fc_12 (3));        // prints (3 + 2 + 1) == "6"
    ///   Console.WriteLine (curry (3)(2)(1));  // prints (3 + 2 + 1) == "6"</code>
    ///   <para>
    ///    Composition is a way to easy chain (or pipe) together multiple delegates
    ///    so that the return value of a "composer" delegate is used as the input 
    ///    parameter for the chained delegate:
    ///   </para>
    ///   <code lang="C#">
    ///   Func&lt;int,string> tostring = Lambda.F ((int n) => n.ToString ());
    ///   Func&lt;int, int>    doubler = Lambda.F ((int n) => n * 2);
    ///   Func&lt;int, string>
    ///        double_then_tostring = tostring.Compose (doubler);
    ///   Console.WriteLine (double_then_tostring (5));
    ///       // Prints "10";</code>
    ///   <para>
    ///    All possible argument and return delegate permutations are provided
    ///    for the <see cref="T:System.Func{T,TResult}"/> and related types.
    ///   </para>
    /// </remarks>
    public partial class FuncCoda
    {
        
        /// <typeparam name="T">
        ///   The <see cref="T:System.Func{T}" /> return type, and <see cref="T:System.Func{T,TResult}" /> argument type.
        /// </typeparam>
        /// <typeparam name="TResult">
        ///   The <see cref="T:System.Func{T,TResult}" /> return type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Func{TResult}" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Func{T,TResult}" /> to compose.
        /// </param>
        /// <param name="composer">
        ///   The <see cref="T:System.Func{T}" /> to compose with <paramref name="self" />.
        /// </param>
        /// <returns>
        ///   Returns a <see cref="T:System.Func{TResult}" /> which, when invoked, will
        ///   invoke <paramref name="composer"/> and pass the return value of
        ///   <paramref name="composer" /> to <paramref name="self" />.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///    Composition is useful for chaining delegates together, so that the
        ///    return value of <paramref name="composer" /> is automatically used as
        ///    the input parameter for <paramref name="self" />.
        ///   </para>
        ///   <code lang="C#">
        ///   Func&lt;int,string> tostring = Lambda.F ((int n) => n.ToString ());
        ///   Func&lt;int, int>    doubler = Lambda.F ((int n) => n * 2);
        ///   Func&lt;int, string>
        ///        double_then_tostring = tostring.Compose (doubler);
        ///   Console.WriteLine (double_then_tostring (5));
        ///       // Prints "10";</code>
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <para>
        ///     <paramref name="self" /> is <see langword="null" />.
        ///   </para>
        ///   <para>-or-</para>
        ///   <para>
        ///     <paramref name="composer" /> is <see langword="null" />.
        ///   </para>
        /// </exception>
        public static System.Func<TResult> Compose<T, TResult>(this System.Func<T, TResult> self, System.Func<T> composer)
        
        
        {
            Check.Self(self);
            Check.Composer(composer);
            return () => self (composer ());
        }
        
        /// <typeparam name="T">
        ///   A <see cref="T:System.Func{T,TResult}" /> parameter type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Func{TResult}" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Func{T,TResult}" /> to curry.
        /// </param>
        /// <param name="value">
        ///   A value of type <typeparamref name="T"/> to fix.
        /// </param>
        /// <returns>
        ///   Returns a <see cref="T:System.Func{TResult}" /> which, when invoked, will
        ///   invoke <paramref name="self"/> along with the provided fixed parameters.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Func<TResult> Curry<T, TResult>(this System.Func<T, TResult> self, T value)
        
        
        {
            Check.Self(self);
            return () => self (value);
        }
        
        /// <typeparam name="T">
        ///   A <see cref="T:System.Func{T,TResult}" /> parameter type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Func{TResult}" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Func{T,TResult}" /> to curry.
        /// </param>
        /// <param name="values">
        ///   A value of type <see cref="T:Tuple{T}" /> which contains the values to fix.
        /// </param>
        /// <returns>
        ///   Returns a <see cref="T:System.Func{TResult}" /> which, when invoked, will
        ///   invoke <paramref name="self"/> along with the provided fixed parameters.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Func<TResult> Curry<T, TResult>(this System.Func<T, TResult> self, Tuple<T> values)
        
        
        {
            Check.Self(self);
            return () => self (values.Item1);
        }
        
        // Currying method idea courtesy of:
        // http://blogs.msdn.com/wesdyer/archive/2007/01/29/currying-and-partial-function-application.aspx
        /// <typeparam name="T">
        ///   The first value type.
        /// </typeparam>
        /// <typeparam name="TResult">
        ///   The return value type.
        /// </typeparam>
        /// <param name="self">
        ///   The <see cref="T:System.Func{T,TResult}" /> to curry.
        /// </param>
        /// <summary>
        ///   Creates a <see cref="T:System.Func{T,TResult}" /> for currying.
        /// </summary>
        /// <returns>
        ///   A <see cref="T:System.Func{T,TResult}" /> which, when invoked, will 
        ///   
        ///   invoke <paramref name="self"/>.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///    This is the more "traditional" view of currying, turning a method
        ///    which takes <c>(X * Y)-&gt;Z</c> (i.e. separate arguments) into a
        ///    <c>X -&gt; (Y -&gt; Z)</c> (that is a "chain" of nested Funcs such that
        ///    you provide only one argument to each Func until you provide enough
        ///    arguments to invoke the original method).
        ///   </para>
        ///   <code lang="C#">
        ///   Func&lt;int,int,int,int&gt; function = (int a, int b, int c) =&gt; a + b + c;
        ///   Func&lt;int,Func&lt;int,Func&lt;int, int&gt;&gt;&gt; curry = function.Curry ();
        ///   Assert.AreEqual(6, curry (3)(2)(1));</code>
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Func<T, TResult> Curry<T, TResult>(this System.Func<T, TResult> self)
        
        
        {
            Check.Self(self);
            return value => self (value);
        }
        
        /// <typeparam name="T2">
        ///   The <see cref="T:System.Func{T1,T2}" /> return type, and <see cref="T:System.Func{T2,TResult}" /> argument type.
        /// </typeparam>
        /// <typeparam name="TResult">
        ///   The <see cref="T:System.Func{T2,TResult}" /> return type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Func{T1,TResult}" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Func{T2,TResult}" /> to compose.
        /// </param>
        /// <param name="composer">
        ///   The <see cref="T:System.Func{T1,T2}" /> to compose with <paramref name="self" />.
        /// </param>
        /// <returns>
        ///   Returns a <see cref="T:System.Func{T1,TResult}" /> which, when invoked, will
        ///   invoke <paramref name="composer"/> and pass the return value of
        ///   <paramref name="composer" /> to <paramref name="self" />.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///    Composition is useful for chaining delegates together, so that the
        ///    return value of <paramref name="composer" /> is automatically used as
        ///    the input parameter for <paramref name="self" />.
        ///   </para>
        ///   <code lang="C#">
        ///   Func&lt;int,string> tostring = Lambda.F ((int n) => n.ToString ());
        ///   Func&lt;int, int>    doubler = Lambda.F ((int n) => n * 2);
        ///   Func&lt;int, string>
        ///        double_then_tostring = tostring.Compose (doubler);
        ///   Console.WriteLine (double_then_tostring (5));
        ///       // Prints "10";</code>
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <para>
        ///     <paramref name="self" /> is <see langword="null" />.
        ///   </para>
        ///   <para>-or-</para>
        ///   <para>
        ///     <paramref name="composer" /> is <see langword="null" />.
        ///   </para>
        /// </exception>
        public static System.Func<T1, TResult> Compose<T1, T2, TResult>(this System.Func<T2, TResult> self, System.Func<T1, T2> composer)
        
        
        
        {
            Check.Self(self);
            Check.Composer(composer);
            return (value) => self (composer (value));
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Func{T1,T2,TResult}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Func{T1,T2,TResult}" /> parameter type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Func{T2,TResult}" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Func{T1,T2,TResult}" /> to curry.
        /// </param>
        /// <param name="value1">
        ///   A value of type <typeparamref name="T1"/> to fix.
        /// </param>
        /// <returns>
        ///   Returns a <see cref="T:System.Func{T2,TResult}" /> which, when invoked, will
        ///   invoke <paramref name="self"/> along with the provided fixed parameters.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Func<T2, TResult> Curry<T1, T2, TResult>(this System.Func<T1, T2, TResult> self, T1 value1)
        
        
        
        {
            Check.Self(self);
            return (value2) => self (value1, value2);
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Func{T1,T2,TResult}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Func{T1,T2,TResult}" /> parameter type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Func{TResult}" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Func{T1,T2,TResult}" /> to curry.
        /// </param>
        /// <param name="value1">
        ///   A value of type <typeparamref name="T1"/> to fix.
        /// </param>
        /// <param name="value2">
        ///   A value of type <typeparamref name="T2"/> to fix.
        /// </param>
        /// <returns>
        ///   Returns a <see cref="T:System.Func{TResult}" /> which, when invoked, will
        ///   invoke <paramref name="self"/> along with the provided fixed parameters.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Func<TResult> Curry<T1, T2, TResult>(this System.Func<T1, T2, TResult> self, T1 value1, T2 value2)
        
        
        
        {
            Check.Self(self);
            return () => self (value1, value2);
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Func{T1,T2,TResult}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Func{T1,T2,TResult}" /> parameter type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Func{T2,TResult}" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Func{T1,T2,TResult}" /> to curry.
        /// </param>
        /// <param name="values">
        ///   A value of type <see cref="T:Tuple{T1}" /> which contains the values to fix.
        /// </param>
        /// <returns>
        ///   Returns a <see cref="T:System.Func{T2,TResult}" /> which, when invoked, will
        ///   invoke <paramref name="self"/> along with the provided fixed parameters.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Func<T2, TResult> Curry<T1, T2, TResult>(this System.Func<T1, T2, TResult> self, Tuple<T1> values)
        
        
        
        {
            Check.Self(self);
            return (value2) => self (values.Item1, value2);
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Func{T1,T2,TResult}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Func{T1,T2,TResult}" /> parameter type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Func{TResult}" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Func{T1,T2,TResult}" /> to curry.
        /// </param>
        /// <param name="values">
        ///   A value of type <see cref="T:Tuple{T1,T2}" /> which contains the values to fix.
        /// </param>
        /// <returns>
        ///   Returns a <see cref="T:System.Func{TResult}" /> which, when invoked, will
        ///   invoke <paramref name="self"/> along with the provided fixed parameters.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Func<TResult> Curry<T1, T2, TResult>(this System.Func<T1, T2, TResult> self, Tuple<T1, T2> values)
        
        
        
        {
            Check.Self(self);
            return () => self (values.Item1, values.Item2);
        }
        
        // Currying method idea courtesy of:
        // http://blogs.msdn.com/wesdyer/archive/2007/01/29/currying-and-partial-function-application.aspx
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
        ///   The <see cref="T:System.Func{T1,T2,TResult}" /> to curry.
        /// </param>
        /// <summary>
        ///   Creates a <see cref="T:System.Func{T1,System.Func{T2,TResult}}" /> for currying.
        /// </summary>
        /// <returns>
        ///   A <see cref="T:System.Func{T1,System.Func{T2,TResult}}" /> which, when invoked, will 
        ///   return a <see cref="T:System.Func{T2,TResult}" /> which, when invoked, will 
        ///   invoke <paramref name="self"/>.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///    This is the more "traditional" view of currying, turning a method
        ///    which takes <c>(X * Y)-&gt;Z</c> (i.e. separate arguments) into a
        ///    <c>X -&gt; (Y -&gt; Z)</c> (that is a "chain" of nested Funcs such that
        ///    you provide only one argument to each Func until you provide enough
        ///    arguments to invoke the original method).
        ///   </para>
        ///   <code lang="C#">
        ///   Func&lt;int,int,int,int&gt; function = (int a, int b, int c) =&gt; a + b + c;
        ///   Func&lt;int,Func&lt;int,Func&lt;int, int&gt;&gt;&gt; curry = function.Curry ();
        ///   Assert.AreEqual(6, curry (3)(2)(1));</code>
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Func<T1, System.Func<T2, TResult>> Curry<T1, T2, TResult>(this System.Func<T1, T2, TResult> self)
        
        
        
        {
            Check.Self(self);
            return value1 => value2 => self (value1, value2);
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Func{T1,T2,T3}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T3">
        ///   The <see cref="T:System.Func{T1,T2,T3}" /> return type, and <see cref="T:System.Func{T3,TResult}" /> argument type.
        /// </typeparam>
        /// <typeparam name="TResult">
        ///   The <see cref="T:System.Func{T3,TResult}" /> return type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Func{T1,T2,TResult}" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Func{T3,TResult}" /> to compose.
        /// </param>
        /// <param name="composer">
        ///   The <see cref="T:System.Func{T1,T2,T3}" /> to compose with <paramref name="self" />.
        /// </param>
        /// <returns>
        ///   Returns a <see cref="T:System.Func{T1,T2,TResult}" /> which, when invoked, will
        ///   invoke <paramref name="composer"/> and pass the return value of
        ///   <paramref name="composer" /> to <paramref name="self" />.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///    Composition is useful for chaining delegates together, so that the
        ///    return value of <paramref name="composer" /> is automatically used as
        ///    the input parameter for <paramref name="self" />.
        ///   </para>
        ///   <code lang="C#">
        ///   Func&lt;int,string> tostring = Lambda.F ((int n) => n.ToString ());
        ///   Func&lt;int, int>    doubler = Lambda.F ((int n) => n * 2);
        ///   Func&lt;int, string>
        ///        double_then_tostring = tostring.Compose (doubler);
        ///   Console.WriteLine (double_then_tostring (5));
        ///       // Prints "10";</code>
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <para>
        ///     <paramref name="self" /> is <see langword="null" />.
        ///   </para>
        ///   <para>-or-</para>
        ///   <para>
        ///     <paramref name="composer" /> is <see langword="null" />.
        ///   </para>
        /// </exception>
        public static System.Func<T1, T2, TResult> Compose<T1, T2, T3, TResult>(this System.Func<T3, TResult> self, System.Func<T1, T2, T3> composer)
        
        
        
        
        {
            Check.Self(self);
            Check.Composer(composer);
            return (value1, value2) => self (composer (value1, value2));
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Func{T1,T2,T3,TResult}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Func{T1,T2,T3,TResult}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T3">
        ///   A <see cref="T:System.Func{T1,T2,T3,TResult}" /> parameter type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Func{T2,T3,TResult}" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Func{T1,T2,T3,TResult}" /> to curry.
        /// </param>
        /// <param name="value1">
        ///   A value of type <typeparamref name="T1"/> to fix.
        /// </param>
        /// <returns>
        ///   Returns a <see cref="T:System.Func{T2,T3,TResult}" /> which, when invoked, will
        ///   invoke <paramref name="self"/> along with the provided fixed parameters.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Func<T2, T3, TResult> Curry<T1, T2, T3, TResult>(this System.Func<T1, T2, T3, TResult> self, T1 value1)
        
        
        
        
        {
            Check.Self(self);
            return (value2, value3) => self (value1, value2, value3);
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Func{T1,T2,T3,TResult}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Func{T1,T2,T3,TResult}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T3">
        ///   A <see cref="T:System.Func{T1,T2,T3,TResult}" /> parameter type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Func{T3,TResult}" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Func{T1,T2,T3,TResult}" /> to curry.
        /// </param>
        /// <param name="value1">
        ///   A value of type <typeparamref name="T1"/> to fix.
        /// </param>
        /// <param name="value2">
        ///   A value of type <typeparamref name="T2"/> to fix.
        /// </param>
        /// <returns>
        ///   Returns a <see cref="T:System.Func{T3,TResult}" /> which, when invoked, will
        ///   invoke <paramref name="self"/> along with the provided fixed parameters.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Func<T3, TResult> Curry<T1, T2, T3, TResult>(this System.Func<T1, T2, T3, TResult> self, T1 value1, T2 value2)
        
        
        
        
        {
            Check.Self(self);
            return (value3) => self (value1, value2, value3);
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Func{T1,T2,T3,TResult}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Func{T1,T2,T3,TResult}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T3">
        ///   A <see cref="T:System.Func{T1,T2,T3,TResult}" /> parameter type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Func{TResult}" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Func{T1,T2,T3,TResult}" /> to curry.
        /// </param>
        /// <param name="value1">
        ///   A value of type <typeparamref name="T1"/> to fix.
        /// </param>
        /// <param name="value2">
        ///   A value of type <typeparamref name="T2"/> to fix.
        /// </param>
        /// <param name="value3">
        ///   A value of type <typeparamref name="T3"/> to fix.
        /// </param>
        /// <returns>
        ///   Returns a <see cref="T:System.Func{TResult}" /> which, when invoked, will
        ///   invoke <paramref name="self"/> along with the provided fixed parameters.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Func<TResult> Curry<T1, T2, T3, TResult>(this System.Func<T1, T2, T3, TResult> self, T1 value1, T2 value2, T3 value3)
        
        
        
        
        {
            Check.Self(self);
            return () => self (value1, value2, value3);
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Func{T1,T2,T3,TResult}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Func{T1,T2,T3,TResult}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T3">
        ///   A <see cref="T:System.Func{T1,T2,T3,TResult}" /> parameter type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Func{T2,T3,TResult}" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Func{T1,T2,T3,TResult}" /> to curry.
        /// </param>
        /// <param name="values">
        ///   A value of type <see cref="T:Tuple{T1}" /> which contains the values to fix.
        /// </param>
        /// <returns>
        ///   Returns a <see cref="T:System.Func{T2,T3,TResult}" /> which, when invoked, will
        ///   invoke <paramref name="self"/> along with the provided fixed parameters.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Func<T2, T3, TResult> Curry<T1, T2, T3, TResult>(this System.Func<T1, T2, T3, TResult> self, Tuple<T1> values)
        
        
        
        
        {
            Check.Self(self);
            return (value2, value3) => self (values.Item1, value2, value3);
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Func{T1,T2,T3,TResult}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Func{T1,T2,T3,TResult}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T3">
        ///   A <see cref="T:System.Func{T1,T2,T3,TResult}" /> parameter type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Func{T3,TResult}" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Func{T1,T2,T3,TResult}" /> to curry.
        /// </param>
        /// <param name="values">
        ///   A value of type <see cref="T:Tuple{T1,T2}" /> which contains the values to fix.
        /// </param>
        /// <returns>
        ///   Returns a <see cref="T:System.Func{T3,TResult}" /> which, when invoked, will
        ///   invoke <paramref name="self"/> along with the provided fixed parameters.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Func<T3, TResult> Curry<T1, T2, T3, TResult>(this System.Func<T1, T2, T3, TResult> self, Tuple<T1, T2> values)
        
        
        
        
        {
            Check.Self(self);
            return (value3) => self (values.Item1, values.Item2, value3);
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Func{T1,T2,T3,TResult}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Func{T1,T2,T3,TResult}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T3">
        ///   A <see cref="T:System.Func{T1,T2,T3,TResult}" /> parameter type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Func{TResult}" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Func{T1,T2,T3,TResult}" /> to curry.
        /// </param>
        /// <param name="values">
        ///   A value of type <see cref="T:Tuple{T1,T2,T3}" /> which contains the values to fix.
        /// </param>
        /// <returns>
        ///   Returns a <see cref="T:System.Func{TResult}" /> which, when invoked, will
        ///   invoke <paramref name="self"/> along with the provided fixed parameters.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Func<TResult> Curry<T1, T2, T3, TResult>(this System.Func<T1, T2, T3, TResult> self, Tuple<T1, T2, T3> values)
        
        
        
        
        {
            Check.Self(self);
            return () => self (values.Item1, values.Item2, values.Item3);
        }
        
        // Currying method idea courtesy of:
        // http://blogs.msdn.com/wesdyer/archive/2007/01/29/currying-and-partial-function-application.aspx
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
        ///   The <see cref="T:System.Func{T1,T2,T3,TResult}" /> to curry.
        /// </param>
        /// <summary>
        ///   Creates a <see cref="T:System.Func{T1,System.Func{T2,System.Func{T3,TResult}}}" /> for currying.
        /// </summary>
        /// <returns>
        ///   A <see cref="T:System.Func{T1,System.Func{T2,System.Func{T3,TResult}}}" /> which, when invoked, will 
        ///   return a <see cref="T:System.Func{T2,System.Func{T3,TResult}}" /> which, when invoked, will return a <see cref="T:System.Func{T3,TResult}" /> which, when invoked, will 
        ///   invoke <paramref name="self"/>.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///    This is the more "traditional" view of currying, turning a method
        ///    which takes <c>(X * Y)-&gt;Z</c> (i.e. separate arguments) into a
        ///    <c>X -&gt; (Y -&gt; Z)</c> (that is a "chain" of nested Funcs such that
        ///    you provide only one argument to each Func until you provide enough
        ///    arguments to invoke the original method).
        ///   </para>
        ///   <code lang="C#">
        ///   Func&lt;int,int,int,int&gt; function = (int a, int b, int c) =&gt; a + b + c;
        ///   Func&lt;int,Func&lt;int,Func&lt;int, int&gt;&gt;&gt; curry = function.Curry ();
        ///   Assert.AreEqual(6, curry (3)(2)(1));</code>
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Func<T1, System.Func<T2, System.Func<T3, TResult>>> Curry<T1, T2, T3, TResult>(this System.Func<T1, T2, T3, TResult> self)
        
        
        
        
        {
            Check.Self(self);
            return value1 => value2 => value3 => self (value1, value2, value3);
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Func{T1,T2,T3,T4}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Func{T1,T2,T3,T4}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T4">
        ///   The <see cref="T:System.Func{T1,T2,T3,T4}" /> return type, and <see cref="T:System.Func{T4,TResult}" /> argument type.
        /// </typeparam>
        /// <typeparam name="TResult">
        ///   The <see cref="T:System.Func{T4,TResult}" /> return type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Func{T1,T2,T3,TResult}" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Func{T4,TResult}" /> to compose.
        /// </param>
        /// <param name="composer">
        ///   The <see cref="T:System.Func{T1,T2,T3,T4}" /> to compose with <paramref name="self" />.
        /// </param>
        /// <returns>
        ///   Returns a <see cref="T:System.Func{T1,T2,T3,TResult}" /> which, when invoked, will
        ///   invoke <paramref name="composer"/> and pass the return value of
        ///   <paramref name="composer" /> to <paramref name="self" />.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///    Composition is useful for chaining delegates together, so that the
        ///    return value of <paramref name="composer" /> is automatically used as
        ///    the input parameter for <paramref name="self" />.
        ///   </para>
        ///   <code lang="C#">
        ///   Func&lt;int,string> tostring = Lambda.F ((int n) => n.ToString ());
        ///   Func&lt;int, int>    doubler = Lambda.F ((int n) => n * 2);
        ///   Func&lt;int, string>
        ///        double_then_tostring = tostring.Compose (doubler);
        ///   Console.WriteLine (double_then_tostring (5));
        ///       // Prints "10";</code>
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <para>
        ///     <paramref name="self" /> is <see langword="null" />.
        ///   </para>
        ///   <para>-or-</para>
        ///   <para>
        ///     <paramref name="composer" /> is <see langword="null" />.
        ///   </para>
        /// </exception>
        public static System.Func<T1, T2, T3, TResult> Compose<T1, T2, T3, T4, TResult>(this System.Func<T4, TResult> self, System.Func<T1, T2, T3, T4> composer)
        
        
        
        
        
        {
            Check.Self(self);
            Check.Composer(composer);
            return (value1, value2, value3) => self (composer (value1, value2, value3));
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T3">
        ///   A <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T4">
        ///   A <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> parameter type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Func{T2,T3,T4,TResult}" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> to curry.
        /// </param>
        /// <param name="value1">
        ///   A value of type <typeparamref name="T1"/> to fix.
        /// </param>
        /// <returns>
        ///   Returns a <see cref="T:System.Func{T2,T3,T4,TResult}" /> which, when invoked, will
        ///   invoke <paramref name="self"/> along with the provided fixed parameters.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Func<T2, T3, T4, TResult> Curry<T1, T2, T3, T4, TResult>(this System.Func<T1, T2, T3, T4, TResult> self, T1 value1)
        
        
        
        
        
        {
            Check.Self(self);
            return (value2, value3, value4) => self (value1, value2, value3, value4);
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T3">
        ///   A <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T4">
        ///   A <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> parameter type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Func{T3,T4,TResult}" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> to curry.
        /// </param>
        /// <param name="value1">
        ///   A value of type <typeparamref name="T1"/> to fix.
        /// </param>
        /// <param name="value2">
        ///   A value of type <typeparamref name="T2"/> to fix.
        /// </param>
        /// <returns>
        ///   Returns a <see cref="T:System.Func{T3,T4,TResult}" /> which, when invoked, will
        ///   invoke <paramref name="self"/> along with the provided fixed parameters.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Func<T3, T4, TResult> Curry<T1, T2, T3, T4, TResult>(this System.Func<T1, T2, T3, T4, TResult> self, T1 value1, T2 value2)
        
        
        
        
        
        {
            Check.Self(self);
            return (value3, value4) => self (value1, value2, value3, value4);
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T3">
        ///   A <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T4">
        ///   A <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> parameter type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Func{T4,TResult}" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> to curry.
        /// </param>
        /// <param name="value1">
        ///   A value of type <typeparamref name="T1"/> to fix.
        /// </param>
        /// <param name="value2">
        ///   A value of type <typeparamref name="T2"/> to fix.
        /// </param>
        /// <param name="value3">
        ///   A value of type <typeparamref name="T3"/> to fix.
        /// </param>
        /// <returns>
        ///   Returns a <see cref="T:System.Func{T4,TResult}" /> which, when invoked, will
        ///   invoke <paramref name="self"/> along with the provided fixed parameters.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Func<T4, TResult> Curry<T1, T2, T3, T4, TResult>(this System.Func<T1, T2, T3, T4, TResult> self, T1 value1, T2 value2, T3 value3)
        
        
        
        
        
        {
            Check.Self(self);
            return (value4) => self (value1, value2, value3, value4);
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T3">
        ///   A <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T4">
        ///   A <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> parameter type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Func{TResult}" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> to curry.
        /// </param>
        /// <param name="value1">
        ///   A value of type <typeparamref name="T1"/> to fix.
        /// </param>
        /// <param name="value2">
        ///   A value of type <typeparamref name="T2"/> to fix.
        /// </param>
        /// <param name="value3">
        ///   A value of type <typeparamref name="T3"/> to fix.
        /// </param>
        /// <param name="value4">
        ///   A value of type <typeparamref name="T4"/> to fix.
        /// </param>
        /// <returns>
        ///   Returns a <see cref="T:System.Func{TResult}" /> which, when invoked, will
        ///   invoke <paramref name="self"/> along with the provided fixed parameters.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Func<TResult> Curry<T1, T2, T3, T4, TResult>(this System.Func<T1, T2, T3, T4, TResult> self, T1 value1, T2 value2, T3 value3, T4 value4)
        
        
        
        
        
        {
            Check.Self(self);
            return () => self (value1, value2, value3, value4);
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T3">
        ///   A <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T4">
        ///   A <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> parameter type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Func{T2,T3,T4,TResult}" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> to curry.
        /// </param>
        /// <param name="values">
        ///   A value of type <see cref="T:Tuple{T1}" /> which contains the values to fix.
        /// </param>
        /// <returns>
        ///   Returns a <see cref="T:System.Func{T2,T3,T4,TResult}" /> which, when invoked, will
        ///   invoke <paramref name="self"/> along with the provided fixed parameters.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Func<T2, T3, T4, TResult> Curry<T1, T2, T3, T4, TResult>(this System.Func<T1, T2, T3, T4, TResult> self, Tuple<T1> values)
        
        
        
        
        
        {
            Check.Self(self);
            return (value2, value3, value4) => self (values.Item1, value2, value3, value4);
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T3">
        ///   A <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T4">
        ///   A <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> parameter type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Func{T3,T4,TResult}" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> to curry.
        /// </param>
        /// <param name="values">
        ///   A value of type <see cref="T:Tuple{T1,T2}" /> which contains the values to fix.
        /// </param>
        /// <returns>
        ///   Returns a <see cref="T:System.Func{T3,T4,TResult}" /> which, when invoked, will
        ///   invoke <paramref name="self"/> along with the provided fixed parameters.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Func<T3, T4, TResult> Curry<T1, T2, T3, T4, TResult>(this System.Func<T1, T2, T3, T4, TResult> self, Tuple<T1, T2> values)
        
        
        
        
        
        {
            Check.Self(self);
            return (value3, value4) => self (values.Item1, values.Item2, value3, value4);
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T3">
        ///   A <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T4">
        ///   A <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> parameter type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Func{T4,TResult}" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> to curry.
        /// </param>
        /// <param name="values">
        ///   A value of type <see cref="T:Tuple{T1,T2,T3}" /> which contains the values to fix.
        /// </param>
        /// <returns>
        ///   Returns a <see cref="T:System.Func{T4,TResult}" /> which, when invoked, will
        ///   invoke <paramref name="self"/> along with the provided fixed parameters.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Func<T4, TResult> Curry<T1, T2, T3, T4, TResult>(this System.Func<T1, T2, T3, T4, TResult> self, Tuple<T1, T2, T3> values)
        
        
        
        
        
        {
            Check.Self(self);
            return (value4) => self (values.Item1, values.Item2, values.Item3, value4);
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T3">
        ///   A <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T4">
        ///   A <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> parameter type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Func{TResult}" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> to curry.
        /// </param>
        /// <param name="values">
        ///   A value of type <see cref="T:Tuple{T1,T2,T3,T4}" /> which contains the values to fix.
        /// </param>
        /// <returns>
        ///   Returns a <see cref="T:System.Func{TResult}" /> which, when invoked, will
        ///   invoke <paramref name="self"/> along with the provided fixed parameters.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Func<TResult> Curry<T1, T2, T3, T4, TResult>(this System.Func<T1, T2, T3, T4, TResult> self, Tuple<T1, T2, T3, T4> values)
        
        
        
        
        
        {
            Check.Self(self);
            return () => self (values.Item1, values.Item2, values.Item3, values.Item4);
        }
        
        // Currying method idea courtesy of:
        // http://blogs.msdn.com/wesdyer/archive/2007/01/29/currying-and-partial-function-application.aspx
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
        ///   The <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> to curry.
        /// </param>
        /// <summary>
        ///   Creates a <see cref="T:System.Func{T1,System.Func{T2,System.Func{T3,System.Func{T4,TResult}}}}" /> for currying.
        /// </summary>
        /// <returns>
        ///   A <see cref="T:System.Func{T1,System.Func{T2,System.Func{T3,System.Func{T4,TResult}}}}" /> which, when invoked, will 
        ///   return a <see cref="T:System.Func{T2,System.Func{T3,System.Func{T4,TResult}}}" /> which, when invoked, will return a <see cref="T:System.Func{T3,System.Func{T4,TResult}}" /> which, when invoked, will return a <see cref="T:System.Func{T4,TResult}" /> which, when invoked, will 
        ///   invoke <paramref name="self"/>.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///    This is the more "traditional" view of currying, turning a method
        ///    which takes <c>(X * Y)-&gt;Z</c> (i.e. separate arguments) into a
        ///    <c>X -&gt; (Y -&gt; Z)</c> (that is a "chain" of nested Funcs such that
        ///    you provide only one argument to each Func until you provide enough
        ///    arguments to invoke the original method).
        ///   </para>
        ///   <code lang="C#">
        ///   Func&lt;int,int,int,int&gt; function = (int a, int b, int c) =&gt; a + b + c;
        ///   Func&lt;int,Func&lt;int,Func&lt;int, int&gt;&gt;&gt; curry = function.Curry ();
        ///   Assert.AreEqual(6, curry (3)(2)(1));</code>
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Func<T1, System.Func<T2, System.Func<T3, System.Func<T4, TResult>>>> Curry<T1, T2, T3, T4, TResult>(this System.Func<T1, T2, T3, T4, TResult> self)
        
        
        
        
        
        {
            Check.Self(self);
            return value1 => value2 => value3 => value4 => self (value1, value2, value3, value4);
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Func{T1,T2,T3,T4,T5}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Func{T1,T2,T3,T4,T5}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T3">
        ///   A <see cref="T:System.Func{T1,T2,T3,T4,T5}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T5">
        ///   The <see cref="T:System.Func{T1,T2,T3,T4,T5}" /> return type, and <see cref="T:System.Func{T5,TResult}" /> argument type.
        /// </typeparam>
        /// <typeparam name="TResult">
        ///   The <see cref="T:System.Func{T5,TResult}" /> return type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Func{T5,TResult}" /> to compose.
        /// </param>
        /// <param name="composer">
        ///   The <see cref="T:System.Func{T1,T2,T3,T4,T5}" /> to compose with <paramref name="self" />.
        /// </param>
        /// <returns>
        ///   Returns a <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> which, when invoked, will
        ///   invoke <paramref name="composer"/> and pass the return value of
        ///   <paramref name="composer" /> to <paramref name="self" />.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///    Composition is useful for chaining delegates together, so that the
        ///    return value of <paramref name="composer" /> is automatically used as
        ///    the input parameter for <paramref name="self" />.
        ///   </para>
        ///   <code lang="C#">
        ///   Func&lt;int,string> tostring = Lambda.F ((int n) => n.ToString ());
        ///   Func&lt;int, int>    doubler = Lambda.F ((int n) => n * 2);
        ///   Func&lt;int, string>
        ///        double_then_tostring = tostring.Compose (doubler);
        ///   Console.WriteLine (double_then_tostring (5));
        ///       // Prints "10";</code>
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <para>
        ///     <paramref name="self" /> is <see langword="null" />.
        ///   </para>
        ///   <para>-or-</para>
        ///   <para>
        ///     <paramref name="composer" /> is <see langword="null" />.
        ///   </para>
        /// </exception>
        public static System.Func<T1, T2, T3, T4, TResult> Compose<T1, T2, T3, T4, T5, TResult>(this System.Func<T5, TResult> self, System.Func<T1, T2, T3, T4, T5> composer)
        
        
        
        
        
        
        {
            Check.Self(self);
            Check.Composer(composer);
            return (value1, value2, value3, value4) => self (composer (value1, value2, value3, value4));
        }
    }
}
