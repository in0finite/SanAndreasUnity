// 
// Lambdas.cs: C# Lambda Expression Helpers.
// 
// GENERATED CODE: DO NOT EDIT.
// 
// To regenerate this code, execute: Lambdas.exe -n 4 -o Cadenza/Lambda.g.cs
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
    using System.Linq.Expressions;
    
    
    /// <summary>
    ///   Provides static utility methods to generate anonymous delegates 
    ///   or expression trees of pre-determined types.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///    C# lambda methods and anonymous delegates are a curious 
    ///    1.5-class citizen: They are implicitly convertable to any
    ///    delegate type, but have no type by themselves.  Thus,
    ///    the following code fails to compile:
    ///   </para>
    ///   <code lang="C#">
    ///   ((int x) => Console.WriteLine (x))(5);
    ///   </code>
    ///   <para>It would instead need:</para>
    ///   <code lang="C#">
    ///   // either:
    ///   Action&lt;int&gt; a = x => Console.WriteLine (x);
    ///   a (5);
    ///   //
    ///   // or
    ///   //
    ///   ((Action&lt;int&gt;) (x => Console.WriteLine (x)))(5);
    ///   </code>
    ///   <para>
    ///    So you'd either need to assign the lambda to an actual
    ///    delegate type, or insert a cast.
    ///   </para>
    ///   <para>
    ///    <see cref="M:Cadenza.Lambda.A" /> allows you to
    ///    provide a lambda body for the <see cref="T:System.Action"/> 
    ///    builtin delegate type, and <see cref="M:Cadenza.Lambda.F" />
    ///    allows you to provide a lambda body for the 
    ///    <see cref="T:System.Func{TResult}"/> delegate type, 
    ///    thus removing the need for a cast or an extra variable:
    ///   </para>
    ///   <code lang="C#">
    ///   Lambda.F ((int x) => Console.WriteLine (x)) (5);</code>
    ///   <para>
    ///    <see cref="T:Cadenza.Lambda"/> provides the following sets of
    ///    functionality:
    ///   </para>
    ///   <list type="bullet">
    ///    <item><term>Delegate creation methods, which return 
    ///     <see cref="T:System.Action"/>-like delegates:
    ///     <see cref="M:Cadenza.Lambda.A(System.Action)"/>,
    ///     <see cref="M:Cadenza.Lambda.A``1(System.Action{``0})"/>,
    ///     <see cref="M:Cadenza.Lambda.A``2(System.Action{``0,``1})"/>,
    ///     <see cref="M:Cadenza.Lambda.A``3(System.Action{``0,``1,``2})"/>, and
    ///     <see cref="M:Cadenza.Lambda.A``4(System.Action{``0,``1,``2,``3})"/>.
    ///    </term></item>
    ///    <item><term>Delegate creation methods which return 
    ///     return <see cref="T:System.Func{TResult}"/>-like delegates
    ///     <see cref="M:Cadenza.Lambda.F``1(System.Func{``0})"/>,
    ///     <see cref="M:Cadenza.Lambda.F``2(System.Func{``0,``1})"/>,
    ///     <see cref="M:Cadenza.Lambda.F``3(System.Func{``0,``1,``2})"/>,
    ///     <see cref="M:Cadenza.Lambda.F``4(System.Func{``0,``1,``2,``3})"/>, and
    ///     <see cref="M:Cadenza.Lambda.F``5(System.Func{``0,``1,``2,``3,``4})"/>.
    ///    </term></item>
    ///    <item><term><see cref="T:System.Linq.Expressions.Expression"/>-creating methods:
    ///     <see cref="M:Cadenza.Lambda.XA(System.Linq.Expressions.Expression{System.Action})"/>,
    ///     <see cref="M:Cadenza.Lambda.XA``1(System.Linq.Expressions.Expression{System.Action{``0}})"/>,
    ///     <see cref="M:Cadenza.Lambda.XA``2(System.Linq.Expressions.Expression{System.Action{``0,``1}})"/>,
    ///     <see cref="M:Cadenza.Lambda.XA``3(System.Linq.Expressions.Expression{System.Action{``0,``1,``2}})"/>,
    ///     <see cref="M:Cadenza.Lambda.XA``4(System.Linq.Expressions.Expression{System.Action{``0,``1,``2,``3}})"/>, and
    ///     <see cref="M:Cadenza.Lambda.XF``1(System.Linq.Expressions.Expression{System.Func{``0}})"/>,
    ///     <see cref="M:Cadenza.Lambda.XF``2(System.Linq.Expressions.Expression{System.Func{``0,``1}})"/>,
    ///     <see cref="M:Cadenza.Lambda.XF``3(System.Linq.Expressions.Expression{System.Func{``0,``1,``2}})"/>,
    ///     <see cref="M:Cadenza.Lambda.XF``4(System.Linq.Expressions.Expression{System.Func{``0,``1,``2,``3}})"/>,
    ///     <see cref="M:Cadenza.Lambda.XF``5(System.Linq.Expressions.Expression{System.Func{``0,``1,``2,``3,``4}})"/>.
    ///    </term></item>
    ///    <item><term>Y-Combinators, which permit writing recursive lambdas:
    ///     <see cref="M:Cadenza.Lambda.RecFunc``2(System.Func{System.Func{``0,``1},System.Func{``0,``1}})"/>,
    ///     <see cref="M:Cadenza.Lambda.RecFunc``3(System.Func{System.Func{``0,``1,``2},System.Func{``0,``1,``2}})"/>,
    ///     <see cref="M:Cadenza.Lambda.RecFunc``4(System.Func{System.Func{``0,``1,``2,``3},System.Func{``0,``1,``2,``3}})"/>, and
    ///     <see cref="M:Cadenza.Lambda.RecFunc``5(System.Func{System.Func{``0,``1,``2,``3,``4},System.Func{``0,``1,``2,``3,``4}})"/>.
    ///    </term></item>
    ///   </list>
    /// </remarks>
    public partial class Lambda
    {
        
        /// <param name="lambda">
        ///   The <see cref="T:System.Action" /> to return.
        /// </param>
        /// <summary>
        ///   Creates a <see cref="T:System.Action" /> delegate.
        /// </summary>
        /// <returns>
        ///   Returns <paramref name="lambda" />.
        /// </returns>
        public static System.Action A(System.Action lambda)
        {
            return lambda;
        }
        
        /// <typeparam name="TResult">
        ///   A <see cref="T:System.Func{TResult}" /> return type.
        /// </typeparam>
        /// <param name="lambda">
        ///   The <see cref="T:System.Func{TResult}" /> to return.
        /// </param>
        /// <summary>
        ///   Creates a <see cref="T:System.Func{TResult}" /> delegate.
        /// </summary>
        /// <returns>
        ///   Returns <paramref name="lambda" />.
        /// </returns>
        public static System.Func<TResult> F<TResult>(System.Func<TResult> lambda)
        
        {
            return lambda;
        }
        
        /// <param name="expr">
        ///   The <see cref="T:System.Linq.Expressions.Expression{System.Action}" /> to return.
        /// </param>
        /// <summary>
        ///   Creates a <see cref="T:System.Linq.Expressions.Expression{System.Action}" /> expression tree.
        /// </summary>
        /// <returns>
        ///   Returns <paramref name="expr" />.
        /// </returns>
        public static System.Linq.Expressions.Expression<System.Action> XA(System.Linq.Expressions.Expression<System.Action> expr)
        {
            return expr;
        }
        
        /// <typeparam name="TResult">
        ///   A <see cref="T:System.Linq.Expressions.Expression{System.Func{TResult}}" /> return type.
        /// </typeparam>
        /// <param name="expr">
        ///   The <see cref="T:System.Linq.Expressions.Expression{System.Func{TResult}}" /> to return.
        /// </param>
        /// <summary>
        ///   Creates a <see cref="T:System.Linq.Expressions.Expression{System.Func{TResult}}" /> expression tree.
        /// </summary>
        /// <returns>
        ///   Returns <paramref name="expr" />.
        /// </returns>
        public static System.Linq.Expressions.Expression<System.Func<TResult>> XF<TResult>(System.Linq.Expressions.Expression<System.Func<TResult>> expr)
        
        {
            return expr;
        }
        
        /// <typeparam name="T">
        ///   A <see cref="T:System.Action{T}" /> parameter type.
        /// </typeparam>
        /// <param name="lambda">
        ///   The <see cref="T:System.Action{T}" /> to return.
        /// </param>
        /// <summary>
        ///   Creates a <see cref="T:System.Action{T}" /> delegate.
        /// </summary>
        /// <returns>
        ///   Returns <paramref name="lambda" />.
        /// </returns>
        public static System.Action<T> A<T>(System.Action<T> lambda)
        
        {
            return lambda;
        }
        
        /// <typeparam name="T">
        ///   A <see cref="T:System.Func{T,TResult}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="TResult">
        ///   A <see cref="T:System.Func{T,TResult}" /> return type.
        /// </typeparam>
        /// <param name="lambda">
        ///   The <see cref="T:System.Func{T,TResult}" /> to return.
        /// </param>
        /// <summary>
        ///   Creates a <see cref="T:System.Func{T,TResult}" /> delegate.
        /// </summary>
        /// <returns>
        ///   Returns <paramref name="lambda" />.
        /// </returns>
        public static System.Func<T, TResult> F<T, TResult>(System.Func<T, TResult> lambda)
        
        
        {
            return lambda;
        }
        
        /// <typeparam name="T">
        ///   A <see cref="T:System.Linq.Expressions.Expression{System.Action{T}}" /> parameter type.
        /// </typeparam>
        /// <param name="expr">
        ///   The <see cref="T:System.Linq.Expressions.Expression{System.Action{T}}" /> to return.
        /// </param>
        /// <summary>
        ///   Creates a <see cref="T:System.Linq.Expressions.Expression{System.Action{T}}" /> expression tree.
        /// </summary>
        /// <returns>
        ///   Returns <paramref name="expr" />.
        /// </returns>
        public static System.Linq.Expressions.Expression<System.Action<T>> XA<T>(System.Linq.Expressions.Expression<System.Action<T>> expr)
        
        {
            return expr;
        }
        
        /// <typeparam name="T">
        ///   A <see cref="T:System.Linq.Expressions.Expression{System.Func{T,TResult}}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="TResult">
        ///   A <see cref="T:System.Linq.Expressions.Expression{System.Func{T,TResult}}" /> return type.
        /// </typeparam>
        /// <param name="expr">
        ///   The <see cref="T:System.Linq.Expressions.Expression{System.Func{T,TResult}}" /> to return.
        /// </param>
        /// <summary>
        ///   Creates a <see cref="T:System.Linq.Expressions.Expression{System.Func{T,TResult}}" /> expression tree.
        /// </summary>
        /// <returns>
        ///   Returns <paramref name="expr" />.
        /// </returns>
        public static System.Linq.Expressions.Expression<System.Func<T, TResult>> XF<T, TResult>(System.Linq.Expressions.Expression<System.Func<T, TResult>> expr)
        
        
        {
            return expr;
        }
        
        // From: http://blogs.msdn.com/madst/archive/2007/05/11/recursive-lambda-expressions.aspx
        /// <typeparam name="T">
        ///   A <see cref="T:System.Func{T,TResult}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="TResult">
        ///   A <see cref="T:System.Func{T,TResult}" /> return type.
        /// </typeparam>
        /// <param name="lambda">
        ///   The <see cref="T:System.Func{T,TResult}" /> to use.
        /// </param>
        /// <summary>
        ///   Creates a <see cref="T:System.Func{T,TResult}" /> delegate, which may be recursive.
        /// </summary>
        /// <returns>
        ///   Returns a <see cref="T:System.Func{T,TResult}" /> which (eventually) invokes <paramref name="lambda"/>.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="lambda"/> is <see langword="null" />.
        /// </exception>
        /// <remarks>
        ///   <para>
        ///     The following example makes use of a recursive lambda:
        ///   </para>
        ///   <code lang="C#">
        ///     Func&lt;int, int> factorial = Lambda.RecFunc&lt;int, int> (
        ///         fac => x => x == 0 ? 1 : x * fac (x-1));
        ///     Console.WriteLine (factorial (5));  // prints "120"
        ///   </code>
        /// </remarks>
        public static System.Func<T, TResult> RecFunc<T, TResult>(Func<System.Func<T, TResult>, System.Func<T, TResult>> lambda)
        
        
        {
            if ((lambda == null))
            {
                throw new System.ArgumentNullException("lambda");
            }
            return (value1) => lambda (RecFunc (lambda))(value1);
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Action{T1,T2}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Action{T1,T2}" /> parameter type.
        /// </typeparam>
        /// <param name="lambda">
        ///   The <see cref="T:System.Action{T1,T2}" /> to return.
        /// </param>
        /// <summary>
        ///   Creates a <see cref="T:System.Action{T1,T2}" /> delegate.
        /// </summary>
        /// <returns>
        ///   Returns <paramref name="lambda" />.
        /// </returns>
        public static System.Action<T1, T2> A<T1, T2>(System.Action<T1, T2> lambda)
        
        
        {
            return lambda;
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Func{T1,T2,TResult}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Func{T1,T2,TResult}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="TResult">
        ///   A <see cref="T:System.Func{T1,T2,TResult}" /> return type.
        /// </typeparam>
        /// <param name="lambda">
        ///   The <see cref="T:System.Func{T1,T2,TResult}" /> to return.
        /// </param>
        /// <summary>
        ///   Creates a <see cref="T:System.Func{T1,T2,TResult}" /> delegate.
        /// </summary>
        /// <returns>
        ///   Returns <paramref name="lambda" />.
        /// </returns>
        public static System.Func<T1, T2, TResult> F<T1, T2, TResult>(System.Func<T1, T2, TResult> lambda)
        
        
        
        {
            return lambda;
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Linq.Expressions.Expression{System.Action{T1,T2}}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Linq.Expressions.Expression{System.Action{T1,T2}}" /> parameter type.
        /// </typeparam>
        /// <param name="expr">
        ///   The <see cref="T:System.Linq.Expressions.Expression{System.Action{T1,T2}}" /> to return.
        /// </param>
        /// <summary>
        ///   Creates a <see cref="T:System.Linq.Expressions.Expression{System.Action{T1,T2}}" /> expression tree.
        /// </summary>
        /// <returns>
        ///   Returns <paramref name="expr" />.
        /// </returns>
        public static System.Linq.Expressions.Expression<System.Action<T1, T2>> XA<T1, T2>(System.Linq.Expressions.Expression<System.Action<T1, T2>> expr)
        
        
        {
            return expr;
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Linq.Expressions.Expression{System.Func{T1,T2,TResult}}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Linq.Expressions.Expression{System.Func{T1,T2,TResult}}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="TResult">
        ///   A <see cref="T:System.Linq.Expressions.Expression{System.Func{T1,T2,TResult}}" /> return type.
        /// </typeparam>
        /// <param name="expr">
        ///   The <see cref="T:System.Linq.Expressions.Expression{System.Func{T1,T2,TResult}}" /> to return.
        /// </param>
        /// <summary>
        ///   Creates a <see cref="T:System.Linq.Expressions.Expression{System.Func{T1,T2,TResult}}" /> expression tree.
        /// </summary>
        /// <returns>
        ///   Returns <paramref name="expr" />.
        /// </returns>
        public static System.Linq.Expressions.Expression<System.Func<T1, T2, TResult>> XF<T1, T2, TResult>(System.Linq.Expressions.Expression<System.Func<T1, T2, TResult>> expr)
        
        
        
        {
            return expr;
        }
        
        // From: http://blogs.msdn.com/madst/archive/2007/05/11/recursive-lambda-expressions.aspx
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Func{T1,T2,TResult}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Func{T1,T2,TResult}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="TResult">
        ///   A <see cref="T:System.Func{T1,T2,TResult}" /> return type.
        /// </typeparam>
        /// <param name="lambda">
        ///   The <see cref="T:System.Func{T1,T2,TResult}" /> to use.
        /// </param>
        /// <summary>
        ///   Creates a <see cref="T:System.Func{T1,T2,TResult}" /> delegate, which may be recursive.
        /// </summary>
        /// <returns>
        ///   Returns a <see cref="T:System.Func{T1,T2,TResult}" /> which (eventually) invokes <paramref name="lambda"/>.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="lambda"/> is <see langword="null" />.
        /// </exception>
        /// <remarks>
        ///   <para>
        ///     The following example makes use of a recursive lambda:
        ///   </para>
        ///   <code lang="C#">
        ///     Func&lt;int, int> factorial = Lambda.RecFunc&lt;int, int> (
        ///         fac => x => x == 0 ? 1 : x * fac (x-1));
        ///     Console.WriteLine (factorial (5));  // prints "120"
        ///   </code>
        /// </remarks>
        public static System.Func<T1, T2, TResult> RecFunc<T1, T2, TResult>(Func<System.Func<T1, T2, TResult>, System.Func<T1, T2, TResult>> lambda)
        
        
        
        {
            if ((lambda == null))
            {
                throw new System.ArgumentNullException("lambda");
            }
            return (value1, value2) => lambda (RecFunc (lambda))(value1, value2);
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
        /// <param name="lambda">
        ///   The <see cref="T:System.Action{T1,T2,T3}" /> to return.
        /// </param>
        /// <summary>
        ///   Creates a <see cref="T:System.Action{T1,T2,T3}" /> delegate.
        /// </summary>
        /// <returns>
        ///   Returns <paramref name="lambda" />.
        /// </returns>
        public static System.Action<T1, T2, T3> A<T1, T2, T3>(System.Action<T1, T2, T3> lambda)
        
        
        
        {
            return lambda;
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
        /// <typeparam name="TResult">
        ///   A <see cref="T:System.Func{T1,T2,T3,TResult}" /> return type.
        /// </typeparam>
        /// <param name="lambda">
        ///   The <see cref="T:System.Func{T1,T2,T3,TResult}" /> to return.
        /// </param>
        /// <summary>
        ///   Creates a <see cref="T:System.Func{T1,T2,T3,TResult}" /> delegate.
        /// </summary>
        /// <returns>
        ///   Returns <paramref name="lambda" />.
        /// </returns>
        public static System.Func<T1, T2, T3, TResult> F<T1, T2, T3, TResult>(System.Func<T1, T2, T3, TResult> lambda)
        
        
        
        
        {
            return lambda;
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Linq.Expressions.Expression{System.Action{T1,T2,T3}}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Linq.Expressions.Expression{System.Action{T1,T2,T3}}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T3">
        ///   A <see cref="T:System.Linq.Expressions.Expression{System.Action{T1,T2,T3}}" /> parameter type.
        /// </typeparam>
        /// <param name="expr">
        ///   The <see cref="T:System.Linq.Expressions.Expression{System.Action{T1,T2,T3}}" /> to return.
        /// </param>
        /// <summary>
        ///   Creates a <see cref="T:System.Linq.Expressions.Expression{System.Action{T1,T2,T3}}" /> expression tree.
        /// </summary>
        /// <returns>
        ///   Returns <paramref name="expr" />.
        /// </returns>
        public static System.Linq.Expressions.Expression<System.Action<T1, T2, T3>> XA<T1, T2, T3>(System.Linq.Expressions.Expression<System.Action<T1, T2, T3>> expr)
        
        
        
        {
            return expr;
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Linq.Expressions.Expression{System.Func{T1,T2,T3,TResult}}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Linq.Expressions.Expression{System.Func{T1,T2,T3,TResult}}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T3">
        ///   A <see cref="T:System.Linq.Expressions.Expression{System.Func{T1,T2,T3,TResult}}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="TResult">
        ///   A <see cref="T:System.Linq.Expressions.Expression{System.Func{T1,T2,T3,TResult}}" /> return type.
        /// </typeparam>
        /// <param name="expr">
        ///   The <see cref="T:System.Linq.Expressions.Expression{System.Func{T1,T2,T3,TResult}}" /> to return.
        /// </param>
        /// <summary>
        ///   Creates a <see cref="T:System.Linq.Expressions.Expression{System.Func{T1,T2,T3,TResult}}" /> expression tree.
        /// </summary>
        /// <returns>
        ///   Returns <paramref name="expr" />.
        /// </returns>
        public static System.Linq.Expressions.Expression<System.Func<T1, T2, T3, TResult>> XF<T1, T2, T3, TResult>(System.Linq.Expressions.Expression<System.Func<T1, T2, T3, TResult>> expr)
        
        
        
        
        {
            return expr;
        }
        
        // From: http://blogs.msdn.com/madst/archive/2007/05/11/recursive-lambda-expressions.aspx
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Func{T1,T2,T3,TResult}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Func{T1,T2,T3,TResult}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T3">
        ///   A <see cref="T:System.Func{T1,T2,T3,TResult}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="TResult">
        ///   A <see cref="T:System.Func{T1,T2,T3,TResult}" /> return type.
        /// </typeparam>
        /// <param name="lambda">
        ///   The <see cref="T:System.Func{T1,T2,T3,TResult}" /> to use.
        /// </param>
        /// <summary>
        ///   Creates a <see cref="T:System.Func{T1,T2,T3,TResult}" /> delegate, which may be recursive.
        /// </summary>
        /// <returns>
        ///   Returns a <see cref="T:System.Func{T1,T2,T3,TResult}" /> which (eventually) invokes <paramref name="lambda"/>.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="lambda"/> is <see langword="null" />.
        /// </exception>
        /// <remarks>
        ///   <para>
        ///     The following example makes use of a recursive lambda:
        ///   </para>
        ///   <code lang="C#">
        ///     Func&lt;int, int> factorial = Lambda.RecFunc&lt;int, int> (
        ///         fac => x => x == 0 ? 1 : x * fac (x-1));
        ///     Console.WriteLine (factorial (5));  // prints "120"
        ///   </code>
        /// </remarks>
        public static System.Func<T1, T2, T3, TResult> RecFunc<T1, T2, T3, TResult>(Func<System.Func<T1, T2, T3, TResult>, System.Func<T1, T2, T3, TResult>> lambda)
        
        
        
        
        {
            if ((lambda == null))
            {
                throw new System.ArgumentNullException("lambda");
            }
            return (value1, value2, value3) => lambda (RecFunc (lambda))(value1, value2, value3);
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
        /// <param name="lambda">
        ///   The <see cref="T:System.Action{T1,T2,T3,T4}" /> to return.
        /// </param>
        /// <summary>
        ///   Creates a <see cref="T:System.Action{T1,T2,T3,T4}" /> delegate.
        /// </summary>
        /// <returns>
        ///   Returns <paramref name="lambda" />.
        /// </returns>
        public static System.Action<T1, T2, T3, T4> A<T1, T2, T3, T4>(System.Action<T1, T2, T3, T4> lambda)
        
        
        
        
        {
            return lambda;
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
        /// <typeparam name="TResult">
        ///   A <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> return type.
        /// </typeparam>
        /// <param name="lambda">
        ///   The <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> to return.
        /// </param>
        /// <summary>
        ///   Creates a <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> delegate.
        /// </summary>
        /// <returns>
        ///   Returns <paramref name="lambda" />.
        /// </returns>
        public static System.Func<T1, T2, T3, T4, TResult> F<T1, T2, T3, T4, TResult>(System.Func<T1, T2, T3, T4, TResult> lambda)
        
        
        
        
        
        {
            return lambda;
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Linq.Expressions.Expression{System.Action{T1,T2,T3,T4}}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Linq.Expressions.Expression{System.Action{T1,T2,T3,T4}}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T3">
        ///   A <see cref="T:System.Linq.Expressions.Expression{System.Action{T1,T2,T3,T4}}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T4">
        ///   A <see cref="T:System.Linq.Expressions.Expression{System.Action{T1,T2,T3,T4}}" /> parameter type.
        /// </typeparam>
        /// <param name="expr">
        ///   The <see cref="T:System.Linq.Expressions.Expression{System.Action{T1,T2,T3,T4}}" /> to return.
        /// </param>
        /// <summary>
        ///   Creates a <see cref="T:System.Linq.Expressions.Expression{System.Action{T1,T2,T3,T4}}" /> expression tree.
        /// </summary>
        /// <returns>
        ///   Returns <paramref name="expr" />.
        /// </returns>
        public static System.Linq.Expressions.Expression<System.Action<T1, T2, T3, T4>> XA<T1, T2, T3, T4>(System.Linq.Expressions.Expression<System.Action<T1, T2, T3, T4>> expr)
        
        
        
        
        {
            return expr;
        }
        
        /// <typeparam name="T1">
        ///   A <see cref="T:System.Linq.Expressions.Expression{System.Func{T1,T2,T3,T4,TResult}}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   A <see cref="T:System.Linq.Expressions.Expression{System.Func{T1,T2,T3,T4,TResult}}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T3">
        ///   A <see cref="T:System.Linq.Expressions.Expression{System.Func{T1,T2,T3,T4,TResult}}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="T4">
        ///   A <see cref="T:System.Linq.Expressions.Expression{System.Func{T1,T2,T3,T4,TResult}}" /> parameter type.
        /// </typeparam>
        /// <typeparam name="TResult">
        ///   A <see cref="T:System.Linq.Expressions.Expression{System.Func{T1,T2,T3,T4,TResult}}" /> return type.
        /// </typeparam>
        /// <param name="expr">
        ///   The <see cref="T:System.Linq.Expressions.Expression{System.Func{T1,T2,T3,T4,TResult}}" /> to return.
        /// </param>
        /// <summary>
        ///   Creates a <see cref="T:System.Linq.Expressions.Expression{System.Func{T1,T2,T3,T4,TResult}}" /> expression tree.
        /// </summary>
        /// <returns>
        ///   Returns <paramref name="expr" />.
        /// </returns>
        public static System.Linq.Expressions.Expression<System.Func<T1, T2, T3, T4, TResult>> XF<T1, T2, T3, T4, TResult>(System.Linq.Expressions.Expression<System.Func<T1, T2, T3, T4, TResult>> expr)
        
        
        
        
        
        {
            return expr;
        }
        
        // From: http://blogs.msdn.com/madst/archive/2007/05/11/recursive-lambda-expressions.aspx
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
        /// <typeparam name="TResult">
        ///   A <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> return type.
        /// </typeparam>
        /// <param name="lambda">
        ///   The <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> to use.
        /// </param>
        /// <summary>
        ///   Creates a <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> delegate, which may be recursive.
        /// </summary>
        /// <returns>
        ///   Returns a <see cref="T:System.Func{T1,T2,T3,T4,TResult}" /> which (eventually) invokes <paramref name="lambda"/>.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="lambda"/> is <see langword="null" />.
        /// </exception>
        /// <remarks>
        ///   <para>
        ///     The following example makes use of a recursive lambda:
        ///   </para>
        ///   <code lang="C#">
        ///     Func&lt;int, int> factorial = Lambda.RecFunc&lt;int, int> (
        ///         fac => x => x == 0 ? 1 : x * fac (x-1));
        ///     Console.WriteLine (factorial (5));  // prints "120"
        ///   </code>
        /// </remarks>
        public static System.Func<T1, T2, T3, T4, TResult> RecFunc<T1, T2, T3, T4, TResult>(Func<System.Func<T1, T2, T3, T4, TResult>, System.Func<T1, T2, T3, T4, TResult>> lambda)
        
        
        
        
        
        {
            if ((lambda == null))
            {
                throw new System.ArgumentNullException("lambda");
            }
            return (value1, value2, value3, value4) => lambda (RecFunc (lambda))(value1, value2, value3, value4);
        }
    }
}
