// 
// Eithers.cs: Either Types.
// 
// GENERATED CODE: DO NOT EDIT.
// 
// To regenerate this code, execute: Eithers.exe -n 4 -o Cadenza/Either.g.cs
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
    
    
    /// <typeparam name="T1">
    ///   The first value type.
    /// </typeparam>
    /// <typeparam name="T2">
    ///   The second value type.
    /// </typeparam>
    /// <summary>
    ///   A union of 2 values.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///    An <c>Either</c> is an immutable, strongly typed union of variously 
    ///    typed values with each value lacking an otherwise meaningful name aside 
    ///    from its position, which is not exposed.  It stores only one (non-null) 
    ///    value from a set of types (as determined by the type parameter list).
    ///   </para>
    ///   <para>
    ///    The value held by a <see cref="T:Cadenza.Either{T1,T2}" /> instance
    ///    can be converted into a value by using the 
    ///    <see cref="M:Cadenza.Either{T1,T2}.Fold``1(System.Func{`0,``0},System.Func{`1,``0})" /> method.
    ///    <c>Fold</c> takes a list of delegates to perform the conversion; the
    ///    delegate used to perform the conversion is based upon the internal 
    ///    position of the value stored.
    ///   </para>
    ///   <para>
    ///    <c>Either</c> instances are created through one of the following
    ///    creation methods:
    ///   </para>
    ///   <list type="bullet">
    ///     <item><term><see cref="M:Cadenza.Either{T1,T2}.A(`0)" /></term></item>
    ///     <item><term><see cref="M:Cadenza.Either{T1,T2}.op_Implicit(`0)~Cadenza.Either{`0,`1}" /></term></item>
    ///     <item><term><see cref="M:Cadenza.Either{T1,T2}.B(`1)" /></term></item>
    ///     <item><term><see cref="M:Cadenza.Either{T1,T2}.op_Implicit(`1)~Cadenza.Either{`0,`1}" /></term></item>
    ///   </list>
    ///   <code lang="C#">
    ///   var a = Either&lt;double, string&gt;.A (Math.PI);   // value stored in 1st position
    ///   
    ///   int r = a.Fold (
    ///           v => (int) v,                                 // 1st position converter
    ///           v => v.Length);                               // 2nd position converter
    ///   
    ///   Console.WriteLine (r);                        // prints 3
    ///  
    ///   // alternatively...
    ///   Either&lt;double, string&gt; b = "value";           // value stored in 2nd position
    ///   Console.WriteLine (b.Fold (v => v.ToString(), v => v));
    ///                                                 // prints "value"</code>
    /// </remarks>
    public abstract class Either<T1, T2> : IEquatable<Either<T1, T2>>
    
    
    {
        
        
        /// <param name="value">
        ///    A <typeparamref name="T1" /> containing the value to store.
        /// </param>
        /// <summary>
        ///    Creates a <see cref="T:Cadenza.Either{T1,T2}" /> instance which
        ///    holds a <typeparamref name="T1" /> value.
        /// </summary>
        /// <returns>
        ///    A <see cref="T:Cadenza.Either{T1,T2}" /> instance which
        ///    holds a <typeparamref name="T1" /> value.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///     This conversion operator is provided to make
        ///     <see cref="T:Cadenza.Either{T1,T2}" /> instance creation easier:
        ///   </para>
        ///   <code lang="C#">
        /// Either&lt;int, double&gt; value = 42.0;  // value stored in 2nd position</code>
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        ///    if <paramref name="value" /> is <see langword="null" />.
        /// </exception>
        /// <seealso cref="M:Cadenza.Either{T1,T2}.A(`0)" />
        public static implicit operator Either<T1,T2>(T1 value)
        {
            return Either<T1,T2>.A (value);
        }

        
        /// <param name="value">
        ///    A <typeparamref name="T2" /> containing the value to store.
        /// </param>
        /// <summary>
        ///    Creates a <see cref="T:Cadenza.Either{T1,T2}" /> instance which
        ///    holds a <typeparamref name="T2" /> value.
        /// </summary>
        /// <returns>
        ///    A <see cref="T:Cadenza.Either{T1,T2}" /> instance which
        ///    holds a <typeparamref name="T2" /> value.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///     This conversion operator is provided to make
        ///     <see cref="T:Cadenza.Either{T1,T2}" /> instance creation easier:
        ///   </para>
        ///   <code lang="C#">
        /// Either&lt;int, double&gt; value = 42.0;  // value stored in 2nd position</code>
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        ///    if <paramref name="value" /> is <see langword="null" />.
        /// </exception>
        /// <seealso cref="M:Cadenza.Either{T1,T2}.B(`1)" />
        public static implicit operator Either<T1,T2>(T2 value)
        {
            return Either<T1,T2>.B (value);
        }

        private Either()
        {
        }
        
        /// <param name="value">
        ///     A <typeparamref name="T1" /> containing the value
        ///     to provide to the first
        ///     <see cref="M:Cadenza.Either{T1,T2}.Fold``1(System.Func{`0,``0},System.Func{`1,``0})" />
        ///     delegate.
        /// </param>
        /// <summary>
        ///     Creates a <see cref="T:Cadenza.Either{T1,T2}" /> instance which
        ///     holds a <typeparamref name="T1" /> value.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:Cadenza.Either{T1,T2}" /> instance which holds a 
        ///     holds a <typeparamref name="T1" /> value.
        /// </returns>
        /// <remarks>
        ///     <para>
        ///      When
        ///      <see cref="M:Cadenza.Either{T1,T2}.Fold``1(System.Func{`0,``0},System.Func{`1,``0})" />
        ///      is invoked,
        ///      the returned <see cref="T:Cadenza.Either{T1,T2}" /> instance
        ///      will invoke the first delegate
        ///      for conversions.
        ///     </para>
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="value"/> is <see langword="null" />.
        /// </exception>
        public static Either<T1, T2> A(T1 value)
        {
            if ((value == null))
            {
                throw new System.ArgumentNullException("value");
            }
            return new AHandler(value);
        }
        
        /// <param name="value">
        ///     A <typeparamref name="T2" /> containing the value
        ///     to provide to the second
        ///     <see cref="M:Cadenza.Either{T1,T2}.Fold``1(System.Func{`0,``0},System.Func{`1,``0})" />
        ///     delegate.
        /// </param>
        /// <summary>
        ///     Creates a <see cref="T:Cadenza.Either{T1,T2}" /> instance which
        ///     holds a <typeparamref name="T2" /> value.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:Cadenza.Either{T1,T2}" /> instance which holds a 
        ///     holds a <typeparamref name="T2" /> value.
        /// </returns>
        /// <remarks>
        ///     <para>
        ///      When
        ///      <see cref="M:Cadenza.Either{T1,T2}.Fold``1(System.Func{`0,``0},System.Func{`1,``0})" />
        ///      is invoked,
        ///      the returned <see cref="T:Cadenza.Either{T1,T2}" /> instance
        ///      will invoke the second delegate
        ///      for conversions.
        ///     </para>
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="value"/> is <see langword="null" />.
        /// </exception>
        public static Either<T1, T2> B(T2 value)
        {
            if ((value == null))
            {
                throw new System.ArgumentNullException("value");
            }
            return new BHandler(value);
        }
        
        /// <typeparam name="TResult">
        ///     The type to convert the <see cref="T:Cadenza.Either{T1,T2}" /> to.
        /// </typeparam>
        /// <param name="a">
        ///   A <see cref="T:System.Func{T1,TResult}" /> 
        ///   used if the <see cref="T:Cadenza.Either{T1,T2}" /> stores a 
        ///   <typeparamref name="T1" /> value into a 
        ///   <typeparamref name="TResult" /> value.
        /// </param>
        /// <param name="b">
        ///   A <see cref="T:System.Func{T2,TResult}" /> 
        ///   used if the <see cref="T:Cadenza.Either{T1,T2}" /> stores a 
        ///   <typeparamref name="T2" /> value into a 
        ///   <typeparamref name="TResult" /> value.
        /// </param>
        /// <summary>
        ///     Converts a <see cref="T:Cadenza.Either{T1,T2}" /> into a <typeparamref name="TResult" /> value.
        /// </summary>
        /// <returns>
        ///     A <typeparamref name="TResult" /> as generated by one
        ///     of the conversion delegate parameters.
        /// </returns>
        /// <remarks>
        ///     <para>
        ///      Converts a <see cref="T:Cadenza.Either{T1,T2}" /> into a <typeparamref name="TResult" />
        ///      by invoking one of the provided delegate parameters.
        ///     </para>
        ///     <para>
        ///      The parameter which is invoked is predicated upon the internal position of
        ///      the value held.  For example, if the internal value is in the first position 
        ///      (i.e. <see cref="M:Cadenza.Either{T1,T2}.A(`0)" />
        ///      was used to create the <see cref="T:Cadenza.Either{T1,T2}" /> instance), then
        ///      <paramref name="a" /> (the first delegate parameter) will be invoked to
        ///      convert the <typeparamref name="T1" /> into a 
        ///      <typeparamref name="TResult" />.
        ///     </para>
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <para>
        ///     <paramref name="a" /> is <see langword="null" />.
        ///   </para>
        ///   <para>-or-</para>
        ///   <para>
        ///     <paramref name="b" /> is <see langword="null" />.
        ///   </para>
        /// </exception>
        public abstract TResult Fold<TResult>(System.Func<T1, TResult> a, System.Func<T2, TResult> b)
        ;
        
        private static void CheckFolders<TResult>(System.Func<T1, TResult> a, System.Func<T2, TResult> b)
        
        {
            if ((a == null))
            {
                throw new System.ArgumentNullException("a");
            }
            if ((b == null))
            {
                throw new System.ArgumentNullException("b");
            }
        }
        
        /// <param name="obj">
        ///     A <see cref="T:System.Object"/> to compare this instance against.
        /// </param>
        /// <summary>
        ///     Determines whether the current instance and the specified object have the same value.
        /// </summary>
        /// <returns>
        ///     <para>
        ///      <see langword="true"/> if <paramref name="obj"/> is a 
        ///      <see cref="T:Cadenza.Either{T1,T2}" /> and each member of <paramref name="obj"/>
        ///      and the current instance have the same value (according to
        ///      <see cref="M:System.Object.Equals(System.Object)"/>); otherwise
        ///      <see langword="false"/> is returned.
        ///     </para>
        /// </returns>
        /// <remarks>
        ///     <para>
        ///      This method checks for value equality 
        ///      (<see cref="M:System.Object.Equals(System.Object)"/>), as defined by each
        ///      value type.
        ///     </para>
        ///     <para>
        ///      <block subset="none" type="note">
        ///       This method overrides <see cref="M:System.Object.Equals(System.Object)"/>.
        ///      </block>
        ///     </para>
        /// </remarks>
        public abstract override System.Boolean Equals(object obj);
        
        /// <param name="obj">
        ///   A <see cref="T:Cadenza.Either{T1,T2}" />to compare this instance against.
        /// </param>
        /// <summary>
        ///     Determines whether the current instance and the specified <see cref="T:Cadenza.Either{T1,T2}" /> have the same value.
        /// </summary>
        /// <returns>
        ///     <para>
        ///      <see langword="true"/> if each member of <paramref name="obj"/>
        ///      and the current instance have the same value (according to
        ///      <see cref="M:System.Object.Equals(System.Object)"/>); otherwise
        ///      <see langword="false"/> is returned.
        ///     </para>
        /// </returns>
        /// <remarks>
        ///     <para>
        ///      This method checks for value equality
        ///      (<see cref="M:System.Object.Equals(System.Object)"/>), as defined by each
        ///      value type.
        ///     </para>
        /// </remarks>
        public abstract bool Equals(Either<T1, T2> obj);
        
        /// <summary>
        ///     Generates a hash code for the current instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.Int32"/> containing the hash code for this instance.
        /// </returns>
        /// <remarks>
        ///     <para>
        ///      <block subset="none" type="note">
        ///       This method overrides <see cref="M:System.Object.GetHashCode"/>.
        ///      </block>
        ///     </para>
        /// </remarks>
        public abstract override System.Int32 GetHashCode();
        
        private class AHandler : Either<T1, T2>
        {
            
            private T1 _value;
            
            public AHandler(T1 value)
            {
                _value = value;
            }
            
            public override int GetHashCode()
            {
                return _value.GetHashCode();
            }
            
            public override bool Equals(object obj)
            {
                AHandler o = obj as AHandler;
                if ((o == null))
                {
                    return false;
                }
                return this.Equals(o);
            }
            
            public override bool Equals(Either<T1, T2> obj)
            {
                AHandler o = obj as AHandler;
                if ((o == null))
                {
                    return false;
                }
                return System.Collections.Generic.EqualityComparer<T1>.Default.Equals(this._value, o._value);
            }
            
            public override TResult Fold<TResult>(System.Func<T1, TResult> a, System.Func<T2, TResult> b)
            
            {
                CheckFolders(a, b);
                return a(_value);
            }
        }
        
        private class BHandler : Either<T1, T2>
        {
            
            private T2 _value;
            
            public BHandler(T2 value)
            {
                _value = value;
            }
            
            public override int GetHashCode()
            {
                return _value.GetHashCode();
            }
            
            public override bool Equals(object obj)
            {
                BHandler o = obj as BHandler;
                if ((o == null))
                {
                    return false;
                }
                return this.Equals(o);
            }
            
            public override bool Equals(Either<T1, T2> obj)
            {
                BHandler o = obj as BHandler;
                if ((o == null))
                {
                    return false;
                }
                return System.Collections.Generic.EqualityComparer<T2>.Default.Equals(this._value, o._value);
            }
            
            public override TResult Fold<TResult>(System.Func<T1, TResult> a, System.Func<T2, TResult> b)
            
            {
                CheckFolders(a, b);
                return b(_value);
            }
        }
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
    /// <summary>
    ///   A union of 3 values.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///    An <c>Either</c> is an immutable, strongly typed union of variously 
    ///    typed values with each value lacking an otherwise meaningful name aside 
    ///    from its position, which is not exposed.  It stores only one (non-null) 
    ///    value from a set of types (as determined by the type parameter list).
    ///   </para>
    ///   <para>
    ///    The value held by a <see cref="T:Cadenza.Either{T1,T2,T3}" /> instance
    ///    can be converted into a value by using the 
    ///    <see cref="M:Cadenza.Either{T1,T2,T3}.Fold``1(System.Func{`0,``0},System.Func{`1,``0},System.Func{`2,``0})" /> method.
    ///    <c>Fold</c> takes a list of delegates to perform the conversion; the
    ///    delegate used to perform the conversion is based upon the internal 
    ///    position of the value stored.
    ///   </para>
    ///   <para>
    ///    <c>Either</c> instances are created through one of the following
    ///    creation methods:
    ///   </para>
    ///   <list type="bullet">
    ///     <item><term><see cref="M:Cadenza.Either{T1,T2,T3}.A(`0)" /></term></item>
    ///     <item><term><see cref="M:Cadenza.Either{T1,T2,T3}.op_Implicit(`0)~Cadenza.Either{`0,`1,`2}" /></term></item>
    ///     <item><term><see cref="M:Cadenza.Either{T1,T2,T3}.B(`1)" /></term></item>
    ///     <item><term><see cref="M:Cadenza.Either{T1,T2,T3}.op_Implicit(`1)~Cadenza.Either{`0,`1,`2}" /></term></item>
    ///     <item><term><see cref="M:Cadenza.Either{T1,T2,T3}.C(`2)" /></term></item>
    ///     <item><term><see cref="M:Cadenza.Either{T1,T2,T3}.op_Implicit(`2)~Cadenza.Either{`0,`1,`2}" /></term></item>
    ///   </list>
    ///   <code lang="C#">
    ///   var a = Either&lt;double, string&gt;.A (Math.PI);   // value stored in 1st position
    ///   
    ///   int r = a.Fold (
    ///           v => (int) v,                                 // 1st position converter
    ///           v => v.Length);                               // 2nd position converter
    ///   
    ///   Console.WriteLine (r);                        // prints 3
    ///  
    ///   // alternatively...
    ///   Either&lt;double, string&gt; b = "value";           // value stored in 2nd position
    ///   Console.WriteLine (b.Fold (v => v.ToString(), v => v));
    ///                                                 // prints "value"</code>
    /// </remarks>
    public abstract class Either<T1, T2, T3> : IEquatable<Either<T1, T2, T3>>
    
    
    
    {
        
        
        /// <param name="value">
        ///    A <typeparamref name="T1" /> containing the value to store.
        /// </param>
        /// <summary>
        ///    Creates a <see cref="T:Cadenza.Either{T1,T2,T3}" /> instance which
        ///    holds a <typeparamref name="T1" /> value.
        /// </summary>
        /// <returns>
        ///    A <see cref="T:Cadenza.Either{T1,T2,T3}" /> instance which
        ///    holds a <typeparamref name="T1" /> value.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///     This conversion operator is provided to make
        ///     <see cref="T:Cadenza.Either{T1,T2,T3}" /> instance creation easier:
        ///   </para>
        ///   <code lang="C#">
        /// Either&lt;int, double&gt; value = 42.0;  // value stored in 2nd position</code>
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        ///    if <paramref name="value" /> is <see langword="null" />.
        /// </exception>
        /// <seealso cref="M:Cadenza.Either{T1,T2,T3}.A(`0)" />
        public static implicit operator Either<T1,T2,T3>(T1 value)
        {
            return Either<T1,T2,T3>.A (value);
        }

        
        /// <param name="value">
        ///    A <typeparamref name="T2" /> containing the value to store.
        /// </param>
        /// <summary>
        ///    Creates a <see cref="T:Cadenza.Either{T1,T2,T3}" /> instance which
        ///    holds a <typeparamref name="T2" /> value.
        /// </summary>
        /// <returns>
        ///    A <see cref="T:Cadenza.Either{T1,T2,T3}" /> instance which
        ///    holds a <typeparamref name="T2" /> value.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///     This conversion operator is provided to make
        ///     <see cref="T:Cadenza.Either{T1,T2,T3}" /> instance creation easier:
        ///   </para>
        ///   <code lang="C#">
        /// Either&lt;int, double&gt; value = 42.0;  // value stored in 2nd position</code>
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        ///    if <paramref name="value" /> is <see langword="null" />.
        /// </exception>
        /// <seealso cref="M:Cadenza.Either{T1,T2,T3}.B(`1)" />
        public static implicit operator Either<T1,T2,T3>(T2 value)
        {
            return Either<T1,T2,T3>.B (value);
        }

        
        /// <param name="value">
        ///    A <typeparamref name="T3" /> containing the value to store.
        /// </param>
        /// <summary>
        ///    Creates a <see cref="T:Cadenza.Either{T1,T2,T3}" /> instance which
        ///    holds a <typeparamref name="T3" /> value.
        /// </summary>
        /// <returns>
        ///    A <see cref="T:Cadenza.Either{T1,T2,T3}" /> instance which
        ///    holds a <typeparamref name="T3" /> value.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///     This conversion operator is provided to make
        ///     <see cref="T:Cadenza.Either{T1,T2,T3}" /> instance creation easier:
        ///   </para>
        ///   <code lang="C#">
        /// Either&lt;int, double&gt; value = 42.0;  // value stored in 2nd position</code>
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        ///    if <paramref name="value" /> is <see langword="null" />.
        /// </exception>
        /// <seealso cref="M:Cadenza.Either{T1,T2,T3}.C(`2)" />
        public static implicit operator Either<T1,T2,T3>(T3 value)
        {
            return Either<T1,T2,T3>.C (value);
        }

        private Either()
        {
        }
        
        /// <param name="value">
        ///     A <typeparamref name="T1" /> containing the value
        ///     to provide to the first
        ///     <see cref="M:Cadenza.Either{T1,T2,T3}.Fold``1(System.Func{`0,``0},System.Func{`1,``0},System.Func{`2,``0})" />
        ///     delegate.
        /// </param>
        /// <summary>
        ///     Creates a <see cref="T:Cadenza.Either{T1,T2,T3}" /> instance which
        ///     holds a <typeparamref name="T1" /> value.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:Cadenza.Either{T1,T2,T3}" /> instance which holds a 
        ///     holds a <typeparamref name="T1" /> value.
        /// </returns>
        /// <remarks>
        ///     <para>
        ///      When
        ///      <see cref="M:Cadenza.Either{T1,T2,T3}.Fold``1(System.Func{`0,``0},System.Func{`1,``0},System.Func{`2,``0})" />
        ///      is invoked,
        ///      the returned <see cref="T:Cadenza.Either{T1,T2,T3}" /> instance
        ///      will invoke the first delegate
        ///      for conversions.
        ///     </para>
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="value"/> is <see langword="null" />.
        /// </exception>
        public static Either<T1, T2, T3> A(T1 value)
        {
            if ((value == null))
            {
                throw new System.ArgumentNullException("value");
            }
            return new AHandler(value);
        }
        
        /// <param name="value">
        ///     A <typeparamref name="T2" /> containing the value
        ///     to provide to the second
        ///     <see cref="M:Cadenza.Either{T1,T2,T3}.Fold``1(System.Func{`0,``0},System.Func{`1,``0},System.Func{`2,``0})" />
        ///     delegate.
        /// </param>
        /// <summary>
        ///     Creates a <see cref="T:Cadenza.Either{T1,T2,T3}" /> instance which
        ///     holds a <typeparamref name="T2" /> value.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:Cadenza.Either{T1,T2,T3}" /> instance which holds a 
        ///     holds a <typeparamref name="T2" /> value.
        /// </returns>
        /// <remarks>
        ///     <para>
        ///      When
        ///      <see cref="M:Cadenza.Either{T1,T2,T3}.Fold``1(System.Func{`0,``0},System.Func{`1,``0},System.Func{`2,``0})" />
        ///      is invoked,
        ///      the returned <see cref="T:Cadenza.Either{T1,T2,T3}" /> instance
        ///      will invoke the second delegate
        ///      for conversions.
        ///     </para>
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="value"/> is <see langword="null" />.
        /// </exception>
        public static Either<T1, T2, T3> B(T2 value)
        {
            if ((value == null))
            {
                throw new System.ArgumentNullException("value");
            }
            return new BHandler(value);
        }
        
        /// <param name="value">
        ///     A <typeparamref name="T3" /> containing the value
        ///     to provide to the third
        ///     <see cref="M:Cadenza.Either{T1,T2,T3}.Fold``1(System.Func{`0,``0},System.Func{`1,``0},System.Func{`2,``0})" />
        ///     delegate.
        /// </param>
        /// <summary>
        ///     Creates a <see cref="T:Cadenza.Either{T1,T2,T3}" /> instance which
        ///     holds a <typeparamref name="T3" /> value.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:Cadenza.Either{T1,T2,T3}" /> instance which holds a 
        ///     holds a <typeparamref name="T3" /> value.
        /// </returns>
        /// <remarks>
        ///     <para>
        ///      When
        ///      <see cref="M:Cadenza.Either{T1,T2,T3}.Fold``1(System.Func{`0,``0},System.Func{`1,``0},System.Func{`2,``0})" />
        ///      is invoked,
        ///      the returned <see cref="T:Cadenza.Either{T1,T2,T3}" /> instance
        ///      will invoke the third delegate
        ///      for conversions.
        ///     </para>
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="value"/> is <see langword="null" />.
        /// </exception>
        public static Either<T1, T2, T3> C(T3 value)
        {
            if ((value == null))
            {
                throw new System.ArgumentNullException("value");
            }
            return new CHandler(value);
        }
        
        /// <typeparam name="TResult">
        ///     The type to convert the <see cref="T:Cadenza.Either{T1,T2,T3}" /> to.
        /// </typeparam>
        /// <param name="a">
        ///   A <see cref="T:System.Func{T1,TResult}" /> 
        ///   used if the <see cref="T:Cadenza.Either{T1,T2,T3}" /> stores a 
        ///   <typeparamref name="T1" /> value into a 
        ///   <typeparamref name="TResult" /> value.
        /// </param>
        /// <param name="b">
        ///   A <see cref="T:System.Func{T2,TResult}" /> 
        ///   used if the <see cref="T:Cadenza.Either{T1,T2,T3}" /> stores a 
        ///   <typeparamref name="T2" /> value into a 
        ///   <typeparamref name="TResult" /> value.
        /// </param>
        /// <param name="c">
        ///   A <see cref="T:System.Func{T3,TResult}" /> 
        ///   used if the <see cref="T:Cadenza.Either{T1,T2,T3}" /> stores a 
        ///   <typeparamref name="T3" /> value into a 
        ///   <typeparamref name="TResult" /> value.
        /// </param>
        /// <summary>
        ///     Converts a <see cref="T:Cadenza.Either{T1,T2,T3}" /> into a <typeparamref name="TResult" /> value.
        /// </summary>
        /// <returns>
        ///     A <typeparamref name="TResult" /> as generated by one
        ///     of the conversion delegate parameters.
        /// </returns>
        /// <remarks>
        ///     <para>
        ///      Converts a <see cref="T:Cadenza.Either{T1,T2,T3}" /> into a <typeparamref name="TResult" />
        ///      by invoking one of the provided delegate parameters.
        ///     </para>
        ///     <para>
        ///      The parameter which is invoked is predicated upon the internal position of
        ///      the value held.  For example, if the internal value is in the first position 
        ///      (i.e. <see cref="M:Cadenza.Either{T1,T2,T3}.A(`0)" />
        ///      was used to create the <see cref="T:Cadenza.Either{T1,T2,T3}" /> instance), then
        ///      <paramref name="a" /> (the first delegate parameter) will be invoked to
        ///      convert the <typeparamref name="T1" /> into a 
        ///      <typeparamref name="TResult" />.
        ///     </para>
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <para>
        ///     <paramref name="a" /> is <see langword="null" />.
        ///   </para>
        ///   <para>-or-</para>
        ///   <para>
        ///     <paramref name="b" /> is <see langword="null" />.
        ///   </para>
        ///   <para>-or-</para>
        ///   <para>
        ///     <paramref name="c" /> is <see langword="null" />.
        ///   </para>
        /// </exception>
        public abstract TResult Fold<TResult>(System.Func<T1, TResult> a, System.Func<T2, TResult> b, System.Func<T3, TResult> c)
        ;
        
        private static void CheckFolders<TResult>(System.Func<T1, TResult> a, System.Func<T2, TResult> b, System.Func<T3, TResult> c)
        
        {
            if ((a == null))
            {
                throw new System.ArgumentNullException("a");
            }
            if ((b == null))
            {
                throw new System.ArgumentNullException("b");
            }
            if ((c == null))
            {
                throw new System.ArgumentNullException("c");
            }
        }
        
        /// <param name="obj">
        ///     A <see cref="T:System.Object"/> to compare this instance against.
        /// </param>
        /// <summary>
        ///     Determines whether the current instance and the specified object have the same value.
        /// </summary>
        /// <returns>
        ///     <para>
        ///      <see langword="true"/> if <paramref name="obj"/> is a 
        ///      <see cref="T:Cadenza.Either{T1,T2,T3}" /> and each member of <paramref name="obj"/>
        ///      and the current instance have the same value (according to
        ///      <see cref="M:System.Object.Equals(System.Object)"/>); otherwise
        ///      <see langword="false"/> is returned.
        ///     </para>
        /// </returns>
        /// <remarks>
        ///     <para>
        ///      This method checks for value equality 
        ///      (<see cref="M:System.Object.Equals(System.Object)"/>), as defined by each
        ///      value type.
        ///     </para>
        ///     <para>
        ///      <block subset="none" type="note">
        ///       This method overrides <see cref="M:System.Object.Equals(System.Object)"/>.
        ///      </block>
        ///     </para>
        /// </remarks>
        public abstract override System.Boolean Equals(object obj);
        
        /// <param name="obj">
        ///   A <see cref="T:Cadenza.Either{T1,T2,T3}" />to compare this instance against.
        /// </param>
        /// <summary>
        ///     Determines whether the current instance and the specified <see cref="T:Cadenza.Either{T1,T2,T3}" /> have the same value.
        /// </summary>
        /// <returns>
        ///     <para>
        ///      <see langword="true"/> if each member of <paramref name="obj"/>
        ///      and the current instance have the same value (according to
        ///      <see cref="M:System.Object.Equals(System.Object)"/>); otherwise
        ///      <see langword="false"/> is returned.
        ///     </para>
        /// </returns>
        /// <remarks>
        ///     <para>
        ///      This method checks for value equality
        ///      (<see cref="M:System.Object.Equals(System.Object)"/>), as defined by each
        ///      value type.
        ///     </para>
        /// </remarks>
        public abstract bool Equals(Either<T1, T2, T3> obj);
        
        /// <summary>
        ///     Generates a hash code for the current instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.Int32"/> containing the hash code for this instance.
        /// </returns>
        /// <remarks>
        ///     <para>
        ///      <block subset="none" type="note">
        ///       This method overrides <see cref="M:System.Object.GetHashCode"/>.
        ///      </block>
        ///     </para>
        /// </remarks>
        public abstract override System.Int32 GetHashCode();
        
        private class AHandler : Either<T1, T2, T3>
        {
            
            private T1 _value;
            
            public AHandler(T1 value)
            {
                _value = value;
            }
            
            public override int GetHashCode()
            {
                return _value.GetHashCode();
            }
            
            public override bool Equals(object obj)
            {
                AHandler o = obj as AHandler;
                if ((o == null))
                {
                    return false;
                }
                return this.Equals(o);
            }
            
            public override bool Equals(Either<T1, T2, T3> obj)
            {
                AHandler o = obj as AHandler;
                if ((o == null))
                {
                    return false;
                }
                return System.Collections.Generic.EqualityComparer<T1>.Default.Equals(this._value, o._value);
            }
            
            public override TResult Fold<TResult>(System.Func<T1, TResult> a, System.Func<T2, TResult> b, System.Func<T3, TResult> c)
            
            {
                CheckFolders(a, b, c);
                return a(_value);
            }
        }
        
        private class BHandler : Either<T1, T2, T3>
        {
            
            private T2 _value;
            
            public BHandler(T2 value)
            {
                _value = value;
            }
            
            public override int GetHashCode()
            {
                return _value.GetHashCode();
            }
            
            public override bool Equals(object obj)
            {
                BHandler o = obj as BHandler;
                if ((o == null))
                {
                    return false;
                }
                return this.Equals(o);
            }
            
            public override bool Equals(Either<T1, T2, T3> obj)
            {
                BHandler o = obj as BHandler;
                if ((o == null))
                {
                    return false;
                }
                return System.Collections.Generic.EqualityComparer<T2>.Default.Equals(this._value, o._value);
            }
            
            public override TResult Fold<TResult>(System.Func<T1, TResult> a, System.Func<T2, TResult> b, System.Func<T3, TResult> c)
            
            {
                CheckFolders(a, b, c);
                return b(_value);
            }
        }
        
        private class CHandler : Either<T1, T2, T3>
        {
            
            private T3 _value;
            
            public CHandler(T3 value)
            {
                _value = value;
            }
            
            public override int GetHashCode()
            {
                return _value.GetHashCode();
            }
            
            public override bool Equals(object obj)
            {
                CHandler o = obj as CHandler;
                if ((o == null))
                {
                    return false;
                }
                return this.Equals(o);
            }
            
            public override bool Equals(Either<T1, T2, T3> obj)
            {
                CHandler o = obj as CHandler;
                if ((o == null))
                {
                    return false;
                }
                return System.Collections.Generic.EqualityComparer<T3>.Default.Equals(this._value, o._value);
            }
            
            public override TResult Fold<TResult>(System.Func<T1, TResult> a, System.Func<T2, TResult> b, System.Func<T3, TResult> c)
            
            {
                CheckFolders(a, b, c);
                return c(_value);
            }
        }
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
    /// <summary>
    ///   A union of 4 values.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///    An <c>Either</c> is an immutable, strongly typed union of variously 
    ///    typed values with each value lacking an otherwise meaningful name aside 
    ///    from its position, which is not exposed.  It stores only one (non-null) 
    ///    value from a set of types (as determined by the type parameter list).
    ///   </para>
    ///   <para>
    ///    The value held by a <see cref="T:Cadenza.Either{T1,T2,T3,T4}" /> instance
    ///    can be converted into a value by using the 
    ///    <see cref="M:Cadenza.Either{T1,T2,T3,T4}.Fold``1(System.Func{`0,``0},System.Func{`1,``0},System.Func{`2,``0},System.Func{`3,``0})" /> method.
    ///    <c>Fold</c> takes a list of delegates to perform the conversion; the
    ///    delegate used to perform the conversion is based upon the internal 
    ///    position of the value stored.
    ///   </para>
    ///   <para>
    ///    <c>Either</c> instances are created through one of the following
    ///    creation methods:
    ///   </para>
    ///   <list type="bullet">
    ///     <item><term><see cref="M:Cadenza.Either{T1,T2,T3,T4}.A(`0)" /></term></item>
    ///     <item><term><see cref="M:Cadenza.Either{T1,T2,T3,T4}.op_Implicit(`0)~Cadenza.Either{`0,`1,`2,`3}" /></term></item>
    ///     <item><term><see cref="M:Cadenza.Either{T1,T2,T3,T4}.B(`1)" /></term></item>
    ///     <item><term><see cref="M:Cadenza.Either{T1,T2,T3,T4}.op_Implicit(`1)~Cadenza.Either{`0,`1,`2,`3}" /></term></item>
    ///     <item><term><see cref="M:Cadenza.Either{T1,T2,T3,T4}.C(`2)" /></term></item>
    ///     <item><term><see cref="M:Cadenza.Either{T1,T2,T3,T4}.op_Implicit(`2)~Cadenza.Either{`0,`1,`2,`3}" /></term></item>
    ///     <item><term><see cref="M:Cadenza.Either{T1,T2,T3,T4}.D(`3)" /></term></item>
    ///     <item><term><see cref="M:Cadenza.Either{T1,T2,T3,T4}.op_Implicit(`3)~Cadenza.Either{`0,`1,`2,`3}" /></term></item>
    ///   </list>
    ///   <code lang="C#">
    ///   var a = Either&lt;double, string&gt;.A (Math.PI);   // value stored in 1st position
    ///   
    ///   int r = a.Fold (
    ///           v => (int) v,                                 // 1st position converter
    ///           v => v.Length);                               // 2nd position converter
    ///   
    ///   Console.WriteLine (r);                        // prints 3
    ///  
    ///   // alternatively...
    ///   Either&lt;double, string&gt; b = "value";           // value stored in 2nd position
    ///   Console.WriteLine (b.Fold (v => v.ToString(), v => v));
    ///                                                 // prints "value"</code>
    /// </remarks>
    public abstract class Either<T1, T2, T3, T4> : IEquatable<Either<T1, T2, T3, T4>>
    
    
    
    
    {
        
        
        /// <param name="value">
        ///    A <typeparamref name="T1" /> containing the value to store.
        /// </param>
        /// <summary>
        ///    Creates a <see cref="T:Cadenza.Either{T1,T2,T3,T4}" /> instance which
        ///    holds a <typeparamref name="T1" /> value.
        /// </summary>
        /// <returns>
        ///    A <see cref="T:Cadenza.Either{T1,T2,T3,T4}" /> instance which
        ///    holds a <typeparamref name="T1" /> value.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///     This conversion operator is provided to make
        ///     <see cref="T:Cadenza.Either{T1,T2,T3,T4}" /> instance creation easier:
        ///   </para>
        ///   <code lang="C#">
        /// Either&lt;int, double&gt; value = 42.0;  // value stored in 2nd position</code>
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        ///    if <paramref name="value" /> is <see langword="null" />.
        /// </exception>
        /// <seealso cref="M:Cadenza.Either{T1,T2,T3,T4}.A(`0)" />
        public static implicit operator Either<T1,T2,T3,T4>(T1 value)
        {
            return Either<T1,T2,T3,T4>.A (value);
        }

        
        /// <param name="value">
        ///    A <typeparamref name="T2" /> containing the value to store.
        /// </param>
        /// <summary>
        ///    Creates a <see cref="T:Cadenza.Either{T1,T2,T3,T4}" /> instance which
        ///    holds a <typeparamref name="T2" /> value.
        /// </summary>
        /// <returns>
        ///    A <see cref="T:Cadenza.Either{T1,T2,T3,T4}" /> instance which
        ///    holds a <typeparamref name="T2" /> value.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///     This conversion operator is provided to make
        ///     <see cref="T:Cadenza.Either{T1,T2,T3,T4}" /> instance creation easier:
        ///   </para>
        ///   <code lang="C#">
        /// Either&lt;int, double&gt; value = 42.0;  // value stored in 2nd position</code>
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        ///    if <paramref name="value" /> is <see langword="null" />.
        /// </exception>
        /// <seealso cref="M:Cadenza.Either{T1,T2,T3,T4}.B(`1)" />
        public static implicit operator Either<T1,T2,T3,T4>(T2 value)
        {
            return Either<T1,T2,T3,T4>.B (value);
        }

        
        /// <param name="value">
        ///    A <typeparamref name="T3" /> containing the value to store.
        /// </param>
        /// <summary>
        ///    Creates a <see cref="T:Cadenza.Either{T1,T2,T3,T4}" /> instance which
        ///    holds a <typeparamref name="T3" /> value.
        /// </summary>
        /// <returns>
        ///    A <see cref="T:Cadenza.Either{T1,T2,T3,T4}" /> instance which
        ///    holds a <typeparamref name="T3" /> value.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///     This conversion operator is provided to make
        ///     <see cref="T:Cadenza.Either{T1,T2,T3,T4}" /> instance creation easier:
        ///   </para>
        ///   <code lang="C#">
        /// Either&lt;int, double&gt; value = 42.0;  // value stored in 2nd position</code>
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        ///    if <paramref name="value" /> is <see langword="null" />.
        /// </exception>
        /// <seealso cref="M:Cadenza.Either{T1,T2,T3,T4}.C(`2)" />
        public static implicit operator Either<T1,T2,T3,T4>(T3 value)
        {
            return Either<T1,T2,T3,T4>.C (value);
        }

        
        /// <param name="value">
        ///    A <typeparamref name="T4" /> containing the value to store.
        /// </param>
        /// <summary>
        ///    Creates a <see cref="T:Cadenza.Either{T1,T2,T3,T4}" /> instance which
        ///    holds a <typeparamref name="T4" /> value.
        /// </summary>
        /// <returns>
        ///    A <see cref="T:Cadenza.Either{T1,T2,T3,T4}" /> instance which
        ///    holds a <typeparamref name="T4" /> value.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///     This conversion operator is provided to make
        ///     <see cref="T:Cadenza.Either{T1,T2,T3,T4}" /> instance creation easier:
        ///   </para>
        ///   <code lang="C#">
        /// Either&lt;int, double&gt; value = 42.0;  // value stored in 2nd position</code>
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        ///    if <paramref name="value" /> is <see langword="null" />.
        /// </exception>
        /// <seealso cref="M:Cadenza.Either{T1,T2,T3,T4}.D(`3)" />
        public static implicit operator Either<T1,T2,T3,T4>(T4 value)
        {
            return Either<T1,T2,T3,T4>.D (value);
        }

        private Either()
        {
        }
        
        /// <param name="value">
        ///     A <typeparamref name="T1" /> containing the value
        ///     to provide to the first
        ///     <see cref="M:Cadenza.Either{T1,T2,T3,T4}.Fold``1(System.Func{`0,``0},System.Func{`1,``0},System.Func{`2,``0},System.Func{`3,``0})" />
        ///     delegate.
        /// </param>
        /// <summary>
        ///     Creates a <see cref="T:Cadenza.Either{T1,T2,T3,T4}" /> instance which
        ///     holds a <typeparamref name="T1" /> value.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:Cadenza.Either{T1,T2,T3,T4}" /> instance which holds a 
        ///     holds a <typeparamref name="T1" /> value.
        /// </returns>
        /// <remarks>
        ///     <para>
        ///      When
        ///      <see cref="M:Cadenza.Either{T1,T2,T3,T4}.Fold``1(System.Func{`0,``0},System.Func{`1,``0},System.Func{`2,``0},System.Func{`3,``0})" />
        ///      is invoked,
        ///      the returned <see cref="T:Cadenza.Either{T1,T2,T3,T4}" /> instance
        ///      will invoke the first delegate
        ///      for conversions.
        ///     </para>
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="value"/> is <see langword="null" />.
        /// </exception>
        public static Either<T1, T2, T3, T4> A(T1 value)
        {
            if ((value == null))
            {
                throw new System.ArgumentNullException("value");
            }
            return new AHandler(value);
        }
        
        /// <param name="value">
        ///     A <typeparamref name="T2" /> containing the value
        ///     to provide to the second
        ///     <see cref="M:Cadenza.Either{T1,T2,T3,T4}.Fold``1(System.Func{`0,``0},System.Func{`1,``0},System.Func{`2,``0},System.Func{`3,``0})" />
        ///     delegate.
        /// </param>
        /// <summary>
        ///     Creates a <see cref="T:Cadenza.Either{T1,T2,T3,T4}" /> instance which
        ///     holds a <typeparamref name="T2" /> value.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:Cadenza.Either{T1,T2,T3,T4}" /> instance which holds a 
        ///     holds a <typeparamref name="T2" /> value.
        /// </returns>
        /// <remarks>
        ///     <para>
        ///      When
        ///      <see cref="M:Cadenza.Either{T1,T2,T3,T4}.Fold``1(System.Func{`0,``0},System.Func{`1,``0},System.Func{`2,``0},System.Func{`3,``0})" />
        ///      is invoked,
        ///      the returned <see cref="T:Cadenza.Either{T1,T2,T3,T4}" /> instance
        ///      will invoke the second delegate
        ///      for conversions.
        ///     </para>
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="value"/> is <see langword="null" />.
        /// </exception>
        public static Either<T1, T2, T3, T4> B(T2 value)
        {
            if ((value == null))
            {
                throw new System.ArgumentNullException("value");
            }
            return new BHandler(value);
        }
        
        /// <param name="value">
        ///     A <typeparamref name="T3" /> containing the value
        ///     to provide to the third
        ///     <see cref="M:Cadenza.Either{T1,T2,T3,T4}.Fold``1(System.Func{`0,``0},System.Func{`1,``0},System.Func{`2,``0},System.Func{`3,``0})" />
        ///     delegate.
        /// </param>
        /// <summary>
        ///     Creates a <see cref="T:Cadenza.Either{T1,T2,T3,T4}" /> instance which
        ///     holds a <typeparamref name="T3" /> value.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:Cadenza.Either{T1,T2,T3,T4}" /> instance which holds a 
        ///     holds a <typeparamref name="T3" /> value.
        /// </returns>
        /// <remarks>
        ///     <para>
        ///      When
        ///      <see cref="M:Cadenza.Either{T1,T2,T3,T4}.Fold``1(System.Func{`0,``0},System.Func{`1,``0},System.Func{`2,``0},System.Func{`3,``0})" />
        ///      is invoked,
        ///      the returned <see cref="T:Cadenza.Either{T1,T2,T3,T4}" /> instance
        ///      will invoke the third delegate
        ///      for conversions.
        ///     </para>
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="value"/> is <see langword="null" />.
        /// </exception>
        public static Either<T1, T2, T3, T4> C(T3 value)
        {
            if ((value == null))
            {
                throw new System.ArgumentNullException("value");
            }
            return new CHandler(value);
        }
        
        /// <param name="value">
        ///     A <typeparamref name="T4" /> containing the value
        ///     to provide to the fourth
        ///     <see cref="M:Cadenza.Either{T1,T2,T3,T4}.Fold``1(System.Func{`0,``0},System.Func{`1,``0},System.Func{`2,``0},System.Func{`3,``0})" />
        ///     delegate.
        /// </param>
        /// <summary>
        ///     Creates a <see cref="T:Cadenza.Either{T1,T2,T3,T4}" /> instance which
        ///     holds a <typeparamref name="T4" /> value.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:Cadenza.Either{T1,T2,T3,T4}" /> instance which holds a 
        ///     holds a <typeparamref name="T4" /> value.
        /// </returns>
        /// <remarks>
        ///     <para>
        ///      When
        ///      <see cref="M:Cadenza.Either{T1,T2,T3,T4}.Fold``1(System.Func{`0,``0},System.Func{`1,``0},System.Func{`2,``0},System.Func{`3,``0})" />
        ///      is invoked,
        ///      the returned <see cref="T:Cadenza.Either{T1,T2,T3,T4}" /> instance
        ///      will invoke the fourth delegate
        ///      for conversions.
        ///     </para>
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        ///   if <paramref name="value"/> is <see langword="null" />.
        /// </exception>
        public static Either<T1, T2, T3, T4> D(T4 value)
        {
            if ((value == null))
            {
                throw new System.ArgumentNullException("value");
            }
            return new DHandler(value);
        }
        
        /// <typeparam name="TResult">
        ///     The type to convert the <see cref="T:Cadenza.Either{T1,T2,T3,T4}" /> to.
        /// </typeparam>
        /// <param name="a">
        ///   A <see cref="T:System.Func{T1,TResult}" /> 
        ///   used if the <see cref="T:Cadenza.Either{T1,T2,T3,T4}" /> stores a 
        ///   <typeparamref name="T1" /> value into a 
        ///   <typeparamref name="TResult" /> value.
        /// </param>
        /// <param name="b">
        ///   A <see cref="T:System.Func{T2,TResult}" /> 
        ///   used if the <see cref="T:Cadenza.Either{T1,T2,T3,T4}" /> stores a 
        ///   <typeparamref name="T2" /> value into a 
        ///   <typeparamref name="TResult" /> value.
        /// </param>
        /// <param name="c">
        ///   A <see cref="T:System.Func{T3,TResult}" /> 
        ///   used if the <see cref="T:Cadenza.Either{T1,T2,T3,T4}" /> stores a 
        ///   <typeparamref name="T3" /> value into a 
        ///   <typeparamref name="TResult" /> value.
        /// </param>
        /// <param name="d">
        ///   A <see cref="T:System.Func{T4,TResult}" /> 
        ///   used if the <see cref="T:Cadenza.Either{T1,T2,T3,T4}" /> stores a 
        ///   <typeparamref name="T4" /> value into a 
        ///   <typeparamref name="TResult" /> value.
        /// </param>
        /// <summary>
        ///     Converts a <see cref="T:Cadenza.Either{T1,T2,T3,T4}" /> into a <typeparamref name="TResult" /> value.
        /// </summary>
        /// <returns>
        ///     A <typeparamref name="TResult" /> as generated by one
        ///     of the conversion delegate parameters.
        /// </returns>
        /// <remarks>
        ///     <para>
        ///      Converts a <see cref="T:Cadenza.Either{T1,T2,T3,T4}" /> into a <typeparamref name="TResult" />
        ///      by invoking one of the provided delegate parameters.
        ///     </para>
        ///     <para>
        ///      The parameter which is invoked is predicated upon the internal position of
        ///      the value held.  For example, if the internal value is in the first position 
        ///      (i.e. <see cref="M:Cadenza.Either{T1,T2,T3,T4}.A(`0)" />
        ///      was used to create the <see cref="T:Cadenza.Either{T1,T2,T3,T4}" /> instance), then
        ///      <paramref name="a" /> (the first delegate parameter) will be invoked to
        ///      convert the <typeparamref name="T1" /> into a 
        ///      <typeparamref name="TResult" />.
        ///     </para>
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <para>
        ///     <paramref name="a" /> is <see langword="null" />.
        ///   </para>
        ///   <para>-or-</para>
        ///   <para>
        ///     <paramref name="b" /> is <see langword="null" />.
        ///   </para>
        ///   <para>-or-</para>
        ///   <para>
        ///     <paramref name="c" /> is <see langword="null" />.
        ///   </para>
        ///   <para>-or-</para>
        ///   <para>
        ///     <paramref name="d" /> is <see langword="null" />.
        ///   </para>
        /// </exception>
        public abstract TResult Fold<TResult>(System.Func<T1, TResult> a, System.Func<T2, TResult> b, System.Func<T3, TResult> c, System.Func<T4, TResult> d)
        ;
        
        private static void CheckFolders<TResult>(System.Func<T1, TResult> a, System.Func<T2, TResult> b, System.Func<T3, TResult> c, System.Func<T4, TResult> d)
        
        {
            if ((a == null))
            {
                throw new System.ArgumentNullException("a");
            }
            if ((b == null))
            {
                throw new System.ArgumentNullException("b");
            }
            if ((c == null))
            {
                throw new System.ArgumentNullException("c");
            }
            if ((d == null))
            {
                throw new System.ArgumentNullException("d");
            }
        }
        
        /// <param name="obj">
        ///     A <see cref="T:System.Object"/> to compare this instance against.
        /// </param>
        /// <summary>
        ///     Determines whether the current instance and the specified object have the same value.
        /// </summary>
        /// <returns>
        ///     <para>
        ///      <see langword="true"/> if <paramref name="obj"/> is a 
        ///      <see cref="T:Cadenza.Either{T1,T2,T3,T4}" /> and each member of <paramref name="obj"/>
        ///      and the current instance have the same value (according to
        ///      <see cref="M:System.Object.Equals(System.Object)"/>); otherwise
        ///      <see langword="false"/> is returned.
        ///     </para>
        /// </returns>
        /// <remarks>
        ///     <para>
        ///      This method checks for value equality 
        ///      (<see cref="M:System.Object.Equals(System.Object)"/>), as defined by each
        ///      value type.
        ///     </para>
        ///     <para>
        ///      <block subset="none" type="note">
        ///       This method overrides <see cref="M:System.Object.Equals(System.Object)"/>.
        ///      </block>
        ///     </para>
        /// </remarks>
        public abstract override System.Boolean Equals(object obj);
        
        /// <param name="obj">
        ///   A <see cref="T:Cadenza.Either{T1,T2,T3,T4}" />to compare this instance against.
        /// </param>
        /// <summary>
        ///     Determines whether the current instance and the specified <see cref="T:Cadenza.Either{T1,T2,T3,T4}" /> have the same value.
        /// </summary>
        /// <returns>
        ///     <para>
        ///      <see langword="true"/> if each member of <paramref name="obj"/>
        ///      and the current instance have the same value (according to
        ///      <see cref="M:System.Object.Equals(System.Object)"/>); otherwise
        ///      <see langword="false"/> is returned.
        ///     </para>
        /// </returns>
        /// <remarks>
        ///     <para>
        ///      This method checks for value equality
        ///      (<see cref="M:System.Object.Equals(System.Object)"/>), as defined by each
        ///      value type.
        ///     </para>
        /// </remarks>
        public abstract bool Equals(Either<T1, T2, T3, T4> obj);
        
        /// <summary>
        ///     Generates a hash code for the current instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.Int32"/> containing the hash code for this instance.
        /// </returns>
        /// <remarks>
        ///     <para>
        ///      <block subset="none" type="note">
        ///       This method overrides <see cref="M:System.Object.GetHashCode"/>.
        ///      </block>
        ///     </para>
        /// </remarks>
        public abstract override System.Int32 GetHashCode();
        
        private class AHandler : Either<T1, T2, T3, T4>
        {
            
            private T1 _value;
            
            public AHandler(T1 value)
            {
                _value = value;
            }
            
            public override int GetHashCode()
            {
                return _value.GetHashCode();
            }
            
            public override bool Equals(object obj)
            {
                AHandler o = obj as AHandler;
                if ((o == null))
                {
                    return false;
                }
                return this.Equals(o);
            }
            
            public override bool Equals(Either<T1, T2, T3, T4> obj)
            {
                AHandler o = obj as AHandler;
                if ((o == null))
                {
                    return false;
                }
                return System.Collections.Generic.EqualityComparer<T1>.Default.Equals(this._value, o._value);
            }
            
            public override TResult Fold<TResult>(System.Func<T1, TResult> a, System.Func<T2, TResult> b, System.Func<T3, TResult> c, System.Func<T4, TResult> d)
            
            {
                CheckFolders(a, b, c, d);
                return a(_value);
            }
        }
        
        private class BHandler : Either<T1, T2, T3, T4>
        {
            
            private T2 _value;
            
            public BHandler(T2 value)
            {
                _value = value;
            }
            
            public override int GetHashCode()
            {
                return _value.GetHashCode();
            }
            
            public override bool Equals(object obj)
            {
                BHandler o = obj as BHandler;
                if ((o == null))
                {
                    return false;
                }
                return this.Equals(o);
            }
            
            public override bool Equals(Either<T1, T2, T3, T4> obj)
            {
                BHandler o = obj as BHandler;
                if ((o == null))
                {
                    return false;
                }
                return System.Collections.Generic.EqualityComparer<T2>.Default.Equals(this._value, o._value);
            }
            
            public override TResult Fold<TResult>(System.Func<T1, TResult> a, System.Func<T2, TResult> b, System.Func<T3, TResult> c, System.Func<T4, TResult> d)
            
            {
                CheckFolders(a, b, c, d);
                return b(_value);
            }
        }
        
        private class CHandler : Either<T1, T2, T3, T4>
        {
            
            private T3 _value;
            
            public CHandler(T3 value)
            {
                _value = value;
            }
            
            public override int GetHashCode()
            {
                return _value.GetHashCode();
            }
            
            public override bool Equals(object obj)
            {
                CHandler o = obj as CHandler;
                if ((o == null))
                {
                    return false;
                }
                return this.Equals(o);
            }
            
            public override bool Equals(Either<T1, T2, T3, T4> obj)
            {
                CHandler o = obj as CHandler;
                if ((o == null))
                {
                    return false;
                }
                return System.Collections.Generic.EqualityComparer<T3>.Default.Equals(this._value, o._value);
            }
            
            public override TResult Fold<TResult>(System.Func<T1, TResult> a, System.Func<T2, TResult> b, System.Func<T3, TResult> c, System.Func<T4, TResult> d)
            
            {
                CheckFolders(a, b, c, d);
                return c(_value);
            }
        }
        
        private class DHandler : Either<T1, T2, T3, T4>
        {
            
            private T4 _value;
            
            public DHandler(T4 value)
            {
                _value = value;
            }
            
            public override int GetHashCode()
            {
                return _value.GetHashCode();
            }
            
            public override bool Equals(object obj)
            {
                DHandler o = obj as DHandler;
                if ((o == null))
                {
                    return false;
                }
                return this.Equals(o);
            }
            
            public override bool Equals(Either<T1, T2, T3, T4> obj)
            {
                DHandler o = obj as DHandler;
                if ((o == null))
                {
                    return false;
                }
                return System.Collections.Generic.EqualityComparer<T4>.Default.Equals(this._value, o._value);
            }
            
            public override TResult Fold<TResult>(System.Func<T1, TResult> a, System.Func<T2, TResult> b, System.Func<T3, TResult> c, System.Func<T4, TResult> d)
            
            {
                CheckFolders(a, b, c, d);
                return d(_value);
            }
        }
    }
}
