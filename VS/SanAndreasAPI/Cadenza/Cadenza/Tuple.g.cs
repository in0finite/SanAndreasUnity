// 
// Tuples.cs: Tuple types.
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
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text;
    
    
    #region Start Tuple
#if !NET_4_0
    /// <summary>
    ///   Utility methods to create Tuple instances.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///    Provides a set of <see cref="M:Cadenza.Tuple.Create"/> methods so that
    ///    C# type inferencing can easily be used with tuples.  For example,
    ///    instead of:
    ///   </para>
    ///   <code lang="C#">
    ///   Tuple&lt;int, long&gt; a = new Tuple&lt;int, long&gt; (1, 2L);</code>
    ///   <para>You can instead write:</para>
    ///   <code lang="C#">
    ///   Tuple&lt;int, long&gt; b = Tuple.Create (1, 2L);
    ///   // or
    ///   var              c = Tuple.Create (1, 2L);</code>
    /// </remarks>
    public partial class Tuple
    {
        
        /// <summary>
        ///   The maximum number of Tuple types provided.
        /// </summary>
        /// <value>
        ///   The maximum number of Tuple types provided.
        /// </value>
        /// <remarks>
        ///     <para>
        ///      Only tuples up to a certain "arity" are supported; for example,
        ///      a <c>Tuple&lt;T1, T2, ..., T100&gt;</c> isn't supported (and won't
        ///      likely ever be).
        ///     </para>
        ///     <para>
        ///      <see cref="P:Cadenza.Tuple.MaxValues" /> is the maximum number of
        ///      values that the Tuple types support.  If you need to support
        ///      more values, then you need to either live with potential boxing
        ///      and use a e.g. <see cref="T:System.Collections.Generic.List{System.Object}" />
        ///      or nest Tuple instantiations, e.g. 
        ///      <c>Tuple&lt;int, Tuple&lt;int, Tuple&lt;int, Tuple&lt;int, int>>>></c>.
        ///      The problem with such nesting is that it becomes "unnatural" to access 
        ///      later elements -- <c>t._2._2._2._2</c> to access the fifth value for
        ///      the previous example.
        ///     </para>
        /// </remarks>
        public static int MaxValues
        {
            get
            {
                return 4;
            }
        }
        
        /// <typeparam name="T">
        ///   The first <see cref="T:Cadenza.Tuple{T}" /> value type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="Cadenza.Tuple{T}" />.
        /// </summary>
        /// <param name="item1">
        ///   The first <see cref="T:Cadenza.Tuple{T}" /> value.
        /// </param>
        /// <returns>
        ///   A <see cref="T:Cadenza.Tuple{T}" /> initialized with the parameter values.
        /// </returns>
        /// <seealso cref="C:Cadenza.Tuple{T}(`0)" />
        public static Cadenza.Tuple<T> Create<T>(T item1)
        
        {
            return new Cadenza.Tuple<T>(item1);
        }
        
        /// <typeparam name="T1">
        ///   The first <see cref="T:Cadenza.Tuple{T1,T2}" /> value type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   The second <see cref="T:Cadenza.Tuple{T1,T2}" /> value type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="Cadenza.Tuple{T1,T2}" />.
        /// </summary>
        /// <param name="item1">
        ///   The first <see cref="T:Cadenza.Tuple{T1,T2}" /> value.
        /// </param>
        /// <param name="item2">
        ///   The second <see cref="T:Cadenza.Tuple{T1,T2}" /> value.
        /// </param>
        /// <returns>
        ///   A <see cref="T:Cadenza.Tuple{T1,T2}" /> initialized with the parameter values.
        /// </returns>
        /// <seealso cref="C:Cadenza.Tuple{T1,T2}(`0,`1)" />
        public static Cadenza.Tuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2)
        
        
        {
            return new Cadenza.Tuple<T1, T2>(item1, item2);
        }
        
        /// <typeparam name="T1">
        ///   The first <see cref="T:Cadenza.Tuple{T1,T2,T3}" /> value type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   The second <see cref="T:Cadenza.Tuple{T1,T2,T3}" /> value type.
        /// </typeparam>
        /// <typeparam name="T3">
        ///   The third <see cref="T:Cadenza.Tuple{T1,T2,T3}" /> value type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="Cadenza.Tuple{T1,T2,T3}" />.
        /// </summary>
        /// <param name="item1">
        ///   The first <see cref="T:Cadenza.Tuple{T1,T2,T3}" /> value.
        /// </param>
        /// <param name="item2">
        ///   The second <see cref="T:Cadenza.Tuple{T1,T2,T3}" /> value.
        /// </param>
        /// <param name="item3">
        ///   The third <see cref="T:Cadenza.Tuple{T1,T2,T3}" /> value.
        /// </param>
        /// <returns>
        ///   A <see cref="T:Cadenza.Tuple{T1,T2,T3}" /> initialized with the parameter values.
        /// </returns>
        /// <seealso cref="C:Cadenza.Tuple{T1,T2,T3}(`0,`1,`2)" />
        public static Cadenza.Tuple<T1, T2, T3> Create<T1, T2, T3>(T1 item1, T2 item2, T3 item3)
        
        
        
        {
            return new Cadenza.Tuple<T1, T2, T3>(item1, item2, item3);
        }
        
        /// <typeparam name="T1">
        ///   The first <see cref="T:Cadenza.Tuple{T1,T2,T3,T4}" /> value type.
        /// </typeparam>
        /// <typeparam name="T2">
        ///   The second <see cref="T:Cadenza.Tuple{T1,T2,T3,T4}" /> value type.
        /// </typeparam>
        /// <typeparam name="T3">
        ///   The third <see cref="T:Cadenza.Tuple{T1,T2,T3,T4}" /> value type.
        /// </typeparam>
        /// <typeparam name="T4">
        ///   The fourth <see cref="T:Cadenza.Tuple{T1,T2,T3,T4}" /> value type.
        /// </typeparam>
        /// <summary>
        ///   Creates a <see cref="Cadenza.Tuple{T1,T2,T3,T4}" />.
        /// </summary>
        /// <param name="item1">
        ///   The first <see cref="T:Cadenza.Tuple{T1,T2,T3,T4}" /> value.
        /// </param>
        /// <param name="item2">
        ///   The second <see cref="T:Cadenza.Tuple{T1,T2,T3,T4}" /> value.
        /// </param>
        /// <param name="item3">
        ///   The third <see cref="T:Cadenza.Tuple{T1,T2,T3,T4}" /> value.
        /// </param>
        /// <param name="item4">
        ///   The fourth <see cref="T:Cadenza.Tuple{T1,T2,T3,T4}" /> value.
        /// </param>
        /// <returns>
        ///   A <see cref="T:Cadenza.Tuple{T1,T2,T3,T4}" /> initialized with the parameter values.
        /// </returns>
        /// <seealso cref="C:Cadenza.Tuple{T1,T2,T3,T4}(`0,`1,`2,`3)" />
        public static Cadenza.Tuple<T1, T2, T3, T4> Create<T1, T2, T3, T4>(T1 item1, T2 item2, T3 item3, T4 item4)
        
        
        
        
        {
            return new Cadenza.Tuple<T1, T2, T3, T4>(item1, item2, item3, item4);
        }
    }
    #region End Tuple
    #endregion
