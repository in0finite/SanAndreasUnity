//
// IEnumerableTest.cs
//
// Author:
//   Jonathan Pryor (jpryor@novell.com)
//
// Copyright (c) 2010 Novell, Inc. (http://www.novell.com)
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

using Cadenza.Collections;
using Cadenza.Tests;

namespace Cadenza.Collections.Tests {

	[TestFixture]
	public class DictionaryCodaTest : BaseRocksFixture {

		[Test]
		public void GetValueOrDefault_Self ()
		{
			IDictionary<string, int> s = null;
			Assert.Throws<ArgumentNullException>(() => s.GetValueOrDefault ("foo"));
			Assert.Throws<ArgumentNullException>(() => s.GetValueOrDefault ("foo", 42));
		}

		[Test]
		public void GetValueOrDefault ()
		{
			#region GetValueOrDefault
			var dict = new Dictionary<string, int> () {
				{ "a", 1 },
			};
			Assert.AreEqual (1, dict.GetValueOrDefault ("a"));
			Assert.AreEqual (0, dict.GetValueOrDefault ("c"));
			Assert.AreEqual (3, dict.GetValueOrDefault ("c", 3));
			#endregion
		}

		[Test]
		public void SequenceEqual_Arguments ()
		{
			IDictionary<string, string> s = null;
			IDictionary<string, string> o = new Dictionary<string, string> ();
			Assert.Throws<ArgumentNullException>(() => s.SequenceEqual (o));
			s = o;
			o = null;
			Assert.Throws<ArgumentNullException>(() => s.SequenceEqual (o));
		}

		[Test]
		public void SequenceEqual ()
		{
			var a = new Dictionary<string, string> {
				{ "a", "1" },
				{ "b", "2" },
			};
			var b = new Dictionary<string, string> {
				{ "b", "2" },
				{ "a", "1" },
			};
			Assert.IsTrue (a.SequenceEqual (b));
			b.Remove ("a");
			Assert.IsFalse (a.SequenceEqual (b));

			a.Clear ();
			b.Clear ();
			Assert.IsTrue (a.SequenceEqual (b));
		}

		[Test]
		public void UpdateValue_Arguments ()
		{
			IDictionary<string, int> s = null;
			Assert.Throws<ArgumentNullException>(() => s.UpdateValue ("key", v => v));
			s = new Dictionary<string, int> ();
			Assert.Throws<ArgumentNullException>(() => s.UpdateValue ("key", null));
		}

		[Test]
		public void UpdateValue ()
		{
			#region UpdateValue
			var words = new[]{
				"Count",
				"the",
				"the",
				"repeated",
				"words",
			};
			var wordCounts = new Dictionary<string, int> ();
			foreach (var word in words) {
				int c;
				wordCounts.TryGetValue (word, out c);
				Assert.AreEqual (c + 1, wordCounts.UpdateValue (word, v => v + 1));
			}
			Assert.AreEqual (4, wordCounts.Count);
			Assert.AreEqual (1, wordCounts ["Count"]);
			Assert.AreEqual (2, wordCounts ["the"]);
			Assert.AreEqual (1, wordCounts ["repeated"]);
			Assert.AreEqual (1, wordCounts ["words"]);
			#endregion
		}

		[Test]
		public void GetValueOrCreate_Arguments ()
		{
			IDictionary<string, int> s = null;
			Assert.Throws<ArgumentNullException>(() => s.GetValueOrCreate ("foo"));
			Assert.Throws<ArgumentNullException>(() => s.GetValueOrCreate ("foo", null));
		}

		[Test]
		public void GetValueOrCreate_ReturnsNewValue ()
		{
			var s = new Dictionary<string, List<int>>();
			Assert.AreEqual (new List<int>(), s.GetValueOrCreate ("foo"));
			List<int> v = null;
			var r = s.GetValueOrCreate ("bar", () => v = new List<int> ());
			Assert.AreSame (r, v);
		}

		[Test]
		public void GetValueOrCreate_ReturnsOldValue ()
		{
			var s = new Dictionary<string, List<int>> () {
				{ "foo", new List<int> {42} },
			};
			AssertAreSame (new[]{42}, s.GetValueOrCreate ("foo"));
			AssertAreSame (new[]{42}, s.GetValueOrCreate ("foo", () => new List<int> {1}));
		}

		[Test]
		public void TryRemove_Arguments()
		{
			IDictionary<string, int> s = null;
			int i;
			Assert.Throws<ArgumentNullException> (() => s.TryRemove ("foo", out i));
		}

		[Test]
		public void TryRemove_Found()
		{
			var dict = new Dictionary<string, string> { { "foo", "bar" } };

			string value;
			Assert.IsTrue (dict.TryRemove ("foo", out value));
			Assert.AreEqual ("bar", value);
		}

		[Test]
		public void TryRemove_NotFound()
		{
			var dict = new Dictionary<string, string> { { "foo", "bar" } };

			string value;
			Assert.IsFalse (dict.TryRemove ("foo2", out value));
		}
	}
}
