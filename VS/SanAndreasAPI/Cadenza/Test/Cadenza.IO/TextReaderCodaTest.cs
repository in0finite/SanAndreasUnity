//
// TextReaderTest.cs
//
// Author:
//   Jonathan Pryor  <jpryor@novell.com>
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
using System.IO;
using System.Linq;

using NUnit.Framework;

using Cadenza.IO;
using Cadenza.Tests;

namespace Cadenza.IO.Tests {

	[TestFixture]
	public class TextReaderTest : BaseRocksFixture {

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Lines_SelfNull ()
		{
			TextReader s = null;
			s.Lines ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Lines_OptionsInvalid()
		{
			TextReader s = new StringReader ("");
			s.Lines ((TextReaderCodaOptions) (-1));
		}

		class MyStringReader : StringReader {
			public bool WasDisposed;

			public MyStringReader (string s)
				: base (s)
			{
			}

			protected override void Dispose (bool disposing)
			{
				if (disposing)
					WasDisposed = true;
			}
		}

		[Test]
		public void Lines ()
		{
			MyStringReader r = new MyStringReader ("hello\nout\rthere\r\nin\nTV\nland!");
			string[] lines = r.Lines ().ToArray ();
			Assert.IsTrue (r.WasDisposed);
			Assert.AreEqual (6, lines.Length);
			Assert.AreEqual ("hello", lines [0]);
			Assert.AreEqual ("out",   lines [1]);
			Assert.AreEqual ("there", lines [2]);
			Assert.AreEqual ("in",    lines [3]);
			Assert.AreEqual ("TV",    lines [4]);
			Assert.AreEqual ("land!", lines [5]);

			r = new MyStringReader ("\nhello\n\nworld!");
			lines = r.Lines (TextReaderCodaOptions.None).ToArray ();
			Assert.IsFalse (r.WasDisposed);
			Assert.AreEqual (4, lines.Length);
			Assert.AreEqual ("",        lines [0]);
			Assert.AreEqual ("hello",   lines [1]);
			Assert.AreEqual ("",        lines [2]);
			Assert.AreEqual ("world!",  lines [3]);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void Tokens_SelfNull ()
		{
			TextReader s = null;
			s.Tokens ();
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void Tokens_CategoriesNull ()
		{
			TextReader                         s = new StringReader ("");
			Func<char?, char, bool>[] categories = null;
			IEnumerable<string>                r = s.Tokens (categories);
			Ignore (r);
		}

		[Test, ExpectedException (typeof (ArgumentException))]
		public void Tokens_CategoriesEmpty ()
		{
			TextReader                         s = new StringReader ("");
			Func<char?, char, bool>[] categories = new Func<char?, char, bool>[0];
			IEnumerable<string>                r = s.Tokens (categories);
			Ignore (r);
		}

		[Test, ExpectedException (typeof (ArgumentException))]
		public void Tokens_OptionsInvalid()
		{
			TextReader s = new StringReader ("");
			s.Tokens ((TextReaderCodaOptions) (-1));
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void Tokens_FullCategoriesNull ()
		{
			TextReader                         s = new StringReader ("");
			Func<char?, char, bool>[] categories = null;
			IEnumerable<string>                r = s.Tokens (TextReaderCodaOptions.None, categories);
			Ignore (r);
		}

		[Test, ExpectedException (typeof (ArgumentException))]
		public void Tokens_FullCategoriesEmpty ()
		{
			TextReader                         s = new StringReader ("");
			Func<char?, char, bool>[] categories = new Func<char?, char, bool>[0];
			IEnumerable<string>                r = s.Tokens (TextReaderCodaOptions.None, categories);
			Ignore (r);
		}

		[Test]
		public void Tokens ()
		{
			#region Tokens
			var r = new MyStringReader ("(append 3.5 \"hello, world!\")");
			var words = r.Tokens (
				(p, c) => char.IsLetterOrDigit (c) || c == '.',
				(p, c) => !char.IsWhiteSpace (c))
				.ToArray ();
			Assert.IsTrue (r.WasDisposed);
			Assert.IsTrue (
					new[]{"(", "append", "3.5", "\"", "hello", ",", "world", "!\")"}
					.SequenceEqual (words));

			r = new MyStringReader ("Hello, world!");
			Assert.AreEqual (false, 
				r.Tokens (TextReaderCodaOptions.None,
					(p, c) => false).Any ());
			Assert.IsFalse (r.WasDisposed);
			#endregion
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Words_SelfNull ()
		{
			TextReader s = null;
			s.Words ();
		}

		[Test, ExpectedException (typeof (ArgumentException))]
		public void Words_OptionsInvalid()
		{
			TextReader s = new StringReader ("");
			s.Words ((TextReaderCodaOptions) (-1));
		}

		[Test]
		public void Words ()
		{
			#region Words
			MyStringReader r = new MyStringReader ("   (skip  leading,\r\n\tand trailing\vwhitespace)   ");
			string[] words = r.Words ().ToArray ();
			Assert.IsTrue (r.WasDisposed);
			Assert.AreEqual (5, words.Length);
			Assert.AreEqual ("(skip",       words [0]);
			Assert.AreEqual ("leading,",    words [1]);
			Assert.AreEqual ("and",         words [2]);
			Assert.AreEqual ("trailing",    words [3]);
			Assert.AreEqual ("whitespace)", words [4]);

			r = new MyStringReader ("notext");
			words = r.Words (TextReaderCodaOptions.None).ToArray ();
			Assert.IsFalse (r.WasDisposed);
			Assert.AreEqual (1, words.Length);
			Assert.AreEqual ("notext", words [0]);

			r = new MyStringReader ("1 2 3 4");
			Assert.AreEqual ("1", r.Words ().First ());
			Assert.AreEqual ("2", r.Words ().First ());
			Assert.AreEqual ("3", r.Words ().First ());
			Assert.AreEqual ("4", r.Words ().First ());
			#endregion
		}
	}
}
