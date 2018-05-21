//
// ReadOnlyDictionaryTests.cs
//
// Author:
//   Eric Maupin  <me@ermau.com>
//
// Copyright (c) 2009 Eric Maupin (http://www.ermau.com)
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
using System.Linq;

using NUnit.Framework;

using Cadenza.Collections;

namespace Cadenza.Collections.Tests
{
	[TestFixture]
	public class ReadOnlyDictionaryTests
	{
		[Test]
		public void ConstructorKeySelector()
		{
			var dict = new ReadOnlyDictionary<int, DateTime> (new[]{
				new DateTime (2009, 1, 1),
				new DateTime (2008, 1, 1),
				new DateTime (2007, 1, 1)
			}.ToDictionary(d => d.Year));

			Assert.IsTrue (dict.ContainsKey (2009));
			Assert.IsTrue (dict.ContainsKey (2008));
			Assert.IsTrue (dict.ContainsKey (2007));
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void Ctor_DictNull()
		{
			IDictionary<string, string> dict = null;
			new ReadOnlyDictionary<string, string> (dict);
		}

		[Test]
		public void IsReadOnly()
		{
			Assert.IsTrue (((ICollection<KeyValuePair<string, string>>)GetBlankDict()).IsReadOnly);
		}

		private static ReadOnlyDictionary<string, string> GetBlankDict()
		{
			return new ReadOnlyDictionary<string, string> (new Dictionary<string, string>());
		}
	}

	[TestFixture]
	public class ReadOnlyDictionaryCollectionContractTests : CollectionContract<KeyValuePair<string, string>> {
		protected override ICollection<KeyValuePair<string, string>> CreateCollection (IEnumerable<KeyValuePair<string, string>> values)
		{
			var d = new Dictionary<string, string> ();
			foreach (var v in values)
				d.Add (v.Key, v.Value);
			return new ReadOnlyDictionary<string, string> (d);
		}

		protected override KeyValuePair<string, string> CreateValueA ()
		{
			return new KeyValuePair<string, string> ("A", "1");
		}

		protected override KeyValuePair<string, string> CreateValueB ()
		{
			return new KeyValuePair<string, string> ("B", "2");
		}

		protected override KeyValuePair<string, string> CreateValueC ()
		{
			return new KeyValuePair<string, string> ("C", "3");
		}
	}

	[TestFixture]
	public class ReadOnlyDictionaryDictionaryContractTests : DictionaryContract {
		protected override IDictionary<string, string> CreateDictionary (IEnumerable<KeyValuePair<string, string>> values)
		{
			var d = new Dictionary<string, string> ();
			foreach (var v in values)
				d.Add (v.Key, v.Value);
			return new ReadOnlyDictionary<string, string> (d);
		}
	}
}
