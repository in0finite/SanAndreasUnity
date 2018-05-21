//
// TextValueReaderTest.cs
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
using System.IO;

using NUnit.Framework;

using Cadenza.Collections;
using Cadenza.IO;
using Cadenza.Tests;

namespace Cadenza.IO.Tests {

	struct TvrOp {
		public char op;
		public string left;
		public string right;
	}

	class TvrData1 {
		public bool   A;
		public byte   B;
		public char   C;
		public double D;
	}

	class TvrData2 {
		public string F;
		public short  A;
		public int    B;
		public TvrOp  O;
		public long   C;
		public float  D;
	}

	static class TvrDataReaders {
		public static IValueReader Read (this IValueReader reader, out TvrData1 d)
		{
			d = new TvrData1 ();
			char paren;

			return reader
				.Read (out paren)
				.Read (out d.A).Read (out d.B).Read (out d.C).Read (out d.D)
				.Read (out paren);
		}

		public static IValueReader Read (this IValueReader reader, out TvrOp o)
		{
			o = new TvrOp ();
			char paren;

			reader.Read (out paren)
				.Read (out o.op)
				.Read (out o.left)
				.Read (out o.right)
				.Read (out paren);
			return reader;
		}

		public static IValueReader Read (this IValueReader reader, out TvrData2 d)
		{
			d = new TvrData2 ();
			char paren;

			reader
				.Read (out paren)
				.Read (out d.F)
				.Read (out d.A).Read (out d.B)
				.Read (out d.O)
				.Read (out d.C).Read (out d.D)
				.Read (out paren);
			return reader;
		}
	}

	[TestFixture]
	public class TextValueReaderTest : BaseRocksFixture {

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void TextValueReader_ReaderNull ()
		{
			new TextValueReader (null);
		}

		[Test, ExpectedException (typeof (InvalidCastException))]
		public void Read_UnsupportedT ()
		{
			TextValueReader r = new[]{"component-model-doesn't-support-TvrOp"}
				.ToValueReader ();
			TvrOp op;
			ValueReaderCoda.Read (r, out op);
		}

		[Test, ExpectedException (typeof (InvalidOperationException))]
		public void Read_PastEnd ()
		{
			string a, b;
			new[]{"foo"}.ToValueReader ().Read (out a).Read (out b);
		}

		[Test]
		public void Read_Generic ()
		{
			TextValueReader r = new[]{"2008-09-29"}.ToValueReader ();
			DateTime d;
			r.Read (out d);
			Assert.AreEqual (2008,  d.Year);
			Assert.AreEqual (9,     d.Month);
			Assert.AreEqual (29,    d.Day);
		}

		[Test]
		public void Reads ()
		{
			TextReader s = new StringReader (" pi  (True 42 C 3.14159) e (Y  1 2 (/3 4) 4 2.71  )  ");
			var categories = new Func<char?, char, bool>[] {
				(_, c) => char.IsLetterOrDigit (c) || c == '.',
				(_, c) => c == '+' || c == '-' || c == '*' || c == '/',
				(_, c) => !char.IsWhiteSpace (c),
			};
			var reader = new TextValueReader (s.Tokens (categories));

			TvrData1 a;
			TvrData2 b;
			string p, e;
			reader.Read (out p)
				.Read (out a)
				.Read (out e)
				.Read (out b);

			Assert.AreEqual ("pi", p);
			Assert.AreEqual ("e", e);

			Assert.AreEqual (true,      a.A);
			Assert.AreEqual ((byte) 42, a.B);
			Assert.AreEqual ('C',       a.C);
			Assert.AreEqual (3.14159,   a.D);

			Assert.AreEqual ("Y",   b.F);
			Assert.AreEqual (1,     b.A);
			Assert.AreEqual (2,     b.B);
			Assert.AreEqual ("3",   b.O.left);
			Assert.AreEqual ('/',   b.O.op);
			Assert.AreEqual ("4",   b.O.right);
			Assert.AreEqual (4,     b.C);
			Assert.AreEqual (2.71f, b.D);
		}
	}
}