#endif  // !NET_4_0

    #endregion
    
    #region Start Tuple
#if !NET_4_0
    /// <typeparam name="T">
    ///   The first value type.
    /// </typeparam>
    /// <summary>
    ///   A strongly-typed sequence of 1 variously typed values.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///    A <c>Tuple</c> is an immutable, strongly typed sequence of variously
    ///    typed values with each value lacking an otherwise meaningful name aside
    ///    from its position.
    ///   </para>
    /// </remarks>
    public partial class Tuple<T>
    
    {
        
        private T item1;
        
        /// <summary>
        ///   Constructs and initializes a new <see cref="T:Cadenza.Tuple{T}" /> instance.
        /// </summary>
        /// <param name="item1">
        ///   A <typeparamref name="T"/> which is used to initialize the <see cref="P:Cadenza.Tuple{T}.Item1" /> property.
        /// </param>
        /// <remarks>
        ///   <para>
        ///     Constructs and initializes a new <see cref="T:Cadenza.Tuple{T}" /> instance.
        ///   </para>
        /// </remarks>
        public Tuple(T item1)
        {
            this.item1 = item1;
        }
        
        /// <summary>
        ///   The first tuple value.
        /// </summary>
        /// <value>
        ///   A <typeparamref name="T" /> which is the first tuple value.
        /// </value>
        /// <remarks>
        ///   The first tuple value.
        /// </remarks>
        public T Item1
        {
            get
            {
                return this.item1;
            }
        }
        
        /// <param name="obj">
        ///   A <see cref="T:System.Object"/> to compare this instance against.
        /// </param>
        /// <summary>
        ///   Determines whether the current instance and the specified object have the same value.
        /// </summary>
        /// <returns>
        ///   <para>
        ///    <see langword="true"/> if <paramref name="obj"/> is a
        ///    <see cref="T:Cadenza.Tuple{T}" /> and each member of <paramref name="obj"/>
        ///    and the current instance have the same value (according to
        ///    <see cref="M:System.Collections.Generic.EqualityComparer{T}.Equals(`0,`0)" />); otherwise
        ///    <see langword="false"/> is returned.
        ///   </para>
        /// </returns>
        /// <remarks>
        ///   <para>
        ///    This method checks for value equality
        ///    (<see cref="M:System.Collections.Generic.EqualityComparer{T}.Equals(`0,`0)" />), as defined by each
        ///    value type.
        ///   </para>
        ///   <para>
        ///    <block subset="none" type="note">
        ///     This method overrides <see cref="M:System.Object.Equals(System.Object)"/>.
        ///    </block>
        ///   </para>
        /// </remarks>
        public override bool Equals(object obj)
        {
            Tuple<T> o = obj as Tuple<T>;
            if ((o == null))
            {
                return false;
            }
            return System.Collections.Generic.EqualityComparer<T>.Default.Equals(this.item1, o.item1);
        }
        
        /// <summary>
        ///   Generates a hash code for the current instance.
        /// </summary>
        /// <returns>
        ///   A <see cref="T:System.Int32"/> containing the hash code for this instance.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///    <block subset="none" type="note">
        ///     This method overrides <see cref="M:System.Object.GetHashCode"/>.
        ///    </block>
        ///   </para>
        /// </remarks>
        public override int GetHashCode()
        {
            int hc = 0;
            hc ^= Item1.GetHashCode();
            return hc;
        }
        
        /// <summary>
        ///   Returns a <see cref="T:System.String"/> representation of the value of the current instance.
        /// </summary>
        /// <returns>
        ///   A <see cref="T:System.String"/> representation of the value of the current instance.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///    <block subset="none" type="behaviors">
        ///     Returns <c>(</c>, followed by a comma-separated list of the result of
        ///     calling <see cref="M:System.Object.ToString"/> on
        ///   <see cref="P:Cadenza.Tuple{T}.Item1" />, 
        ///     followed by <c>)</c>.
        ///    </block>
        ///   </para>
        /// </remarks>
        public override string ToString()
        {
            return string.Concat("(", this.Item1.ToString(), ")");
        }
    }
    #region End Tuple
    #endregion
