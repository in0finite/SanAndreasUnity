// 
// Action.g.cs: Extension methods for Action<...> types.
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
    ///   Provides extension methods on <see cref="T:System.Action{T}"/>
    ///   and related delegates.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///    <see cref="T:Cadenza.ActionCoda" /> provides methods methods for:
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
    ///    for the <see cref="T:System.Action{T}"/> and related types.
    ///   </para>
    /// </remarks>
    public partial class ActionCoda
    {
        
        /// <typeparam name="T">
        ///   The <see cref="T:System.Func{T}" /> return type, and <see cref="T:System.Action{T}" /> argument type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Action" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Action{T}" /> to compose.
        /// </param>
        /// <param name="composer">
        ///   The <see cref="T:System.Func{T}" /> to compose with <paramref name="self" />.
        /// </param>
        /// <returns>
        ///   Returns a <see cref="T:System.Action" /> which, when invoked, will
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
        public static System.Action Compose<T>(this System.Action<T> self, System.Func<T> composer)
        
        {
            Check.Self(self);
            Check.Composer(composer);
            return () => self (composer ());
        }
        
        /// <summary>
        ///   Get timing information for delegate invocations.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Action" /> to generate timings for.
        /// </param>
        /// <param name="runs">
        ///   An <see cref="T:System.Int32" /> containing the number of <see cref="T:System.TimeSpan" /> values to return.
        /// </param>
        /// <returns>
        ///   An <see cref="T:System.Collections.Generic.IEnumerable{System.TimeSpan}" />
        ///   which will return the timing information for <paramref name="self" />.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///    This is equivalent to calling
        ///    <see cref="M:Cadenza.ActionCoda.Timings(System.Action,System.Int32,System.Int32)" />
        ///    with a <paramref name="loopsPerRun" /> value of <c>1</c>,
        ///    e.g. as if by calling <c>self.Timing (runs, 1)</c>.
        ///   </para>
        /// </remarks>
        /// <seealso cref="M:Cadenza.ActionCoda.Timings(System.Action,System.Int32,System.Int32)" />
        /// <exception cref="T:System.ArgumentException">
        ///   <para>
        ///    <paramref name="runs" /> is negative.
        ///   </para>
        /// </exception>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Collections.Generic.IEnumerable<System.TimeSpan> Timings(this System.Action self, int runs)
        {
            return Timings(self, runs, 1);
        }
        
        /// <summary>
        ///   Get timing information for delegate invocations.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Action" /> to generate timings for.
        /// </param>
        /// <param name="runs">
        ///   An <see cref="T:System.Int32" /> containing the number of <see cref="T:System.TimeSpan" /> values to return.
        /// </param>
        /// <param name="loopsPerRun">
        ///   An <see cref="T:System.Int32" /> containing the number of times to invoke <paramref name="self" /> for each <see cref="T:System.TimeSpan" /> value returned.
        /// </param>
        /// <returns>
        ///   An <see cref="T:System.Collections.Generic.IEnumerable{System.TimeSpan}" />
        ///   which will return the timing information for <paramref name="self" />.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///    Generates <paramref name="runs" /> <see cref="T:System.TimeSpan" />
        ///    instances, in which each <c>TimeSpan</c> instance is the amount of time
        ///    required to execute <paramref name="self" /> for
        ///    <paramref name="loopsPerRun" /> times.
        ///   </para>
        /// </remarks>
        /// <exception cref="T:System.ArgumentException">
        ///   <para>
        ///    <paramref name="runs" /> is negative.
        ///   </para>
        ///   <para>-or-</para>
        ///   <para>
        ///    <paramref name="loopsPerRun" /> is negative.
        ///   </para>
        /// </exception>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Collections.Generic.IEnumerable<System.TimeSpan> Timings(this System.Action self, int runs, int loopsPerRun)
        {
            Check.Self(self);
            if ((runs < 0))
            {
                throw new System.ArgumentException("Negative values are not supported.", "runs");
            }
            if ((loopsPerRun < 0))
            {
                throw new System.ArgumentException("Negative values are not supported.", "loopsPerRun");
            }
            return CreateTimingsIterator(self, runs, loopsPerRun);
        }
        
        private static System.Collections.Generic.IEnumerable<System.TimeSpan> CreateTimingsIterator(System.Action self, int runs, int loopsPerRun)
        {
            // Ensure that required methods are already JITed
            System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
            self();
            watch.Stop();
            watch.Reset();
            for (int i = 0; (i < runs); i = (i + 1))
            {
                watch.Start();
                for (int j = 0; (j < loopsPerRun); j = (j + 1))
                {
                    self();
                }
                watch.Stop();
                yield return watch.Elapsed;
                watch.Reset();
            }
        }
        
        /// <typeparam name="T">
        ///   A <see cref="T:System.Action{T}" /> parameter type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Action" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Action{T}" /> to curry.
        /// </param>
        /// <param name="value">
        ///   A value of type <typeparamref name="T"/> to fix.
        /// </param>
        /// <returns>
        ///   Returns a <see cref="T:System.Action" /> which, when invoked, will
        ///   invoke <paramref name="self"/> along with the provided fixed parameters.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Action Curry<T>(this System.Action<T> self, T value)
        
        {
            Check.Self(self);
            return () => self (value);
        }
        
        /// <typeparam name="T">
        ///   A <see cref="T:System.Action{T}" /> parameter type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Action" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Action{T}" /> to curry.
        /// </param>
        /// <param name="values">
        ///   A value of type <see cref="T:Tuple{T}" /> which contains the values to fix.
        /// </param>
        /// <returns>
        ///   Returns a <see cref="T:System.Action" /> which, when invoked, will
        ///   invoke <paramref name="self"/> along with the provided fixed parameters.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Action Curry<T>(this System.Action<T> self, Tuple<T> values)
        
        {
            Check.Self(self);
            return () => self (values.Item1);
        }
        
        // Currying method idea courtesy of:
        // http://blogs.msdn.com/wesdyer/archive/2007/01/29/currying-and-partial-function-application.aspx
        /// <typeparam name="T">
        ///   The first value type.
        /// </typeparam>
        /// <param name="self">
        ///   The <see cref="T:System.Action{T}" /> to curry.
        /// </param>
        /// <summary>
        ///   Creates a <see cref="T:System.Action{T}" /> for currying.
        /// </summary>
        /// <returns>
        ///   A <see cref="T:System.Action{T}" /> which, when invoked, will 
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
        public static System.Action<T> Curry<T>(this System.Action<T> self)
        
        {
            Check.Self(self);
            return value => self (value);
        }
        
        /// <typeparam name="T2">
        ///   The <see cref="T:System.Func{T1,T2}" /> return type, and <see cref="T:System.Action{T2}" /> argument type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Action{T1}" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Action{T2}" /> to compose.
        /// </param>
        /// <param name="composer">
        ///   The <see cref="T:System.Func{T1,T2}" /> to compose with <paramref name="self" />.
        /// </param>
        /// <returns>
        ///   Returns a <see cref="T:System.Action{T1}" /> which, when invoked, will
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
        public static System.Action<T1> Compose<T1, T2>(this System.Action<T2> self, System.Func<T1, T2> composer)
        
        
        {
            Check.Self(self);
            Check.Composer(composer);
            return (value) => self (composer (value));
        }
        
        /// <typeparam name="T">
        ///   A <see cref="T:System.Action{T}" /> parameter type.
        /// </typeparam>
        /// <summary>
        ///   Get timing information for delegate invocations.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Action{T}" /> to generate timings for.
        /// </param>
        /// <param name="value">
        ///   The *unknown* <paramref name="self"/> parameter value.
        /// </param>
        /// <param name="runs">
        ///   An <see cref="T:System.Int32" /> containing the number of <see cref="T:System.TimeSpan" /> values to return.
        /// </param>
        /// <returns>
        ///   An <see cref="T:System.Collections.Generic.IEnumerable{System.TimeSpan}" />
        ///   which will return the timing information for <paramref name="self" />.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///    This is equivalent to calling
        ///    <see cref="M:Cadenza.ActionCoda.Timings``1(System.Action{``0},``0,System.Int32,System.Int32)" />
        ///    with a <paramref name="loopsPerRun" /> value of <c>1</c>,
        ///    e.g. as if by calling <c>self.Timing (value, runs, 1)</c>.
        ///   </para>
        /// </remarks>
        /// <seealso cref="M:Cadenza.ActionCoda.Timings``1(System.Action{``0},``0,System.Int32,System.Int32)" />
        /// <exception cref="T:System.ArgumentException">
        ///   <para>
        ///    <paramref name="runs" /> is negative.
        ///   </para>
        /// </exception>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Collections.Generic.IEnumerable<System.TimeSpan> Timings<T>(this System.Action<T> self, T value, int runs)
        
        {
            return Timings(self, value, runs, 1);
        }
        
        /// <typeparam name="T">
        ///   A <see cref="T:System.Action{T}" /> parameter type.
        /// </typeparam>
        /// <summary>
        ///   Get timing information for delegate invocations.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Action{T}" /> to generate timings for.
        /// </param>
        /// <param name="value">
        ///   The *unknown* <paramref name="self"/> parameter value.
        /// </param>
        /// <param name="runs">
        ///   An <see cref="T:System.Int32" /> containing the number of <see cref="T:System.TimeSpan" /> values to return.
        /// </param>
        /// <param name="loopsPerRun">
        ///   An <see cref="T:System.Int32" /> containing the number of times to invoke <paramref name="self" /> for each <see cref="T:System.TimeSpan" /> value returned.
        /// </param>
        /// <returns>
        ///   An <see cref="T:System.Collections.Generic.IEnumerable{System.TimeSpan}" />
        ///   which will return the timing information for <paramref name="self" />.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///    Generates <paramref name="runs" /> <see cref="T:System.TimeSpan" />
        ///    instances, in which each <c>TimeSpan</c> instance is the amount of time
        ///    required to execute <paramref name="self" /> for
        ///    <paramref name="loopsPerRun" /> times.
        ///   </para>
        /// </remarks>
        /// <exception cref="T:System.ArgumentException">
        ///   <para>
        ///    <paramref name="runs" /> is negative.
        ///   </para>
        ///   <para>-or-</para>
        ///   <para>
        ///    <paramref name="loopsPerRun" /> is negative.
        ///   </para>
        /// </exception>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Collections.Generic.IEnumerable<System.TimeSpan> Timings<T>(this System.Action<T> self, T value, int runs, int loopsPerRun)
        
        {
            Check.Self(self);
            if ((runs < 0))
            {
                throw new System.ArgumentException("Negative values are not supported.", "runs");
            }
            if ((loopsPerRun < 0))
            {
                throw new System.ArgumentException("Negative values are not supported.", "loopsPerRun");
            }
            return CreateTimingsIterator(self, value, runs, loopsPerRun);
        }
        
        private static System.Collections.Generic.IEnumerable<System.TimeSpan> CreateTimingsIterator<T>(System.Action<T> self, T value, int runs, int loopsPerRun)
        
        {
            // Ensure that required methods are already JITed
            System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
            self(value);
            watch.Stop();
            watch.Reset();
            for (int i = 0; (i < runs); i = (i + 1))
            {
                watch.Start();
                for (int j = 0; (j < loopsPerRun); j = (j + 1))
                {
                    self(value);
                }
                watch.Stop();
                yield return watch.Elapsed;
                watch.Reset();
            }
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Action{T1,T2}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Action{T1,T2}" /> parameter type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Action{T2}" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Action{T1,T2}" /> to curry.
        /// </param>
        /// <param name="value1">
        ///   A value of type <typeparamref name="T1"/> to fix.
        /// </param>
        /// <returns>
        ///   Returns a <see cref="T:System.Action{T2}" /> which, when invoked, will
        ///   invoke <paramref name="self"/> along with the provided fixed parameters.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Action<T2> Curry<T1, T2>(this System.Action<T1, T2> self, T1 value1)
        
        
        {
            Check.Self(self);
            return (value2) => self (value1, value2);
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Action{T1,T2}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Action{T1,T2}" /> parameter type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Action" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Action{T1,T2}" /> to curry.
        /// </param>
        /// <param name="value1">
        ///   A value of type <typeparamref name="T1"/> to fix.
        /// </param>
        /// <param name="value2">
        ///   A value of type <typeparamref name="T2"/> to fix.
        /// </param>
        /// <returns>
        ///   Returns a <see cref="T:System.Action" /> which, when invoked, will
        ///   invoke <paramref name="self"/> along with the provided fixed parameters.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Action Curry<T1, T2>(this System.Action<T1, T2> self, T1 value1, T2 value2)
        
        
        {
            Check.Self(self);
            return () => self (value1, value2);
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Action{T1,T2}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Action{T1,T2}" /> parameter type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Action{T2}" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Action{T1,T2}" /> to curry.
        /// </param>
        /// <param name="values">
        ///   A value of type <see cref="T:Tuple{T1}" /> which contains the values to fix.
        /// </param>
        /// <returns>
        ///   Returns a <see cref="T:System.Action{T2}" /> which, when invoked, will
        ///   invoke <paramref name="self"/> along with the provided fixed parameters.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Action<T2> Curry<T1, T2>(this System.Action<T1, T2> self, Tuple<T1> values)
        
        
        {
            Check.Self(self);
            return (value2) => self (values.Item1, value2);
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Action{T1,T2}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Action{T1,T2}" /> parameter type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Action" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Action{T1,T2}" /> to curry.
        /// </param>
        /// <param name="values">
        ///   A value of type <see cref="T:Tuple{T1,T2}" /> which contains the values to fix.
        /// </param>
        /// <returns>
        ///   Returns a <see cref="T:System.Action" /> which, when invoked, will
        ///   invoke <paramref name="self"/> along with the provided fixed parameters.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Action Curry<T1, T2>(this System.Action<T1, T2> self, Tuple<T1, T2> values)
        
        
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
        /// <param name="self">
        ///   The <see cref="T:System.Action{T1,T2}" /> to curry.
        /// </param>
        /// <summary>
        ///   Creates a <see cref="T:System.Func{T1,System.Action{T2}}" /> for currying.
        /// </summary>
        /// <returns>
        ///   A <see cref="T:System.Func{T1,System.Action{T2}}" /> which, when invoked, will 
        ///   return a <see cref="T:System.Action{T2}" /> which, when invoked, will 
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
        public static System.Func<T1, System.Action<T2>> Curry<T1, T2>(this System.Action<T1, T2> self)
        
        
        {
            Check.Self(self);
            return value1 => value2 => self (value1, value2);
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Func{T1,T2,T3}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T3">
        ///   The <see cref="T:System.Func{T1,T2,T3}" /> return type, and <see cref="T:System.Action{T3}" /> argument type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Action{T1,T2}" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Action{T3}" /> to compose.
        /// </param>
        /// <param name="composer">
        ///   The <see cref="T:System.Func{T1,T2,T3}" /> to compose with <paramref name="self" />.
        /// </param>
        /// <returns>
        ///   Returns a <see cref="T:System.Action{T1,T2}" /> which, when invoked, will
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
        public static System.Action<T1, T2> Compose<T1, T2, T3>(this System.Action<T3> self, System.Func<T1, T2, T3> composer)
        
        
        
        {
            Check.Self(self);
            Check.Composer(composer);
            return (value1, value2) => self (composer (value1, value2));
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Action{T1,T2}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Action{T1,T2}" /> parameter type.
        /// </typeparam>
        /// <summary>
        ///   Get timing information for delegate invocations.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Action{T1,T2}" /> to generate timings for.
        /// </param>
        /// <param name="value1">
        ///   The *unknown* <paramref name="self"/> parameter value.
        /// </param>
        /// <param name="value2">
        ///   The *unknown* <paramref name="self"/> parameter value.
        /// </param>
        /// <param name="runs">
        ///   An <see cref="T:System.Int32" /> containing the number of <see cref="T:System.TimeSpan" /> values to return.
        /// </param>
        /// <returns>
        ///   An <see cref="T:System.Collections.Generic.IEnumerable{System.TimeSpan}" />
        ///   which will return the timing information for <paramref name="self" />.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///    This is equivalent to calling
        ///    <see cref="M:Cadenza.ActionCoda.Timings``2(System.Action{``0,``1},``0,``1,System.Int32,System.Int32)" />
        ///    with a <paramref name="loopsPerRun" /> value of <c>1</c>,
        ///    e.g. as if by calling <c>self.Timing (value1, value2, runs, 1)</c>.
        ///   </para>
        /// </remarks>
        /// <seealso cref="M:Cadenza.ActionCoda.Timings``2(System.Action{``0,``1},``0,``1,System.Int32,System.Int32)" />
        /// <exception cref="T:System.ArgumentException">
        ///   <para>
        ///    <paramref name="runs" /> is negative.
        ///   </para>
        /// </exception>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Collections.Generic.IEnumerable<System.TimeSpan> Timings<T1, T2>(this System.Action<T1, T2> self, T1 value1, T2 value2, int runs)
        
        
        {
            return Timings(self, value1, value2, runs, 1);
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Action{T1,T2}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Action{T1,T2}" /> parameter type.
        /// </typeparam>
        /// <summary>
        ///   Get timing information for delegate invocations.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Action{T1,T2}" /> to generate timings for.
        /// </param>
        /// <param name="value1">
        ///   The *unknown* <paramref name="self"/> parameter value.
        /// </param>
        /// <param name="value2">
        ///   The *unknown* <paramref name="self"/> parameter value.
        /// </param>
        /// <param name="runs">
        ///   An <see cref="T:System.Int32" /> containing the number of <see cref="T:System.TimeSpan" /> values to return.
        /// </param>
        /// <param name="loopsPerRun">
        ///   An <see cref="T:System.Int32" /> containing the number of times to invoke <paramref name="self" /> for each <see cref="T:System.TimeSpan" /> value returned.
        /// </param>
        /// <returns>
        ///   An <see cref="T:System.Collections.Generic.IEnumerable{System.TimeSpan}" />
        ///   which will return the timing information for <paramref name="self" />.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///    Generates <paramref name="runs" /> <see cref="T:System.TimeSpan" />
        ///    instances, in which each <c>TimeSpan</c> instance is the amount of time
        ///    required to execute <paramref name="self" /> for
        ///    <paramref name="loopsPerRun" /> times.
        ///   </para>
        /// </remarks>
        /// <exception cref="T:System.ArgumentException">
        ///   <para>
        ///    <paramref name="runs" /> is negative.
        ///   </para>
        ///   <para>-or-</para>
        ///   <para>
        ///    <paramref name="loopsPerRun" /> is negative.
        ///   </para>
        /// </exception>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Collections.Generic.IEnumerable<System.TimeSpan> Timings<T1, T2>(this System.Action<T1, T2> self, T1 value1, T2 value2, int runs, int loopsPerRun)
        
        
        {
            Check.Self(self);
            if ((runs < 0))
            {
                throw new System.ArgumentException("Negative values are not supported.", "runs");
            }
            if ((loopsPerRun < 0))
            {
                throw new System.ArgumentException("Negative values are not supported.", "loopsPerRun");
            }
            return CreateTimingsIterator(self, value1, value2, runs, loopsPerRun);
        }
        
        private static System.Collections.Generic.IEnumerable<System.TimeSpan> CreateTimingsIterator<T1, T2>(System.Action<T1, T2> self, T1 value1, T2 value2, int runs, int loopsPerRun)
        
        
        {
            // Ensure that required methods are already JITed
            System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
            self(value1, value2);
            watch.Stop();
            watch.Reset();
            for (int i = 0; (i < runs); i = (i + 1))
            {
                watch.Start();
                for (int j = 0; (j < loopsPerRun); j = (j + 1))
                {
                    self(value1, value2);
                }
                watch.Stop();
                yield return watch.Elapsed;
                watch.Reset();
            }
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Action{T1,T2,T3}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Action{T1,T2,T3}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T3">
        ///   A <see cref="T:System.Action{T1,T2,T3}" /> parameter type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Action{T2,T3}" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Action{T1,T2,T3}" /> to curry.
        /// </param>
        /// <param name="value1">
        ///   A value of type <typeparamref name="T1"/> to fix.
        /// </param>
        /// <returns>
        ///   Returns a <see cref="T:System.Action{T2,T3}" /> which, when invoked, will
        ///   invoke <paramref name="self"/> along with the provided fixed parameters.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Action<T2, T3> Curry<T1, T2, T3>(this System.Action<T1, T2, T3> self, T1 value1)
        
        
        
        {
            Check.Self(self);
            return (value2, value3) => self (value1, value2, value3);
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Action{T1,T2,T3}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Action{T1,T2,T3}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T3">
        ///   A <see cref="T:System.Action{T1,T2,T3}" /> parameter type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Action{T3}" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Action{T1,T2,T3}" /> to curry.
        /// </param>
        /// <param name="value1">
        ///   A value of type <typeparamref name="T1"/> to fix.
        /// </param>
        /// <param name="value2">
        ///   A value of type <typeparamref name="T2"/> to fix.
        /// </param>
        /// <returns>
        ///   Returns a <see cref="T:System.Action{T3}" /> which, when invoked, will
        ///   invoke <paramref name="self"/> along with the provided fixed parameters.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Action<T3> Curry<T1, T2, T3>(this System.Action<T1, T2, T3> self, T1 value1, T2 value2)
        
        
        
        {
            Check.Self(self);
            return (value3) => self (value1, value2, value3);
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Action{T1,T2,T3}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Action{T1,T2,T3}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T3">
        ///   A <see cref="T:System.Action{T1,T2,T3}" /> parameter type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Action" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Action{T1,T2,T3}" /> to curry.
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
        ///   Returns a <see cref="T:System.Action" /> which, when invoked, will
        ///   invoke <paramref name="self"/> along with the provided fixed parameters.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Action Curry<T1, T2, T3>(this System.Action<T1, T2, T3> self, T1 value1, T2 value2, T3 value3)
        
        
        
        {
            Check.Self(self);
            return () => self (value1, value2, value3);
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Action{T1,T2,T3}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Action{T1,T2,T3}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T3">
        ///   A <see cref="T:System.Action{T1,T2,T3}" /> parameter type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Action{T2,T3}" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Action{T1,T2,T3}" /> to curry.
        /// </param>
        /// <param name="values">
        ///   A value of type <see cref="T:Tuple{T1}" /> which contains the values to fix.
        /// </param>
        /// <returns>
        ///   Returns a <see cref="T:System.Action{T2,T3}" /> which, when invoked, will
        ///   invoke <paramref name="self"/> along with the provided fixed parameters.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Action<T2, T3> Curry<T1, T2, T3>(this System.Action<T1, T2, T3> self, Tuple<T1> values)
        
        
        
        {
            Check.Self(self);
            return (value2, value3) => self (values.Item1, value2, value3);
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Action{T1,T2,T3}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Action{T1,T2,T3}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T3">
        ///   A <see cref="T:System.Action{T1,T2,T3}" /> parameter type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Action{T3}" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Action{T1,T2,T3}" /> to curry.
        /// </param>
        /// <param name="values">
        ///   A value of type <see cref="T:Tuple{T1,T2}" /> which contains the values to fix.
        /// </param>
        /// <returns>
        ///   Returns a <see cref="T:System.Action{T3}" /> which, when invoked, will
        ///   invoke <paramref name="self"/> along with the provided fixed parameters.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Action<T3> Curry<T1, T2, T3>(this System.Action<T1, T2, T3> self, Tuple<T1, T2> values)
        
        
        
        {
            Check.Self(self);
            return (value3) => self (values.Item1, values.Item2, value3);
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Action{T1,T2,T3}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Action{T1,T2,T3}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T3">
        ///   A <see cref="T:System.Action{T1,T2,T3}" /> parameter type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Action" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Action{T1,T2,T3}" /> to curry.
        /// </param>
        /// <param name="values">
        ///   A value of type <see cref="T:Tuple{T1,T2,T3}" /> which contains the values to fix.
        /// </param>
        /// <returns>
        ///   Returns a <see cref="T:System.Action" /> which, when invoked, will
        ///   invoke <paramref name="self"/> along with the provided fixed parameters.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Action Curry<T1, T2, T3>(this System.Action<T1, T2, T3> self, Tuple<T1, T2, T3> values)
        
        
        
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
        /// <param name="self">
        ///   The <see cref="T:System.Action{T1,T2,T3}" /> to curry.
        /// </param>
        /// <summary>
        ///   Creates a <see cref="T:System.Func{T1,System.Func{T2,System.Action{T3}}}" /> for currying.
        /// </summary>
        /// <returns>
        ///   A <see cref="T:System.Func{T1,System.Func{T2,System.Action{T3}}}" /> which, when invoked, will 
        ///   return a <see cref="T:System.Func{T2,System.Action{T3}}" /> which, when invoked, will return a <see cref="T:System.Action{T3}" /> which, when invoked, will 
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
        public static System.Func<T1, System.Func<T2, System.Action<T3>>> Curry<T1, T2, T3>(this System.Action<T1, T2, T3> self)
        
        
        
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
        ///   The <see cref="T:System.Func{T1,T2,T3,T4}" /> return type, and <see cref="T:System.Action{T4}" /> argument type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Action{T1,T2,T3}" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Action{T4}" /> to compose.
        /// </param>
        /// <param name="composer">
        ///   The <see cref="T:System.Func{T1,T2,T3,T4}" /> to compose with <paramref name="self" />.
        /// </param>
        /// <returns>
        ///   Returns a <see cref="T:System.Action{T1,T2,T3}" /> which, when invoked, will
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
        public static System.Action<T1, T2, T3> Compose<T1, T2, T3, T4>(this System.Action<T4> self, System.Func<T1, T2, T3, T4> composer)
        
        
        
        
        {
            Check.Self(self);
            Check.Composer(composer);
            return (value1, value2, value3) => self (composer (value1, value2, value3));
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Action{T1,T2,T3}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Action{T1,T2,T3}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T3">
        ///   A <see cref="T:System.Action{T1,T2,T3}" /> parameter type.
        /// </typeparam>
        /// <summary>
        ///   Get timing information for delegate invocations.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Action{T1,T2,T3}" /> to generate timings for.
        /// </param>
        /// <param name="value1">
        ///   The *unknown* <paramref name="self"/> parameter value.
        /// </param>
        /// <param name="value2">
        ///   The *unknown* <paramref name="self"/> parameter value.
        /// </param>
        /// <param name="value3">
        ///   The *unknown* <paramref name="self"/> parameter value.
        /// </param>
        /// <param name="runs">
        ///   An <see cref="T:System.Int32" /> containing the number of <see cref="T:System.TimeSpan" /> values to return.
        /// </param>
        /// <returns>
        ///   An <see cref="T:System.Collections.Generic.IEnumerable{System.TimeSpan}" />
        ///   which will return the timing information for <paramref name="self" />.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///    This is equivalent to calling
        ///    <see cref="M:Cadenza.ActionCoda.Timings``3(System.Action{``0,``1,``2},``0,``1,``2,System.Int32,System.Int32)" />
        ///    with a <paramref name="loopsPerRun" /> value of <c>1</c>,
        ///    e.g. as if by calling <c>self.Timing (value1, value2, value3, runs, 1)</c>.
        ///   </para>
        /// </remarks>
        /// <seealso cref="M:Cadenza.ActionCoda.Timings``3(System.Action{``0,``1,``2},``0,``1,``2,System.Int32,System.Int32)" />
        /// <exception cref="T:System.ArgumentException">
        ///   <para>
        ///    <paramref name="runs" /> is negative.
        ///   </para>
        /// </exception>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Collections.Generic.IEnumerable<System.TimeSpan> Timings<T1, T2, T3>(this System.Action<T1, T2, T3> self, T1 value1, T2 value2, T3 value3, int runs)
        
        
        
        {
            return Timings(self, value1, value2, value3, runs, 1);
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Action{T1,T2,T3}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Action{T1,T2,T3}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T3">
        ///   A <see cref="T:System.Action{T1,T2,T3}" /> parameter type.
        /// </typeparam>
        /// <summary>
        ///   Get timing information for delegate invocations.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Action{T1,T2,T3}" /> to generate timings for.
        /// </param>
        /// <param name="value1">
        ///   The *unknown* <paramref name="self"/> parameter value.
        /// </param>
        /// <param name="value2">
        ///   The *unknown* <paramref name="self"/> parameter value.
        /// </param>
        /// <param name="value3">
        ///   The *unknown* <paramref name="self"/> parameter value.
        /// </param>
        /// <param name="runs">
        ///   An <see cref="T:System.Int32" /> containing the number of <see cref="T:System.TimeSpan" /> values to return.
        /// </param>
        /// <param name="loopsPerRun">
        ///   An <see cref="T:System.Int32" /> containing the number of times to invoke <paramref name="self" /> for each <see cref="T:System.TimeSpan" /> value returned.
        /// </param>
        /// <returns>
        ///   An <see cref="T:System.Collections.Generic.IEnumerable{System.TimeSpan}" />
        ///   which will return the timing information for <paramref name="self" />.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///    Generates <paramref name="runs" /> <see cref="T:System.TimeSpan" />
        ///    instances, in which each <c>TimeSpan</c> instance is the amount of time
        ///    required to execute <paramref name="self" /> for
        ///    <paramref name="loopsPerRun" /> times.
        ///   </para>
        /// </remarks>
        /// <exception cref="T:System.ArgumentException">
        ///   <para>
        ///    <paramref name="runs" /> is negative.
        ///   </para>
        ///   <para>-or-</para>
        ///   <para>
        ///    <paramref name="loopsPerRun" /> is negative.
        ///   </para>
        /// </exception>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Collections.Generic.IEnumerable<System.TimeSpan> Timings<T1, T2, T3>(this System.Action<T1, T2, T3> self, T1 value1, T2 value2, T3 value3, int runs, int loopsPerRun)
        
        
        
        {
            Check.Self(self);
            if ((runs < 0))
            {
                throw new System.ArgumentException("Negative values are not supported.", "runs");
            }
            if ((loopsPerRun < 0))
            {
                throw new System.ArgumentException("Negative values are not supported.", "loopsPerRun");
            }
            return CreateTimingsIterator(self, value1, value2, value3, runs, loopsPerRun);
        }
        
        private static System.Collections.Generic.IEnumerable<System.TimeSpan> CreateTimingsIterator<T1, T2, T3>(System.Action<T1, T2, T3> self, T1 value1, T2 value2, T3 value3, int runs, int loopsPerRun)
        
        
        
        {
            // Ensure that required methods are already JITed
            System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
            self(value1, value2, value3);
            watch.Stop();
            watch.Reset();
            for (int i = 0; (i < runs); i = (i + 1))
            {
                watch.Start();
                for (int j = 0; (j < loopsPerRun); j = (j + 1))
                {
                    self(value1, value2, value3);
                }
                watch.Stop();
                yield return watch.Elapsed;
                watch.Reset();
            }
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Action{T1,T2,T3,T4}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Action{T1,T2,T3,T4}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T3">
        ///   A <see cref="T:System.Action{T1,T2,T3,T4}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T4">
        ///   A <see cref="T:System.Action{T1,T2,T3,T4}" /> parameter type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Action{T2,T3,T4}" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Action{T1,T2,T3,T4}" /> to curry.
        /// </param>
        /// <param name="value1">
        ///   A value of type <typeparamref name="T1"/> to fix.
        /// </param>
        /// <returns>
        ///   Returns a <see cref="T:System.Action{T2,T3,T4}" /> which, when invoked, will
        ///   invoke <paramref name="self"/> along with the provided fixed parameters.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Action<T2, T3, T4> Curry<T1, T2, T3, T4>(this System.Action<T1, T2, T3, T4> self, T1 value1)
        
        
        
        
        {
            Check.Self(self);
            return (value2, value3, value4) => self (value1, value2, value3, value4);
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Action{T1,T2,T3,T4}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Action{T1,T2,T3,T4}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T3">
        ///   A <see cref="T:System.Action{T1,T2,T3,T4}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T4">
        ///   A <see cref="T:System.Action{T1,T2,T3,T4}" /> parameter type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Action{T3,T4}" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Action{T1,T2,T3,T4}" /> to curry.
        /// </param>
        /// <param name="value1">
        ///   A value of type <typeparamref name="T1"/> to fix.
        /// </param>
        /// <param name="value2">
        ///   A value of type <typeparamref name="T2"/> to fix.
        /// </param>
        /// <returns>
        ///   Returns a <see cref="T:System.Action{T3,T4}" /> which, when invoked, will
        ///   invoke <paramref name="self"/> along with the provided fixed parameters.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Action<T3, T4> Curry<T1, T2, T3, T4>(this System.Action<T1, T2, T3, T4> self, T1 value1, T2 value2)
        
        
        
        
        {
            Check.Self(self);
            return (value3, value4) => self (value1, value2, value3, value4);
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Action{T1,T2,T3,T4}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Action{T1,T2,T3,T4}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T3">
        ///   A <see cref="T:System.Action{T1,T2,T3,T4}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T4">
        ///   A <see cref="T:System.Action{T1,T2,T3,T4}" /> parameter type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Action{T4}" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Action{T1,T2,T3,T4}" /> to curry.
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
        ///   Returns a <see cref="T:System.Action{T4}" /> which, when invoked, will
        ///   invoke <paramref name="self"/> along with the provided fixed parameters.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Action<T4> Curry<T1, T2, T3, T4>(this System.Action<T1, T2, T3, T4> self, T1 value1, T2 value2, T3 value3)
        
        
        
        
        {
            Check.Self(self);
            return (value4) => self (value1, value2, value3, value4);
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Action{T1,T2,T3,T4}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Action{T1,T2,T3,T4}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T3">
        ///   A <see cref="T:System.Action{T1,T2,T3,T4}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T4">
        ///   A <see cref="T:System.Action{T1,T2,T3,T4}" /> parameter type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Action" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Action{T1,T2,T3,T4}" /> to curry.
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
        ///   Returns a <see cref="T:System.Action" /> which, when invoked, will
        ///   invoke <paramref name="self"/> along with the provided fixed parameters.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Action Curry<T1, T2, T3, T4>(this System.Action<T1, T2, T3, T4> self, T1 value1, T2 value2, T3 value3, T4 value4)
        
        
        
        
        {
            Check.Self(self);
            return () => self (value1, value2, value3, value4);
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Action{T1,T2,T3,T4}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Action{T1,T2,T3,T4}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T3">
        ///   A <see cref="T:System.Action{T1,T2,T3,T4}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T4">
        ///   A <see cref="T:System.Action{T1,T2,T3,T4}" /> parameter type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Action{T2,T3,T4}" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Action{T1,T2,T3,T4}" /> to curry.
        /// </param>
        /// <param name="values">
        ///   A value of type <see cref="T:Tuple{T1}" /> which contains the values to fix.
        /// </param>
        /// <returns>
        ///   Returns a <see cref="T:System.Action{T2,T3,T4}" /> which, when invoked, will
        ///   invoke <paramref name="self"/> along with the provided fixed parameters.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Action<T2, T3, T4> Curry<T1, T2, T3, T4>(this System.Action<T1, T2, T3, T4> self, Tuple<T1> values)
        
        
        
        
        {
            Check.Self(self);
            return (value2, value3, value4) => self (values.Item1, value2, value3, value4);
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Action{T1,T2,T3,T4}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Action{T1,T2,T3,T4}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T3">
        ///   A <see cref="T:System.Action{T1,T2,T3,T4}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T4">
        ///   A <see cref="T:System.Action{T1,T2,T3,T4}" /> parameter type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Action{T3,T4}" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Action{T1,T2,T3,T4}" /> to curry.
        /// </param>
        /// <param name="values">
        ///   A value of type <see cref="T:Tuple{T1,T2}" /> which contains the values to fix.
        /// </param>
        /// <returns>
        ///   Returns a <see cref="T:System.Action{T3,T4}" /> which, when invoked, will
        ///   invoke <paramref name="self"/> along with the provided fixed parameters.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Action<T3, T4> Curry<T1, T2, T3, T4>(this System.Action<T1, T2, T3, T4> self, Tuple<T1, T2> values)
        
        
        
        
        {
            Check.Self(self);
            return (value3, value4) => self (values.Item1, values.Item2, value3, value4);
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Action{T1,T2,T3,T4}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Action{T1,T2,T3,T4}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T3">
        ///   A <see cref="T:System.Action{T1,T2,T3,T4}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T4">
        ///   A <see cref="T:System.Action{T1,T2,T3,T4}" /> parameter type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Action{T4}" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Action{T1,T2,T3,T4}" /> to curry.
        /// </param>
        /// <param name="values">
        ///   A value of type <see cref="T:Tuple{T1,T2,T3}" /> which contains the values to fix.
        /// </param>
        /// <returns>
        ///   Returns a <see cref="T:System.Action{T4}" /> which, when invoked, will
        ///   invoke <paramref name="self"/> along with the provided fixed parameters.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Action<T4> Curry<T1, T2, T3, T4>(this System.Action<T1, T2, T3, T4> self, Tuple<T1, T2, T3> values)
        
        
        
        
        {
            Check.Self(self);
            return (value4) => self (values.Item1, values.Item2, values.Item3, value4);
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Action{T1,T2,T3,T4}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Action{T1,T2,T3,T4}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T3">
        ///   A <see cref="T:System.Action{T1,T2,T3,T4}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T4">
        ///   A <see cref="T:System.Action{T1,T2,T3,T4}" /> parameter type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Action" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Action{T1,T2,T3,T4}" /> to curry.
        /// </param>
        /// <param name="values">
        ///   A value of type <see cref="T:Tuple{T1,T2,T3,T4}" /> which contains the values to fix.
        /// </param>
        /// <returns>
        ///   Returns a <see cref="T:System.Action" /> which, when invoked, will
        ///   invoke <paramref name="self"/> along with the provided fixed parameters.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Action Curry<T1, T2, T3, T4>(this System.Action<T1, T2, T3, T4> self, Tuple<T1, T2, T3, T4> values)
        
        
        
        
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
        /// <param name="self">
        ///   The <see cref="T:System.Action{T1,T2,T3,T4}" /> to curry.
        /// </param>
        /// <summary>
        ///   Creates a <see cref="T:System.Func{T1,System.Func{T2,System.Func{T3,System.Action{T4}}}}" /> for currying.
        /// </summary>
        /// <returns>
        ///   A <see cref="T:System.Func{T1,System.Func{T2,System.Func{T3,System.Action{T4}}}}" /> which, when invoked, will 
        ///   return a <see cref="T:System.Func{T2,System.Func{T3,System.Action{T4}}}" /> which, when invoked, will return a <see cref="T:System.Func{T3,System.Action{T4}}" /> which, when invoked, will return a <see cref="T:System.Action{T4}" /> which, when invoked, will 
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
        public static System.Func<T1, System.Func<T2, System.Func<T3, System.Action<T4>>>> Curry<T1, T2, T3, T4>(this System.Action<T1, T2, T3, T4> self)
        
        
        
        
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
        ///   The <see cref="T:System.Func{T1,T2,T3,T4,T5}" /> return type, and <see cref="T:System.Action{T5}" /> argument type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="T:System.Action{T1,T2,T3,T4}" /> delegate.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Action{T5}" /> to compose.
        /// </param>
        /// <param name="composer">
        ///   The <see cref="T:System.Func{T1,T2,T3,T4,T5}" /> to compose with <paramref name="self" />.
        /// </param>
        /// <returns>
        ///   Returns a <see cref="T:System.Action{T1,T2,T3,T4}" /> which, when invoked, will
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
        public static System.Action<T1, T2, T3, T4> Compose<T1, T2, T3, T4, T5>(this System.Action<T5> self, System.Func<T1, T2, T3, T4, T5> composer)
        
        
        
        
        
        {
            Check.Self(self);
            Check.Composer(composer);
            return (value1, value2, value3, value4) => self (composer (value1, value2, value3, value4));
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Action{T1,T2,T3,T4}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Action{T1,T2,T3,T4}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T3">
        ///   A <see cref="T:System.Action{T1,T2,T3,T4}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T4">
        ///   A <see cref="T:System.Action{T1,T2,T3,T4}" /> parameter type.
        /// </typeparam>
        /// <summary>
        ///   Get timing information for delegate invocations.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Action{T1,T2,T3,T4}" /> to generate timings for.
        /// </param>
        /// <param name="value1">
        ///   The *unknown* <paramref name="self"/> parameter value.
        /// </param>
        /// <param name="value2">
        ///   The *unknown* <paramref name="self"/> parameter value.
        /// </param>
        /// <param name="value3">
        ///   The *unknown* <paramref name="self"/> parameter value.
        /// </param>
        /// <param name="value4">
        ///   The *unknown* <paramref name="self"/> parameter value.
        /// </param>
        /// <param name="runs">
        ///   An <see cref="T:System.Int32" /> containing the number of <see cref="T:System.TimeSpan" /> values to return.
        /// </param>
        /// <returns>
        ///   An <see cref="T:System.Collections.Generic.IEnumerable{System.TimeSpan}" />
        ///   which will return the timing information for <paramref name="self" />.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///    This is equivalent to calling
        ///    <see cref="M:Cadenza.ActionCoda.Timings``4(System.Action{``0,``1,``2,``3},``0,``1,``2,``3,System.Int32,System.Int32)" />
        ///    with a <paramref name="loopsPerRun" /> value of <c>1</c>,
        ///    e.g. as if by calling <c>self.Timing (value1, value2, value3, value4, runs, 1)</c>.
        ///   </para>
        /// </remarks>
        /// <seealso cref="M:Cadenza.ActionCoda.Timings``4(System.Action{``0,``1,``2,``3},``0,``1,``2,``3,System.Int32,System.Int32)" />
        /// <exception cref="T:System.ArgumentException">
        ///   <para>
        ///    <paramref name="runs" /> is negative.
        ///   </para>
        /// </exception>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Collections.Generic.IEnumerable<System.TimeSpan> Timings<T1, T2, T3, T4>(this System.Action<T1, T2, T3, T4> self, T1 value1, T2 value2, T3 value3, T4 value4, int runs)
        
        
        
        
        {
            return Timings(self, value1, value2, value3, value4, runs, 1);
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Action{T1,T2,T3,T4}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Action{T1,T2,T3,T4}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T3">
        ///   A <see cref="T:System.Action{T1,T2,T3,T4}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T4">
        ///   A <see cref="T:System.Action{T1,T2,T3,T4}" /> parameter type.
        /// </typeparam>
        /// <summary>
        ///   Get timing information for delegate invocations.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="T:System.Action{T1,T2,T3,T4}" /> to generate timings for.
        /// </param>
        /// <param name="value1">
        ///   The *unknown* <paramref name="self"/> parameter value.
        /// </param>
        /// <param name="value2">
        ///   The *unknown* <paramref name="self"/> parameter value.
        /// </param>
        /// <param name="value3">
        ///   The *unknown* <paramref name="self"/> parameter value.
        /// </param>
        /// <param name="value4">
        ///   The *unknown* <paramref name="self"/> parameter value.
        /// </param>
        /// <param name="runs">
        ///   An <see cref="T:System.Int32" /> containing the number of <see cref="T:System.TimeSpan" /> values to return.
        /// </param>
        /// <param name="loopsPerRun">
        ///   An <see cref="T:System.Int32" /> containing the number of times to invoke <paramref name="self" /> for each <see cref="T:System.TimeSpan" /> value returned.
        /// </param>
        /// <returns>
        ///   An <see cref="T:System.Collections.Generic.IEnumerable{System.TimeSpan}" />
        ///   which will return the timing information for <paramref name="self" />.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///    Generates <paramref name="runs" /> <see cref="T:System.TimeSpan" />
        ///    instances, in which each <c>TimeSpan</c> instance is the amount of time
        ///    required to execute <paramref name="self" /> for
        ///    <paramref name="loopsPerRun" /> times.
        ///   </para>
        /// </remarks>
        /// <exception cref="T:System.ArgumentException">
        ///   <para>
        ///    <paramref name="runs" /> is negative.
        ///   </para>
        ///   <para>-or-</para>
        ///   <para>
        ///    <paramref name="loopsPerRun" /> is negative.
        ///   </para>
        /// </exception>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="self"/> is <see langword="null" />.
        /// </exception>
        public static System.Collections.Generic.IEnumerable<System.TimeSpan> Timings<T1, T2, T3, T4>(this System.Action<T1, T2, T3, T4> self, T1 value1, T2 value2, T3 value3, T4 value4, int runs, int loopsPerRun)
        
        
        
        
        {
            Check.Self(self);
            if ((runs < 0))
            {
                throw new System.ArgumentException("Negative values are not supported.", "runs");
            }
            if ((loopsPerRun < 0))
            {
                throw new System.ArgumentException("Negative values are not supported.", "loopsPerRun");
            }
            return CreateTimingsIterator(self, value1, value2, value3, value4, runs, loopsPerRun);
        }
        
        private static System.Collections.Generic.IEnumerable<System.TimeSpan> CreateTimingsIterator<T1, T2, T3, T4>(System.Action<T1, T2, T3, T4> self, T1 value1, T2 value2, T3 value3, T4 value4, int runs, int loopsPerRun)
        
        
        
        
        {
            // Ensure that required methods are already JITed
            System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
            self(value1, value2, value3, value4);
            watch.Stop();
            watch.Reset();
            for (int i = 0; (i < runs); i = (i + 1))
            {
                watch.Start();
                for (int j = 0; (j < loopsPerRun); j = (j + 1))
                {
                    self(value1, value2, value3, value4);
                }
                watch.Stop();
                yield return watch.Elapsed;
                watch.Reset();
            }
        }
    }
}
