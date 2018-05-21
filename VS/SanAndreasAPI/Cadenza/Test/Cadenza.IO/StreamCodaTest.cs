//
// StreamTest.cs
//
// Authors:
//   Jonathan Pryor  <jpryor@novell.com>
//   Bojan Rajkovic  <bojanr@brandeis.edu>
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

using NUnit.Framework;

using Cadenza.IO;
using Cadenza.Tests;

namespace Cadenza.IO.Tests {

	[TestFixture]
	public class StreamTest : BaseRocksFixture {

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void WithSystemConverter_SelfNull ()
		{
			Stream s = null;
			s.WithSystemConverter();
		}

		[Test]
		public void WithSystemConverter ()
		{
			Stream s = new MemoryStream ();
			s.WithSystemConverter();
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void WriteTo_SelfNull ()
		{
			Stream s = null;
			s.WriteTo (new MemoryStream ());
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void WriteTo_DestinationNull ()
		{
			Stream s = new MemoryStream ();
			s.WriteTo (null);
		}

		[Test]
		public void WriteTo ()
		{
			Stream s = new MemoryStream (
					Encoding.UTF8.GetBytes ("Hello, world!"));
			MemoryStream d = new MemoryStream ();

			s.Position = 7;
			s.WriteTo (d);
			Assert.AreEqual (13, s.Position);
			Assert.AreEqual (6, d.Length);
			Assert.AreEqual (6, d.Position);
			Assert.AreEqual ("world!", Encoding.UTF8.GetString (d.ToArray ()));
		}
	}
}
