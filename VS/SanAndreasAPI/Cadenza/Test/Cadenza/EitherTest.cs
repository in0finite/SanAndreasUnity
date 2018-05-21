//
// EitherTest.cs
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

using NUnit.Framework;

using Cadenza;

namespace Cadenza.Tests {

	[TestFixture]
	public class Either2EquatableContract : EquatableContract<Either<Action, object>>
	{
		protected override Either<Action, object> CreateValueX ()
		{
			return Either<Action, object>.A (() => {});
		}

		protected override Either<Action, object> CreateValueY ()
		{
			return Either<Action, object>.B ("y");
		}

		protected override Either<Action, object> CreateValueZ ()
		{
			return Either<Action, object>.B ("z");
		}
	}

	[TestFixture]
	public class Either3EquatableContract : EquatableContract<Either<Action, object, string>>
	{
		protected override Either<Action, object, string> CreateValueX ()
		{
			return Either<Action, object, string>.A (() => {});
		}

		protected override Either<Action, object, string> CreateValueY ()
		{
			return Either<Action, object, string>.B ("y");
		}

		protected override Either<Action, object, string> CreateValueZ ()
		{
			return Either<Action, object, string>.C ("z");
		}
	}

	[TestFixture]
	public class Either4EquatableContract : EquatableContract<Either<Action, object, string, Type>>
	{
		protected override Either<Action, object, string, Type> CreateValueX ()
		{
			return Either<Action, object, string, Type>.B ("x");
		}

		protected override Either<Action, object, string, Type> CreateValueY ()
		{
			return Either<Action, object, string, Type>.C ("y");
		}

		protected override Either<Action, object, string, Type> CreateValueZ ()
		{
			return Either<Action, object, string, Type>.D (typeof (string));
		}
	}

	[TestFixture]
	public class EitherTest : BaseRocksFixture {

		struct CustomConvertible : IConvertible {
			public const            bool      Boolean   = true;
			public const            byte      Byte      = (byte) 1;
			public const            char      Char      = 'a';
			public static readonly  DateTime  DateTime  = new DateTime (2008, 9, 27);
			public const            decimal   Decimal   = 5m;
			public const            double    Double    = 6.0;
			public const            short     Int16     = 7;
			public const            int       Int32     = 8;
			public const            long      Int64     = 9L;
			public const            sbyte     SByte     = (sbyte) 10;
			public const            float     Single    = 11.0f;
			public static readonly  string    String    = "string";
			public static readonly  object    Type      = new object ();
			public const            ushort    UInt16    = 14;
			public const            uint      UInt32    = 15U;
			public const            ulong     UInt64    = 16UL;

			TypeCode IConvertible.GetTypeCode ()
				{return TypeCode.Object;}
			bool IConvertible.ToBoolean (IFormatProvider p)
				{return Boolean;}
			byte IConvertible.ToByte (IFormatProvider p)
				{return Byte;}
			char IConvertible.ToChar (IFormatProvider p)
				{return Char;}
			DateTime IConvertible.ToDateTime (IFormatProvider p)
				{return DateTime;}
			decimal IConvertible.ToDecimal (IFormatProvider p)
				{return Decimal;}
			double IConvertible.ToDouble (IFormatProvider p)
				{return Double;}
			short IConvertible.ToInt16 (IFormatProvider p)
				{return Int16;}
			int IConvertible.ToInt32 (IFormatProvider p)
				{return Int32;}
			long IConvertible.ToInt64 (IFormatProvider p)
				{return Int64;}
			sbyte IConvertible.ToSByte (IFormatProvider p)
				{return SByte;}
			float IConvertible.ToSingle (IFormatProvider p)
				{return Single;}
			string IConvertible.ToString (IFormatProvider p)
				{return String;}
			object IConvertible.ToType (Type to, IFormatProvider p)
				{return Type;}
			ushort IConvertible.ToUInt16 (IFormatProvider p)
				{return UInt16;}
			uint IConvertible.ToUInt32 (IFormatProvider p)
				{return UInt32;}
			ulong IConvertible.ToUInt64 (IFormatProvider p)
				{throw new BadException ();}
		}

		class BadException : Exception {
		}

		[Test]
		public void TryParse ()
		{
			#region TryParse
			var v = Either.TryParse<int> ("3.14159");
			var e = v.Fold (i => null, i => i);
			Assert.IsNotNull (e);
			Assert.IsTrue (typeof(Exception).IsAssignableFrom (e.GetType()));

			v = Either.TryParse<int> ("42");
			var n = v.Fold (i => i, i => -1);
			Assert.AreEqual (42, n);

			var v2 = Either.TryParse<int?> ("3.14159");
			e = v2.Fold (i => null, i => i);
			Assert.IsNotNull (e);
			Assert.IsTrue (typeof(Exception).IsAssignableFrom (e.GetType()));

			v2 = Either.TryParse<int?> ("42");
			n = v2.Fold (i => i.Value, i => -1);
			Assert.AreEqual (42, n);
			#endregion
		}