#endif  // !NET_4_0

    #endregion
    
    #region Start Tuple
#if !NET_4_0
    /// <typeparam name="T1">
    ///   The first value type.
    /// </typeparam>
    /// <typeparam name="T2">
    ///   The second value type.
    /// </typeparam>
    /// <summary>
    ///   A strongly-typed sequence of 2 variously typed values.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///    A <c>Tuple</c> is an immutable, strongly typed sequence of variously
    ///    typed values with each value lacking an otherwise meaningful name aside
    ///    from its position.
    ///   </para>
    /// </remarks>
    public partial class Tuple<T1, T2>
    
    
    {
        
        private T1 item1;
        
        private T2 item2;
        
        /// <summary>
        ///   Constructs and initializes a new <see cref="T:Cadenza.Tuple{T1,T2}" /> instance.
        /// </summary>
        /// <param name="item1">
        ///   A <typeparamref name="T1"/> which is used to initialize the <see cref="P:Cadenza.Tuple{T1,T2}.Item1" /> property.
        /// </param>
        /// <param name="item2">
        ///   A <typeparamref name="T2"/> which is used to initialize the <see cref="P:Cadenza.Tuple{T1,T2}.Item2" /> property.
        /// </param>
        /// <remarks>
        ///   <para>
        ///     Constructs and initializes a new <see cref="T:Cadenza.Tuple{T1,T2}" /> instance.
        ///   </para>
        /// </remarks>
        public Tuple(T1 item1, T2 item2)
        {
            this.item1 = item1;
            this.item2 = item2;
        }
        
        /// <summary>
        ///   The first tuple value.
        /// </summary>
        /// <value>
        ///   A <typeparamref name="T1" /> which is the first tuple value.
        /// </value>
        /// <remarks>
        ///   The first tuple value.
        /// </remarks>
        public T1 Item1
        {
            get
            {
                return this.item1;
            }
        }
        
        /// <summary>
        ///   The second tuple value.
        /// </summary>
        /// <value>
        ///   A <typeparamref name="T2" /> which is the second tuple value.
        /// </value>
        /// <remarks>
        ///   The second tuple value.
        /// </remarks>
        public T2 Item2
        {
            get
            {
                return this.item2;
            }
        }
        
        /// <param name="obj">
        ///   A <see cref="T:System.Object"/> to compare this instance against.
        /// </param>
        /// <summary>
        ///   Determines whether the current instance and the specified object have the same value.
        /// </summary>
        /// <returns>
        ///   <para>
        ///    <see langword="true"/> if <paramref name="obj"/> is a
        ///    <see cref="T:Cadenza.Tuple{T1,T2}" /> and each member of <paramref name="obj"/>
        ///    and the current instance have the same value (according to
        ///    <see cref="M:System.Collections.Generic.EqualityComparer{T}.Equals(`0,`0)" />); otherwise
        ///    <see langword="false"/> is returned.
        ///   </para>
        /// </returns>
        /// <remarks>
        ///   <para>
        ///    This method checks for value equality
        ///    (<see cref="M:System.Collections.Generic.EqualityComparer{T}.Equals(`0,`0)" />), as defined by each
        ///    value type.
        ///   </para>
        ///   <para>
        ///    <block subset="none" type="note">
        ///     This method overrides <see cref="M:System.Object.Equals(System.Object)"/>.
        ///    </block>
        ///   </para>
        /// </remarks>
        public override bool Equals(object obj)
        {
            Tuple<T1,T2> o = obj as Tuple<T1,T2>;
            if ((o == null))
            {
                return false;
            }
            return (System.Collections.Generic.EqualityComparer<T1>.Default.Equals(this.item1, o.item1) && System.Collections.Generic.EqualityComparer<T2>.Default.Equals(this.item2, o.item2));
        }
        
        /// <summary>
        ///   Generates a hash code for the current instance.
        /// </summary>
        /// <returns>
        ///   A <see cref="T:System.Int32"/> containing the hash code for this instance.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///    <block subset="none" type="note">
        ///     This method overrides <see cref="M:System.Object.GetHashCode"/>.
        ///    </block>
        ///   </para>
        /// </remarks>
        public override int GetHashCode()
        {
            int hc = 0;
            hc ^= Item1.GetHashCode();
            hc ^= Item2.GetHashCode();
            return hc;
        }
        
        /// <summary>
        ///   Returns a <see cref="T:System.String"/> representation of the value of the current instance.
        /// </summary>
        /// <returns>
        ///   A <see cref="T:System.String"/> representation of the value of the current instance.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///    <block subset="none" type="behaviors">
        ///     Returns <c>(</c>, followed by a comma-separated list of the result of
        ///     calling <see cref="M:System.Object.ToString"/> on
        ///   <see cref="P:Cadenza.Tuple{T1,T2}.Item1" />, 
        ///   <see cref="P:Cadenza.Tuple{T1,T2}.Item2" />, 
        ///     followed by <c>)</c>.
        ///    </block>
        ///   </para>
        /// </remarks>
        public override string ToString()
        {
            return string.Concat("(", this.Item1.ToString(), ", ", this.Item2.ToString(), ")");
        }
    }
    #region End Tuple
    #endregion
