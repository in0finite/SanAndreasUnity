//
// Int64Test.cs
//
// Author:
//   Jonathan Pryor
//
// Copyright (c) 2008 Novell, Inc. (http://www.novell.com)
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
using System.IO;
using System.Linq;
using System.Reflection;

using NUnit.Framework;

using Cadenza;
using Cadenza.Tests;

namespace Cadenza.Numerics.Tests {

	[TestFixture]
	public class MathTests : BaseRocksFixture {

		// "Unsupported", i.e. types which don't support add/subtract/etc.
		// We still allow these to be created; the individual methods will instead throw.
		[Test]
		public void Default_DoesNotThrowsWithUnsupportedTypes ()
		{
			Math<string> m = Math<string>.Default;
			Ignore (m);
		}

		// Math<T>.Default uses ExpressionMath<T> as a fallback, which is in Cadenza.Core.
		// If Cadenza.Core can't be found (e.g. MonoTouch, bad packaging), then
		// Math<T>.Default should throw a NotSupportedException.
		[Test]
		public void Default_ThrowsIfCadenzaCoreCannotBeFound ()
		{
			try {
				var ads = new AppDomainSetup () {
					ApplicationBase = Path.GetDirectoryName (typeof (Math<>).Assembly.Location),
					DisallowApplicationBaseProbing = true,
				};
				AppDomain d = AppDomain.CreateDomain ("Test Math<string>.Default", null, ads);
				Assembly[] assemblies = new[]{typeof (Math<>), typeof (MathTests)}.Select (type => {
					using (var f = File.OpenRead (type.Assembly.Location)) {
						byte[] lib = new byte [f.Length];
						f.Read (lib, 0, lib.Length);
						return d.Load (lib);
					}
				}).ToArray ();
				try {
					var r = (GetStringMathOpsRunner) assemblies [1].CreateInstance (typeof (GetStringMathOpsRunner).FullName);
					r.Run ();
				}
				finally {
					AppDomain.Unload (d);
				}
			}
			catch (NotSupportedException) {
				// success!
			}
			catch (Exception e) {
				Assert.Fail ("Generated exception: " + e);
			}
		}

		class GetStringMathOpsRunner : MarshalByRefObject {
			public void Run ()
			{
				var m = Math<string>.Default;
				Ignore (m);
			}
		}

		[Test]
		public void SetDefault ()
		{
			var d = Math<SimpleNumber>.Default;
			try {
				Math<SimpleNumber>.SetDefault (new SimpleNumberMath ());
				Assert.IsFalse (object.ReferenceEquals (d, Math<SimpleNumber>.Default));

				Math<SimpleNumber>.SetDefault (null);
				Assert.AreEqual (d.GetType ().FullName, Math<SimpleNumber>.Default.GetType ().FullName);
			}
			finally {
				Math<SimpleNumber>.SetDefault (d);
			}
		}
	}

	//
	// Builtin type tests
	//

	[TestFixture]
	public class DecimalMathTests : MathContract<decimal> {
	}

	[TestFixture]
	public class DoubleMathTests : MathContract<double> {
	}

	[TestFixture]
	public class SingleMathTests : MathContract<float> {
	}

	[TestFixture]
	public class ByteMathTests : MathContract<byte> {
	}

	[TestFixture]
	public class Int16MathTests : MathContract<short> {
	}

	[TestFixture]
	public class Int32MathTests : MathContract<int> {
	}

	[TestFixture]
	public class Int64MathTests : MathContract<long> {
	}

	[TestFixture]
	public class SByteMathTests : MathContract<sbyte> {
	}

	[TestFixture]
	public class UInt16MathTests : MathContract<ushort> {
	}

	[TestFixture]
	public class UInt32MathTests : MathContract<uint> {
	}

	[TestFixture]
	public class UInt64MathTests : MathContract<ulong> {
	}

	//
	// To implement Math<T> support for a new type, you obviously need a type.
	// We also require the ability to create instances of that type from an int.
	// There are three ways to do this:
	//  1.  Override Math<T>.FromInt32() and Math<T>.ToInt32().
	//  2.  Provide a System.CodeDom.TypeConverter for the type and annotate the
	//      type with [TypeConverter]
	//  3.  Implement System.IConvertible, and override Math<T>.FromInt32().
	//      This is necessary because System.Int32 doesn't support conversion to
	//      to arbitrary types.
	//
	// The type must also implement IComparer<T> and IEquatable<T>.
	//
	// SimpleNumber + SimpleNumberMath show approach (1).
	// SimpleNumber2 + SimpleNumber2Converter + SimpleNumber2Math show approach (2).
	// SimpleNumber3 + SimpleNumber3Math show approach (3).
	//