		[Test]
		public void TryConvert_Validation ()
		{
			object value = null;
			Assert.Throws<ArgumentNullException>(() => Either.TryConvert<int>(value));
		}

		[Test]
		public void TryConvert ()
		{
			#region TryConvert
			Either<DateTime, Exception> a = Either.TryConvert<int, DateTime> (42);
			Exception e = a.Fold (i => null, i => i);
			Assert.IsNotNull (e);
			Assert.AreEqual (typeof (InvalidCastException), e.GetType ());

			Either<string, Exception> b = Either.TryConvert<int, string> (42);
			string n2 = b.Fold (i => i, i => null);
			Assert.AreEqual ("42", n2);

			Either<int, Exception> c = 
				Either.TryConvert<CustomConvertible, int> (new CustomConvertible ());
			int n3 = c.Fold (i => i, i => -1);
			Assert.AreEqual (CustomConvertible.Int32, n3);

			Either<ulong, Exception> u = 
				Either.TryConvert<CustomConvertible, ulong> (new CustomConvertible ());
			e = u.Fold (i => null, i => i);
			Assert.IsNotNull (e);
			Assert.AreEqual (typeof (NotSupportedException), e.GetType ());
			Assert.AreEqual (typeof (BadException), e.InnerException.GetType ());
			#endregion
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void Either2_A_ValueNull ()
		{
			Either<Action, object>.A (null);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void Either2_B_ValueNull ()
		{
			Either<Action, object>.B (null);
		}

		[Test]
		public void Either2_Fold_a_Null ()
		{
			Func<Action, int> a = null;
			Func<object, int> b = x => Convert.ToInt32(x);

			var e = Either<Action, object>.A (() => {});
			AssertException<ArgumentNullException> (() => e.Fold (a, b));

			e = Either<Action, object>.B (new object());
			AssertException<ArgumentNullException> (() => e.Fold (a, b));
		}

		[Test]
		public void Either2_Fold_b_Null ()
		{
			Func<Action, int> a = x => 42;
			Func<object, int> b = null;

			var e = Either<Action, object>.A (() => {});
			AssertException<ArgumentNullException> (() => e.Fold (a, b));

			e = Either<Action, object>.B (new object());
			AssertException<ArgumentNullException> (() => e.Fold (a, b));
		}

		[Test]
		public void Either2_Fold ()
		{
			Action a = () => {};
			Either<Action, object> e = Either<Action, object>.A (a);
			Assert.AreEqual (a, e.Fold (v => v, v => null));

			e = Either<Action, object>.B ("foo");
			Assert.AreEqual ("foo", e.Fold (v => null, v => v.ToString ()));
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void Either3_A_ValueNull ()
		{
			Either<Action, object, string>.A (null);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void Either3_B_ValueNull ()
		{
			Either<Action, object, string>.B (null);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void Either3_C_ValueNull ()
		{
			Either<Action, object, string>.C (null);
		}

		[Test]
		public void Either3_Fold_a_Null ()
		{
			Func<Action, int> a = null;
			Func<object, int> b = x => Convert.ToInt32(x);
			Func<string, int> c = x => Convert.ToInt32(x);

			var e = Either<Action, object, string>.A (() => {});
			AssertException<ArgumentNullException> (() => e.Fold (a, b, c));

			e = Either<Action, object, string>.B (new object());
			AssertException<ArgumentNullException> (() => e.Fold (a, b, c));

			e = Either<Action, object, string>.C ("foo");
			AssertException<ArgumentNullException> (() => e.Fold (a, b, c));
		}

		[Test]
		public void Either3_Fold_b_Null ()
		{
			Func<Action, int> a = x => 42;
			Func<object, int> b = null;
			Func<string, int> c = x => Convert.ToInt32(x);

			var e = Either<Action, object, string>.A (() => {});
			AssertException<ArgumentNullException> (() => e.Fold (a, b, c));

			e = Either<Action, object, string>.B (new object());
			AssertException<ArgumentNullException> (() => e.Fold (a, b, c));

			e = Either<Action, object, string>.C ("foo");
			AssertException<ArgumentNullException> (() => e.Fold (a, b, c));
		}

		[Test]
		public void Either3_Fold_c_Null ()
		{
			Func<Action, int> a = x => 42;
			Func<object, int> b = x => Convert.ToInt32(x);
			Func<string, int> c = null;

			var e = Either<Action, object, string>.A (() => {});
			AssertException<ArgumentNullException> (() => e.Fold (a, b, c));

			e = Either<Action, object, string>.B (new object());
			AssertException<ArgumentNullException> (() => e.Fold (a, b, c));

			e = Either<Action, object, string>.C ("foo");
			AssertException<ArgumentNullException> (() => e.Fold (a, b, c));
		}

		[Test]
		public void Either3_Fold ()
		{
			Action a = () => {};
			Either<Action, object, string> e = Either<Action, object, string>.A (a);
			Assert.AreEqual (a, e.Fold (v => v, v => null, v => null));

			e = Either<Action, object, string>.C ("foo");
			Assert.AreEqual ("foo", e.Fold (v => null, v => null, v => v));
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void Either4_A_ValueNull ()
		{
			Either<Action, object, string, Type>.A (null);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void Either4_B_ValueNull ()
		{
			Either<Action, object, string, Type>.B (null);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void Either4_C_ValueNull ()
		{
			Either<Action, object, string, Type>.C (null);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void Either4_D_ValueNull ()
		{
			Either<Action, object, string, Type>.D (null);
		}

		[Test]
		public void Either4_Fold_a_Null ()
		{
			Func<Action, int> a = null;
			Func<object, int> b = x => Convert.ToInt32(x);
			Func<string, int> c = x => Convert.ToInt32(x);
			Func<Type,   int> d = x => 42;

			var e = Either<Action, object, string, Type>.A (() => {});
			AssertException<ArgumentNullException> (() => e.Fold (a, b, c, d));

			e = Either<Action, object, string, Type>.B (new object());
			AssertException<ArgumentNullException> (() => e.Fold (a, b, c, d));

			e = Either<Action, object, string, Type>.C ("foo");
			AssertException<ArgumentNullException> (() => e.Fold (a, b, c, d));

			e = Either<Action, object, string, Type>.D (typeof (object));
			AssertException<ArgumentNullException> (() => e.Fold (a, b, c, d));
		}

		[Test]
		public void Either4_Fold_b_Null ()
		{
			Func<Action, int> a = x => 42;
			Func<object, int> b = null;
			Func<string, int> c = x => Convert.ToInt32(x);
			Func<Type,   int> d = x => 42;

			var e = Either<Action, object, string, Type>.A (() => {});
			AssertException<ArgumentNullException> (() => e.Fold (a, b, c, d));

			e = Either<Action, object, string, Type>.B (new object());
			AssertException<ArgumentNullException> (() => e.Fold (a, b, c, d));

			e = Either<Action, object, string, Type>.C ("foo");
			AssertException<ArgumentNullException> (() => e.Fold (a, b, c, d));

			e = Either<Action, object, string, Type>.D (typeof (object));
			AssertException<ArgumentNullException> (() => e.Fold (a, b, c, d));
		}

		[Test]
		public void Either4_Fold_c_Null ()
		{
			Func<Action, int> a = x => 42;
			Func<object, int> b = x => Convert.ToInt32(x);
			Func<string, int> c = null;
			Func<Type,   int> d = x => 42;

			var e = Either<Action, object, string, Type>.A (() => {});
			AssertException<ArgumentNullException> (() => e.Fold (a, b, c, d));

			e = Either<Action, object, string, Type>.B (new object());
			AssertException<ArgumentNullException> (() => e.Fold (a, b, c, d));

			e = Either<Action, object, string, Type>.C ("foo");
			AssertException<ArgumentNullException> (() => e.Fold (a, b, c, d));

			e = Either<Action, object, string, Type>.D (typeof (object));
			AssertException<ArgumentNullException> (() => e.Fold (a, b, c, d));
		}

		[Test]
		public void Either4_Fold_d_Null ()
		{
			Func<Action, int> a = x => 42;
			Func<object, int> b = x => Convert.ToInt32(x);
			Func<string, int> c = x => Convert.ToInt32(x);
			Func<Type,   int> d = null;

			var e = Either<Action, object, string, Type>.A (() => {});
			AssertException<ArgumentNullException> (() => e.Fold (a, b, c, d));

			e = Either<Action, object, string, Type>.B (new object());
			AssertException<ArgumentNullException> (() => e.Fold (a, b, c, d));

			e = Either<Action, object, string, Type>.C ("foo");
			AssertException<ArgumentNullException> (() => e.Fold (a, b, c, d));

			e = Either<Action, object, string, Type>.D (typeof (object));
			AssertException<ArgumentNullException> (() => e.Fold (a, b, c, d));
		}

		[Test]
		public void Either4_Fold ()
		{
			Action a = () => {};
			Either<Action, object, string, Type> e = Either<Action, object, string, Type>.A (a);
			Assert.AreEqual (a, e.Fold (v => v, v => null, v => null, v => null));

			e = Either<Action, object, string, Type>.D (typeof (object));
			Assert.AreEqual (typeof (object), e.Fold (v => null, v => null, v => null, v => v));
		}
	}
}