#endif  // !NET_4_0

    #endregion
    
    #region Start Tuple
#if !NET_4_0
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
    ///   A strongly-typed sequence of 3 variously typed values.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///    A <c>Tuple</c> is an immutable, strongly typed sequence of variously
    ///    typed values with each value lacking an otherwise meaningful name aside
    ///    from its position.
    ///   </para>
    /// </remarks>
    public partial class Tuple<T1, T2, T3>
    
    
    
    {
        
        private T1 item1;
        
        private T2 item2;
        
        private T3 item3;
        
        /// <summary>
        ///   Constructs and initializes a new <see cref="T:Cadenza.Tuple{T1,T2,T3}" /> instance.
        /// </summary>
        /// <param name="item1">
        ///   A <typeparamref name="T1"/> which is used to initialize the <see cref="P:Cadenza.Tuple{T1,T2,T3}.Item1" /> property.
        /// </param>
        /// <param name="item2">
        ///   A <typeparamref name="T2"/> which is used to initialize the <see cref="P:Cadenza.Tuple{T1,T2,T3}.Item2" /> property.
        /// </param>
        /// <param name="item3">
        ///   A <typeparamref name="T3"/> which is used to initialize the <see cref="P:Cadenza.Tuple{T1,T2,T3}.Item3" /> property.
        /// </param>
        /// <remarks>
        ///   <para>
        ///     Constructs and initializes a new <see cref="T:Cadenza.Tuple{T1,T2,T3}" /> instance.
        ///   </para>
        /// </remarks>
        public Tuple(T1 item1, T2 item2, T3 item3)
        {
            this.item1 = item1;
            this.item2 = item2;
            this.item3 = item3;
        }
        
        /// <summary>
        ///   The first tuple value.
        /// </summary>
        /// <value>
        ///   A <typeparamref name="T1" /> which is the first tuple value.
        /// </value>
        /// <remarks>
        ///   The first tuple value.
        /// </remarks>
        public T1 Item1
        {
            get
            {
                return this.item1;
            }
        }
        
        /// <summary>
        ///   The second tuple value.
        /// </summary>
        /// <value>
        ///   A <typeparamref name="T2" /> which is the second tuple value.
        /// </value>
        /// <remarks>
        ///   The second tuple value.
        /// </remarks>
        public T2 Item2
        {
            get
            {
                return this.item2;
            }
        }
        
        /// <summary>
        ///   The third tuple value.
        /// </summary>
        /// <value>
        ///   A <typeparamref name="T3" /> which is the third tuple value.
        /// </value>
        /// <remarks>
        ///   The third tuple value.
        /// </remarks>
        public T3 Item3
        {
            get
            {
                return this.item3;
            }
        }
        
        /// <param name="obj">
        ///   A <see cref="T:System.Object"/> to compare this instance against.
        /// </param>
        /// <summary>
        ///   Determines whether the current instance and the specified object have the same value.
        /// </summary>
        /// <returns>
        ///   <para>
        ///    <see langword="true"/> if <paramref name="obj"/> is a
        ///    <see cref="T:Cadenza.Tuple{T1,T2,T3}" /> and each member of <paramref name="obj"/>
        ///    and the current instance have the same value (according to
        ///    <see cref="M:System.Collections.Generic.EqualityComparer{T}.Equals(`0,`0)" />); otherwise
        ///    <see langword="false"/> is returned.
        ///   </para>
        /// </returns>
        /// <remarks>
        ///   <para>
        ///    This method checks for value equality
        ///    (<see cref="M:System.Collections.Generic.EqualityComparer{T}.Equals(`0,`0)" />), as defined by each
        ///    value type.
        ///   </para>
        ///   <para>
        ///    <block subset="none" type="note">
        ///     This method overrides <see cref="M:System.Object.Equals(System.Object)"/>.
        ///    </block>
        ///   </para>
        /// </remarks>
        public override bool Equals(object obj)
        {
            Tuple<T1,T2,T3> o = obj as Tuple<T1,T2,T3>;
            if ((o == null))
            {
                return false;
            }
            return ((System.Collections.Generic.EqualityComparer<T1>.Default.Equals(this.item1, o.item1) && System.Collections.Generic.EqualityComparer<T2>.Default.Equals(this.item2, o.item2)) && System.Collections.Generic.EqualityComparer<T3>.Default.Equals(this.item3, o.item3));
        }
        
        /// <summary>
        ///   Generates a hash code for the current instance.
        /// </summary>
        /// <returns>
        ///   A <see cref="T:System.Int32"/> containing the hash code for this instance.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///    <block subset="none" type="note">
        ///     This method overrides <see cref="M:System.Object.GetHashCode"/>.
        ///    </block>
        ///   </para>
        /// </remarks>
        public override int GetHashCode()
        {
            int hc = 0;
            hc ^= Item1.GetHashCode();
            hc ^= Item2.GetHashCode();
            hc ^= Item3.GetHashCode();
            return hc;
        }
        
        /// <summary>
        ///   Returns a <see cref="T:System.String"/> representation of the value of the current instance.
        /// </summary>
        /// <returns>
        ///   A <see cref="T:System.String"/> representation of the value of the current instance.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///    <block subset="none" type="behaviors">
        ///     Returns <c>(</c>, followed by a comma-separated list of the result of
        ///     calling <see cref="M:System.Object.ToString"/> on
        ///   <see cref="P:Cadenza.Tuple{T1,T2,T3}.Item1" />, 
        ///   <see cref="P:Cadenza.Tuple{T1,T2,T3}.Item2" />, 
        ///   <see cref="P:Cadenza.Tuple{T1,T2,T3}.Item3" />, 
        ///     followed by <c>)</c>.
        ///    </block>
        ///   </para>
        /// </remarks>
        public override string ToString()
        {
            return string.Concat("(", this.Item1.ToString(), ", ", this.Item2.ToString(), ", ", this.Item3.ToString(), ")");
        }
    }
    #region End Tuple
    #endregion
