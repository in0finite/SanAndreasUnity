//
// IEnumerableContract.cs
//
// Author:
//   Jonathan Pryor  <jpryor@novell.com>
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
using System.Linq;

using NUnit.Framework;

using Cadenza.Collections;
using Cadenza.Tests;

namespace Cadenza.Collections.Tests {

	public abstract class DictionaryContract : BaseRocksFixture {

		protected abstract IDictionary<string, string> CreateDictionary (IEnumerable<KeyValuePair<string, string>> values);

		[Test]
		public void Add ()
		{
			var d = CreateDictionary (new KeyValuePair<string, string> [0]);

			var n = d.Count;
			Assert.AreEqual (n, d.Keys.Count);
			Assert.AreEqual (n, d.Values.Count);

			// key cannot be null
			try {
				d.Add ("key", "value");
				Assert.IsTrue (d.ContainsKey ("key"));
				Assert.IsFalse (d.ContainsKey ("value"));
				Assert.AreEqual (n+1, d.Keys.Count);
				Assert.AreEqual (n+1, d.Values.Count);
				Assert.IsTrue (d.Keys.Contains ("key"));
				Assert.IsTrue (d.Values.Contains ("value"));

				// Cannot use Add() w/ the same key
				Assert.Throws<ArgumentException>(() => d.Add ("key", "value2"));

				Assert.Throws<ArgumentNullException>(() => d.Add (null, null));
			}
			catch (NotSupportedException) {
				Assert.IsTrue (d.IsReadOnly);
			}
		}

		[Test]
		public void ContainsKey ()
		{
			var d = CreateDictionary (new KeyValuePair<string, string> []{
				new KeyValuePair<string, string> ("another-key", "another-value"),
			});
			Assert.Throws<ArgumentNullException>(() => d.ContainsKey (null));
			Assert.IsFalse (d.ContainsKey ("key"));
			Assert.IsTrue (d.ContainsKey ("another-key"));
			Assert.IsTrue (d.Keys.Contains ("another-key"));
		}

		[Test]
		public void Remove ()
		{
			var d = CreateDictionary (new KeyValuePair<string, string> []{
				new KeyValuePair<string, string> ("another-key", "another-value"),
			});
			var n = d.Count;
			try {
				Assert.IsFalse (d.Remove ("key"));
				Assert.AreEqual (n, d.Count);
				Assert.IsTrue (d.Remove ("another-key"));
				Assert.AreEqual (n-1, d.Count);
				Assert.AreEqual (n-1, d.Keys.Count);
				Assert.AreEqual (n-1, d.Values.Count);
				Assert.IsFalse (d.Keys.Contains ("another-key"));
				Assert.IsFalse (d.Values.Contains ("another-value"));

				Assert.Throws<ArgumentNullException>(() => d.Remove (null));
			}
			catch (NotSupportedException) {
				Assert.IsTrue (d.IsReadOnly);
			}
		}

		[Test]
		public void TryGetValue ()
		{
			var d = CreateDictionary (new KeyValuePair<string, string> []{
				new KeyValuePair<string, string> ("key", "value"),
			});
			string v = null;
			Assert.Throws<ArgumentNullException>(() => d.TryGetValue (null, out v));
			Assert.IsFalse (d.TryGetValue ("another-key", out v));
			Assert.IsTrue (d.TryGetValue ("key", out v));
			Assert.AreEqual ("value", v);
		}

		[Test]
		public void Item ()
		{
			var d = CreateDictionary (new KeyValuePair<string, string> []{
				new KeyValuePair<string, string> ("key", "value"),
			});
			#pragma warning disable 0168
			Assert.Throws<ArgumentNullException>(() => {var _ = d [null];});
			Assert.Throws<KeyNotFoundException>(() => {var _ = d ["another-key"];});
			#pragma warning restore
			try {
				d ["key"] = "another-value";
				Assert.IsFalse (d.Values.Contains ("value"));
				Assert.IsTrue (d.Values.Contains ("another-value"));
				Assert.AreEqual ("another-value", d ["key"]);
				Assert.AreEqual (1, d.Keys.Count);
				Assert.AreEqual (1, d.Values.Count);
			}
			catch (NotSupportedException) {
				Assert.IsTrue (d.IsReadOnly);
			}

		}

		[Test]
		public void Keys_And_Values_Order_Must_Match ()
		{
			var d = CreateDictionary (new KeyValuePair<string, string> []{
				new KeyValuePair<string, string> ("a", "1"),
				new KeyValuePair<string, string> ("b", "2"),
				new KeyValuePair<string, string> ("c", "3"),
			});
			Assert.AreEqual (d.Keys.IndexOf ("a"), d.Values.IndexOf ("1"));
			Assert.AreEqual (d.Keys.IndexOf ("b"), d.Values.IndexOf ("2"));
			Assert.AreEqual (d.Keys.IndexOf ("c"), d.Values.IndexOf ("3"));
		}

		class SubCollectionContract : CollectionContract<string> {
			DictionaryContract dictContract;
			Func<IDictionary<string, string>, ICollection<string>> collectionSelector;

			public SubCollectionContract (DictionaryContract dictContract,
					Func<IDictionary<string, string>, ICollection<string>> collectionSelector)
			{
				this.dictContract = dictContract;
				this.collectionSelector = collectionSelector;
			}

			protected override ICollection<string> CreateCollection (IEnumerable<string> values)
			{
				var d = dictContract.CreateDictionary (values.Select (v => new KeyValuePair<string, string>(v, v)));
				var c = collectionSelector (d);
				Assert.IsTrue (c.IsReadOnly);
				return c;
			}

			protected override string CreateValueA ()
			{
				return "A";
			}

			protected override string CreateValueB ()
			{
				return "B";
			}

			protected override string CreateValueC ()
			{
				return "C";
			}
		}

		[Test]
		public void Keys ()
		{
			new SubCollectionContract (this, d => d.Keys).RunAllTests ();
		}

		[Test]
		public void Values ()
		{
			new SubCollectionContract (this, d => d.Values).RunAllTests ();
		}
	}
}