	public class SimpleNumber : IComparable<SimpleNumber>, IEquatable<SimpleNumber>
	{
		public SimpleNumber (int value)
		{
			Value = value;
		}

		public int CompareTo (SimpleNumber other)
		{
			if (other == null)
				return -1;
			return Value.CompareTo (other.Value);
		}

		public override int GetHashCode ()
		{
			return Value.GetHashCode ();
		}

		public override bool Equals (object obj)
		{
			return Equals (obj as SimpleNumber);
		}

		public bool Equals (SimpleNumber other)
		{
			if (other == null)
				return false;
			return Value.Equals (other.Value);
		}

		public int Value {get; private set;}

		public override string ToString ()
		{
			return "(" + GetType ().Name + " " + Value.ToString () + ")";
		}
	}

	// minimal implementation
	public class SimpleNumberMath : Math<SimpleNumber> {
		public override SimpleNumber FromInt32 (int value)
		{
			return new SimpleNumber (value);
		}

		public override int ToInt32 (SimpleNumber value)
		{
			return value.Value;
		}

		public override SimpleNumber FromIConvertible (IConvertible c)
		{
			return new SimpleNumber (c.ToInt32 (null));
		}

		public override IConvertible ToIConvertible (SimpleNumber v)
		{
			return v.Value;
		}

		public override SimpleNumber Add (SimpleNumber x, SimpleNumber y)
		{
			return new SimpleNumber (x.Value + y.Value);
		}

		public override SimpleNumber Subtract (SimpleNumber x, SimpleNumber y)
		{
			return new SimpleNumber (x.Value - y.Value);
		}

		public override SimpleNumber Multiply (SimpleNumber x, SimpleNumber y)
		{
			return new SimpleNumber (x.Value * y.Value);
		}

		public override SimpleNumber QuotientRemainder (SimpleNumber x, SimpleNumber y, out SimpleNumber remainder)
		{
			remainder = new SimpleNumber (x.Value % y.Value);
			return new SimpleNumber (x.Value / y.Value);
		}
	}

	[TestFixture]
	public class SimpleNumberMathTests : MathContract<SimpleNumber> {

		static SimpleNumberMathTests ()
		{
			Math<SimpleNumber>.SetDefault (new SimpleNumberMath ());
		}
	}

	[TypeConverter (typeof (SimpleNumber2Converter))]
	public class SimpleNumber2 : SimpleNumber, IComparable<SimpleNumber2>, IEquatable<SimpleNumber2>
	{
		public SimpleNumber2 (int value)
			: base (value)
		{
		}

		public int CompareTo (SimpleNumber2 x)
		{
			return base.CompareTo (x);
		}

		public bool Equals (SimpleNumber2 other)
		{
			return base.Equals (other);
		}
	}

	public class SimpleNumber2Converter : TypeConverter {

		KeyValuePair<Type, Func<object, SimpleNumber2>>[] convertFrom = new KeyValuePair<Type, Func<object, SimpleNumber2>>[]{
			new KeyValuePair<Type, Func<object, SimpleNumber2>>(typeof (int), v => {
				int? value = v as int?;
				if (value.HasValue)
					return new SimpleNumber2 (value.Value);
				return null;
			}),
			new KeyValuePair<Type, Func<object, SimpleNumber2>>(typeof (int), v => {
				string value = v as string;
				if (value != null)
					return new SimpleNumber2 (int.Parse (value));
				return null;
			}),
		};

		KeyValuePair<Type, Func<SimpleNumber2, object>>[] convertTo = new KeyValuePair<Type, Func<SimpleNumber2, object>>[]{
			new KeyValuePair<Type, Func<SimpleNumber2, object>>(typeof (int), v => v.Value),
			new KeyValuePair<Type, Func<SimpleNumber2, object>>(typeof (int), v => v.Value.ToString ()),
		};

		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
		{
			if (convertFrom.Any (c => c.Key == sourceType))
				return true;
			return base.CanConvertFrom (context, sourceType);
		}

		public override object ConvertFrom (ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			foreach (var c in convertFrom) {
				var v = c.Value (value);
				if (v != null)
					return v;
			}
			return base.ConvertFrom (context, culture, value);
		}

		public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType)
		{
			if (convertTo.Any (c => c.Key == destinationType))
				return true;
			return base.CanConvertTo (context, destinationType);
		}

		public override object ConvertTo (ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
		{
			var n = (SimpleNumber2) value;
			foreach (var c in convertTo) {
				var v = c.Value (n);
				if (v != null)
					return v;
			}
			return base.ConvertTo (context, culture, value, destinationType);
		}
	}

	public class SimpleNumber2Math : Math<SimpleNumber2> {
		public override SimpleNumber2 Add (SimpleNumber2 x, SimpleNumber2 y)
		{
			return new SimpleNumber2 (x.Value + y.Value);
		}