#endif  // !NET_4_0

    #endregion
    
    #region Start Tuple
#if !NET_4_0
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
    ///   A strongly-typed sequence of 4 variously typed values.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///    A <c>Tuple</c> is an immutable, strongly typed sequence of variously
    ///    typed values with each value lacking an otherwise meaningful name aside
    ///    from its position.
    ///   </para>
    /// </remarks>
    public partial class Tuple<T1, T2, T3, T4>
    
    
    
    
    {
        
        private T1 item1;
        
        private T2 item2;
        
        private T3 item3;
        
        private T4 item4;
        
        /// <summary>
        ///   Constructs and initializes a new <see cref="T:Cadenza.Tuple{T1,T2,T3,T4}" /> instance.
        /// </summary>
        /// <param name="item1">
        ///   A <typeparamref name="T1"/> which is used to initialize the <see cref="P:Cadenza.Tuple{T1,T2,T3,T4}.Item1" /> property.
        /// </param>
        /// <param name="item2">
        ///   A <typeparamref name="T2"/> which is used to initialize the <see cref="P:Cadenza.Tuple{T1,T2,T3,T4}.Item2" /> property.
        /// </param>
        /// <param name="item3">
        ///   A <typeparamref name="T3"/> which is used to initialize the <see cref="P:Cadenza.Tuple{T1,T2,T3,T4}.Item3" /> property.
        /// </param>
        /// <param name="item4">
        ///   A <typeparamref name="T4"/> which is used to initialize the <see cref="P:Cadenza.Tuple{T1,T2,T3,T4}.Item4" /> property.
        /// </param>
        /// <remarks>
        ///   <para>
        ///     Constructs and initializes a new <see cref="T:Cadenza.Tuple{T1,T2,T3,T4}" /> instance.
        ///   </para>
        /// </remarks>
        public Tuple(T1 item1, T2 item2, T3 item3, T4 item4)
        {
            this.item1 = item1;
            this.item2 = item2;
            this.item3 = item3;
            this.item4 = item4;
        }
        
        /// <summary>
        ///   The first tuple value.
        /// </summary>
        /// <value>
        ///   A <typeparamref name="T1" /> which is the first tuple value.
        /// </value>
        /// <remarks>
        ///   The first tuple value.
        /// </remarks>
        public T1 Item1
        {
            get
            {
                return this.item1;
            }
        }
        
        /// <summary>
        ///   The second tuple value.
        /// </summary>
        /// <value>
        ///   A <typeparamref name="T2" /> which is the second tuple value.
        /// </value>
        /// <remarks>
        ///   The second tuple value.
        /// </remarks>
        public T2 Item2
        {
            get
            {
                return this.item2;
            }
        }
        
        /// <summary>
        ///   The third tuple value.
        /// </summary>
        /// <value>
        ///   A <typeparamref name="T3" /> which is the third tuple value.
        /// </value>
        /// <remarks>
        ///   The third tuple value.
        /// </remarks>
        public T3 Item3
        {
            get
            {
                return this.item3;
            }
        }
        
        /// <summary>
        ///   The fourth tuple value.
        /// </summary>
        /// <value>
        ///   A <typeparamref name="T4" /> which is the fourth tuple value.
        /// </value>
        /// <remarks>
        ///   The fourth tuple value.
        /// </remarks>
        public T4 Item4
        {
            get
            {
                return this.item4;
            }
        }
        
        /// <param name="obj">
        ///   A <see cref="T:System.Object"/> to compare this instance against.
        /// </param>
        /// <summary>
        ///   Determines whether the current instance and the specified object have the same value.
        /// </summary>
        /// <returns>
        ///   <para>
        ///    <see langword="true"/> if <paramref name="obj"/> is a
        ///    <see cref="T:Cadenza.Tuple{T1,T2,T3,T4}" /> and each member of <paramref name="obj"/>
        ///    and the current instance have the same value (according to
        ///    <see cref="M:System.Collections.Generic.EqualityComparer{T}.Equals(`0,`0)" />); otherwise
        ///    <see langword="false"/> is returned.
        ///   </para>
        /// </returns>
        /// <remarks>
        ///   <para>
        ///    This method checks for value equality
        ///    (<see cref="M:System.Collections.Generic.EqualityComparer{T}.Equals(`0,`0)" />), as defined by each
        ///    value type.
        ///   </para>
        ///   <para>
        ///    <block subset="none" type="note">
        ///     This method overrides <see cref="M:System.Object.Equals(System.Object)"/>.
        ///    </block>
        ///   </para>
        /// </remarks>
        public override bool Equals(object obj)
        {
            Tuple<T1,T2,T3,T4> o = obj as Tuple<T1,T2,T3,T4>;
            if ((o == null))
            {
                return false;
            }
            return (((System.Collections.Generic.EqualityComparer<T1>.Default.Equals(this.item1, o.item1) && System.Collections.Generic.EqualityComparer<T2>.Default.Equals(this.item2, o.item2)) && System.Collections.Generic.EqualityComparer<T3>.Default.Equals(this.item3, o.item3)) && System.Collections.Generic.EqualityComparer<T4>.Default.Equals(this.item4, o.item4));
        }
        
        /// <summary>
        ///   Generates a hash code for the current instance.
        /// </summary>
        /// <returns>
        ///   A <see cref="T:System.Int32"/> containing the hash code for this instance.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///    <block subset="none" type="note">
        ///     This method overrides <see cref="M:System.Object.GetHashCode"/>.
        ///    </block>
        ///   </para>
        /// </remarks>
        public override int GetHashCode()
        {
            int hc = 0;
            hc ^= Item1.GetHashCode();
            hc ^= Item2.GetHashCode();
            hc ^= Item3.GetHashCode();
            hc ^= Item4.GetHashCode();
            return hc;
        }
        
        /// <summary>
        ///   Returns a <see cref="T:System.String"/> representation of the value of the current instance.
        /// </summary>
        /// <returns>
        ///   A <see cref="T:System.String"/> representation of the value of the current instance.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///    <block subset="none" type="behaviors">
        ///     Returns <c>(</c>, followed by a comma-separated list of the result of
        ///     calling <see cref="M:System.Object.ToString"/> on
        ///   <see cref="P:Cadenza.Tuple{T1,T2,T3,T4}.Item1" />, 
        ///   <see cref="P:Cadenza.Tuple{T1,T2,T3,T4}.Item2" />, 
        ///   <see cref="P:Cadenza.Tuple{T1,T2,T3,T4}.Item3" />, 
        ///   <see cref="P:Cadenza.Tuple{T1,T2,T3,T4}.Item4" />, 
        ///     followed by <c>)</c>.
        ///    </block>
        ///   </para>
        /// </remarks>
        public override string ToString()
        {
            return string.Concat("(", this.Item1.ToString(), ", ", this.Item2.ToString(), ", ", this.Item3.ToString(), ", ", this.Item4.ToString(), ")");
        }
    }
    #region End Tuple
    #endregion
#endif  // !NET_4_0

    #endregion
}
