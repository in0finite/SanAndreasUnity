//
// TextReader.cs
//
// Author:
//   Jonathan Pryor <jpryor@novell.com>
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
using System.Text;

using Cadenza.Collections;

namespace Cadenza.IO {

	[Flags]
	public enum TextReaderCodaOptions {
		None          = 0,
		CloseReader   = 1,
	}

	public static class TextReaderCoda {

		public static IEnumerable<string> Lines (this TextReader self)
		{
			return Lines (self, TextReaderCodaOptions.CloseReader);
		}

		public static IEnumerable<string> Lines (this TextReader self, TextReaderCodaOptions options)
		{
			Check.Self (self);
			CheckOptions (options);

			return CreateLineIterator (self, options);
		}

		private static void CheckOptions (TextReaderCodaOptions options)
		{
			if (options != TextReaderCodaOptions.None && options != TextReaderCodaOptions.CloseReader)
				throw new ArgumentException ("options", "Invalid `options' value.");
		}

		private static IEnumerable<string> CreateLineIterator (TextReader self, TextReaderCodaOptions options)
		{
			try {
				string line;
				while ((line = self.ReadLine ()) != null)
					yield return line;
			} finally {
				if ((options & TextReaderCodaOptions.CloseReader) != 0) {
					self.Close ();
					self.Dispose ();
				}
			}
		}

		public static IEnumerable<string> Tokens (this TextReader self, params Func<char?, char, bool>[] categories)
		{
			Check.Self (self);
			Check.Categories (categories);
			if (categories.Length == 0)
				throw new ArgumentException ("categories", "Must provide at least one catagory");
			return Tokens (self, TextReaderCodaOptions.CloseReader, categories);
		}

		public static IEnumerable<string> Tokens (this TextReader self, TextReaderCodaOptions options, params Func<char?, char, bool>[] categories)
		{
			Check.Self (self);
			CheckOptions (options);
			Check.Categories (categories);
			if (categories.Length == 0)
				throw new ArgumentException ("categories", "Must provide at least one catagory");

			return CreateTokensIterator (self, options, categories);
		}

		private static IEnumerable<string> CreateTokensIterator (TextReader self, TextReaderCodaOptions options, Func<char?, char, bool>[] categories)
		{
			try {
				var cats = categories.Select (
						c => Lambda.F ((StringBuilder buf, char ch) => 
							c (buf.Length == 0 ? ((char?) null) : (char?) buf [buf.Length-1], ch)));
				foreach (var t in Chars (self).Tokens (
							new StringBuilder (),
							(buf, c) => buf.Append (c),
							buf => {
								var r = buf.ToString (); 
								buf.Length = 0; 
								return Tuple.Create (r, buf);
							},
							cats.ToArray ()))
					yield return t;
			} finally {
				if ((options & TextReaderCodaOptions.CloseReader) != 0) {
					self.Close ();
					self.Dispose ();
				}
			}
		}

		private static IEnumerable<char> Chars (TextReader self)
		{
			int c;
			while ((c = self.Read ()) >= 0)
				yield return (char) c;
		}

		public static IEnumerable<string> Words (this TextReader self)
		{
			return Words (self, TextReaderCodaOptions.CloseReader);
		}

		public static IEnumerable<string> Words (this TextReader self, TextReaderCodaOptions options)
		{
			Check.Self (self);
			CheckOptions (options);

			return Tokens (self, options, (p, c) => !char.IsWhiteSpace (c));
		}
	}
}