		public override SimpleNumber2 Subtract (SimpleNumber2 x, SimpleNumber2 y)
		{
			return new SimpleNumber2 (x.Value - y.Value);
		}

		public override SimpleNumber2 Multiply (SimpleNumber2 x, SimpleNumber2 y)
		{
			return new SimpleNumber2 (x.Value * y.Value);
		}

		public override SimpleNumber2 QuotientRemainder (SimpleNumber2 x, SimpleNumber2 y, out SimpleNumber2 remainder)
		{
			remainder = new SimpleNumber2 (x.Value % y.Value);
			return new SimpleNumber2 (x.Value / y.Value);
		}
	}

	[TestFixture]
	public class SimpleNumber2MathTests : MathContract<SimpleNumber2> {

		static SimpleNumber2MathTests ()
		{
			Math<SimpleNumber2>.SetDefault (new SimpleNumber2Math ());
		}
	}

	public class SimpleNumber3 : SimpleNumber, IComparable<SimpleNumber3>, IEquatable<SimpleNumber3>, IConvertible
	{
		public SimpleNumber3 (int value)
			: base (value)
		{
		}

		public int CompareTo (SimpleNumber3 x)
		{
			return base.CompareTo (x);
		}

		public bool Equals (SimpleNumber3 other)
		{
			return base.Equals (other);
		}

		TypeCode IConvertible.GetTypeCode ()
			{return TypeCode.Object;}
		bool IConvertible.ToBoolean (IFormatProvider p)
			{return ((IConvertible) Value).ToBoolean (p);}
		byte IConvertible.ToByte (IFormatProvider p)
			{return ((IConvertible) Value).ToByte (p);}
		char IConvertible.ToChar (IFormatProvider p)
			{return ((IConvertible) Value).ToChar (p);}
		DateTime IConvertible.ToDateTime (IFormatProvider p)
			{return ((IConvertible) Value).ToDateTime (p);}
		decimal IConvertible.ToDecimal (IFormatProvider p)
			{return ((IConvertible) Value).ToDecimal (p);}
		double IConvertible.ToDouble (IFormatProvider p)
			{return ((IConvertible) Value).ToDouble (p);}
		short IConvertible.ToInt16 (IFormatProvider p)
			{return ((IConvertible) Value).ToInt16 (p);}
		int IConvertible.ToInt32 (IFormatProvider p)
			{return ((IConvertible) Value).ToInt32 (p);}
		long IConvertible.ToInt64 (IFormatProvider p)
			{return ((IConvertible) Value).ToInt64 (p);}
		sbyte IConvertible.ToSByte (IFormatProvider p)
			{return ((IConvertible) Value).ToSByte (p);}
		float IConvertible.ToSingle (IFormatProvider p)
			{return ((IConvertible) Value).ToSingle (p);}
		string IConvertible.ToString (IFormatProvider p)
			{return ((IConvertible) Value).ToString (p);}
		object IConvertible.ToType (Type to, IFormatProvider p)
			{return ((IConvertible) Value).ToType (to, p);}
		ushort IConvertible.ToUInt16 (IFormatProvider p)
			{return ((IConvertible) Value).ToUInt16 (p);}
		uint IConvertible.ToUInt32 (IFormatProvider p)
			{return ((IConvertible) Value).ToUInt32 (p);}
		ulong IConvertible.ToUInt64 (IFormatProvider p)
			{return ((IConvertible) Value).ToUInt64 (p);}
	}

	public class SimpleNumber3Math : Math<SimpleNumber3> {

		public override SimpleNumber3 FromInt32 (int value)
		{
			return new SimpleNumber3 (value);
		}

		public override SimpleNumber3 Add (SimpleNumber3 x, SimpleNumber3 y)
		{
			return new SimpleNumber3 (x.Value + y.Value);
		}

		public override SimpleNumber3 Subtract (SimpleNumber3 x, SimpleNumber3 y)
		{
			return new SimpleNumber3 (x.Value - y.Value);
		}

		public override SimpleNumber3 Multiply (SimpleNumber3 x, SimpleNumber3 y)
		{
			return new SimpleNumber3 (x.Value * y.Value);
		}

		public override SimpleNumber3 QuotientRemainder (SimpleNumber3 x, SimpleNumber3 y, out SimpleNumber3 remainder)
		{
			remainder = new SimpleNumber3 (x.Value % y.Value);
			return new SimpleNumber3 (x.Value / y.Value);
		}
	}

	[TestFixture]
	public class SimpleNumber3MathTests : MathContract<SimpleNumber3> {

		static SimpleNumber3MathTests ()
		{
			Math<SimpleNumber3>.SetDefault (new SimpleNumber3Math ());
		}
	}
}
