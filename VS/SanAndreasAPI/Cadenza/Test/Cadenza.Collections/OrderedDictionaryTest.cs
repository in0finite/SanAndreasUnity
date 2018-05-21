//
// OrderedDictionaryTest.cs
//
// Author:
//   Eric Maupin  <me@ermau.com>
//
// Copyright (c) 2009 Eric Maupin (http://wwwermau.com)
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

namespace Cadenza.Collections.Tests
{
	[TestFixture]
	public class OrderedDictionaryTest : BaseRocksFixture
	{
		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void Ctor_DictNull()
		{
			Dictionary<string, string> foo = null;
			new OrderedDictionary<string, string>(foo);
		}

		[Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Ctor_CapacityOutOfRange()
		{
			new OrderedDictionary<string, string>(-1);
		}

		[Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Ctor_CapacityOutOfRangeWithEquality()
		{
			new OrderedDictionary<string, string> (-1, null);
		}

		[Test]
		public void KeyIndexer()
		{
			var dict = new OrderedDictionary<string, string> { { "foo", "bar" }, { "baz", "monkeys" } };
			Assert.AreEqual("bar", dict["foo"]);
			Assert.AreEqual("monkeys", dict["baz"]);
		}

		[Test, ExpectedException (typeof (KeyNotFoundException))]
		public void KeyIndexer_KeyNotFound()
		{
			var dict = new OrderedDictionary<string, string>
			{ { "foo", "bar" }, { "baz", "monkeys" } };

			dict["wee"].ToString();
		}

		[Test]
		public void KeyIndexerSet()
		{
			var dict = new OrderedDictionary<uint, int>();
			dict[(uint) 1] = 1;
			dict[(uint)2] = 2;
			dict[(uint)3] = 3;
			dict.Remove (2);
			dict[(uint)4] = 4;

			Assert.AreEqual(1, dict[(int)0]);
			Assert.AreEqual(3, dict[(int)1]);
			Assert.AreEqual(4, dict[(int)2]);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void KeyIndexerGet_KeyNull()
		{
			var dict = new OrderedDictionary<string, string>();

			dict[null].ToString();
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void KeyIndexerSet_KeyNull()
		{
			var dict = new OrderedDictionary<string, string>();

			dict[null] = "foo";
		}

		[Test]
		public void IndexIndexer()
		{
			var dict = new OrderedDictionary<uint, int>();
			dict.Add (1, 1);
			dict.Add (2, 2);
			dict.Add (3, 3);
			dict.Remove (2);
			dict.Add (4, 4);

			Assert.AreEqual (1, dict[(int)0]);
			Assert.AreEqual (3, dict[(int)1]);
			Assert.AreEqual (4, dict[(int)2]);
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Indexer_IndexOutOfRangeLower()
		{
			var dict = new OrderedDictionary<string, string>();
			dict[-1].ToString();
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Indexer_IndexOutOfRangeUpper()
		{
			var dict = new OrderedDictionary<string, string>
			{ { "foo", "bar" }, { "baz", "monkeys" } };

			dict[2].ToString();
		}

		[Test]
		public void EnumerableOrder()
		{
			var dict = new OrderedDictionary<uint, int>();
			dict.Add(1, 1);
			dict.Add(2, 2);
			dict.Add(3, 3);
			dict.Remove(2);
			dict.Add(4, 4);

			using (var enumerator = dict.GetEnumerator())
			{
				Assert.IsTrue (enumerator.MoveNext());
				Assert.AreEqual (1, enumerator.Current.Value);
				Assert.IsTrue(enumerator.MoveNext());
				Assert.AreEqual (3, enumerator.Current.Value);
				Assert.IsTrue(enumerator.MoveNext());
				Assert.AreEqual (4, enumerator.Current.Value);
			}
		}

		[Test]
		public void Values_EnumerableOrder()
		{
			var dict = new OrderedDictionary<uint, int>();
			dict.Add(1, 1);
			dict.Add(2, 2);
			dict.Add(3, 3);
			dict.Remove(2);
			dict.Add(4, 4);

			Assert.AreEqual (1, dict.Values.ElementAt (0));
			Assert.AreEqual (3, dict.Values.ElementAt (1));
			Assert.AreEqual (4, dict.Values.ElementAt (2));
		}

		[Test]
		public void CopyTo()
		{
			var dict = new OrderedDictionary<uint, int>();
			dict.Add(1, 1);
			dict.Add(2, 2);
			dict.Add(3, 3);
			dict.Remove(2);
			dict.Add(4, 4);

			KeyValuePair<uint, int>[] a = new KeyValuePair<uint, int>[13];

			((ICollection<KeyValuePair<uint, int>>)dict).CopyTo (a, 10);

			for (int i = 0; i < 10; ++i)
			{
				if (i < 10)
					Assert.AreEqual (default(KeyValuePair<uint, int>), a[i]);
			}

			Assert.AreEqual (1, a[10].Value);
			Assert.AreEqual (3, a[11].Value);
			Assert.AreEqual (4, a[12].Value);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void CopyTo_NullArray()
		{
			var dict = new OrderedDictionary<string, string>();
			KeyValuePair<string, string>[] a = null;

			((ICollection<KeyValuePair<string, string>>)dict).CopyTo(a, 0);
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void CopyTo_ArrayTooSmall()
		{
			var dict = new OrderedDictionary<string, string>();
			for (int i = 0; i < 1000; ++i)
				dict.Add(i.ToString(), (i + 1).ToString());

			KeyValuePair<string, string>[] a = new KeyValuePair<string, string>[1];
			((ICollection<KeyValuePair<string, string>>)dict).CopyTo(a, 0);
		}

		[Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void CopyTo_IndexOutOfRange()
		{
			var dict = new OrderedDictionary<string, string>();

			((ICollection<KeyValuePair<string, string>>)dict).CopyTo(new KeyValuePair<string, string>[10], -1);
		}

		[Test]
		public void Values_CopyTo()
		{
			var dict = new OrderedDictionary<uint, int>();
			dict.Add(1, 1);
			dict.Add(2, 2);
			dict.Add(3, 3);
			dict.Remove(2);
			dict.Add(4, 4);

			int[] a = new int[13];

			dict.Values.CopyTo(a, 10);

			for (int i = 0; i < 10; ++i)
			{
				if (i < 10)
					Assert.AreEqual(default(int), a[i]);
			}

			Assert.AreEqual(1, a[10]);
			Assert.AreEqual(3, a[11]);
			Assert.AreEqual(4, a[12]);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ValuesCopyTo_NullArray()
		{
			var dict = new OrderedDictionary<string, string>();
			string[] a = null;

			dict.Values.CopyTo(a, 0);
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void ValuesCopyTo_ArrayTooSmall()
		{
			var dict = new OrderedDictionary<string, string>();
			for (int i = 0; i < 1000; ++i)
				dict.Add(i.ToString(), (i + 1).ToString());

			dict.Values.CopyTo(new string[1], 0);
		}

		[Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void ValuesCopyTo_IndexOutOfRange()
		{
			var dict = new OrderedDictionary<string, string>();

			dict.Values.CopyTo(new string[1], -1);
		}

		[Test]
		public void IsReadOnly()
		{
			Assert.IsFalse(((ICollection<KeyValuePair<int, int>>)new OrderedDictionary<int, int>()).IsReadOnly);
		}

		[Test]
		public void Values_IsReadOnly()
		{
			Assert.IsTrue(new OrderedDictionary<int, int>().Values.IsReadOnly);
		}

		[Test]
		public void Clear()
		{
			var dict = new OrderedDictionary<int, int> { { 1, 2 }, { 2, 3 }, { 3, 4 }, { 4, 5 } };

			dict.Clear();

			Assert.AreEqual(0, dict.Count);
			Assert.AreEqual(0, dict.Values.Count);
			Assert.IsFalse(dict.ContainsKey(1));
			Assert.IsFalse(dict.ContainsValue(2));
		}

		[Test, ExpectedException(typeof(NotSupportedException))]
		public void Values_Clear()
		{
			var dict = new OrderedDictionary<int, int>();
			dict.Values.Clear();
		}

		[Test]
		public void Add()
		{
			var dict = new OrderedDictionary<string, int>();
			dict.Add("1", 2);
			dict.Add("2", 3);

			Assert.AreEqual(dict[0], 2);
			Assert.AreEqual(dict[1], 3);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void Add_KeyNull()
		{
			var dict = new OrderedDictionary<string, int>();
			dict.Add (null, 1);
		}

		[Test, ExpectedException (typeof (ArgumentException))]
		public void Add_KeyExists()
		{
			var dict = new OrderedDictionary<string, int>();
			dict.Add ("foo", 0);
			dict.Add ("foo", 1);
		}

		[Test]
		public void KVP_Add()
		{
			var dict = new OrderedDictionary<uint, int>();
			((ICollection<KeyValuePair<uint, int>>)dict).Add(new KeyValuePair<uint, int>(1, 1));
			((ICollection<KeyValuePair<uint, int>>)dict).Add(new KeyValuePair<uint, int>(2, 2));
			((ICollection<KeyValuePair<uint, int>>)dict).Add(new KeyValuePair<uint, int>(3, 3));
			((ICollection<KeyValuePair<uint, int>>)dict).Remove(new KeyValuePair<uint, int>(2, 2));
			((ICollection<KeyValuePair<uint, int>>)dict).Add(new KeyValuePair<uint, int>(4, 4));

			Assert.AreEqual (3, dict.Count);
			Assert.AreEqual (dict[0], 1);
			Assert.AreEqual (dict[1], 3);
			Assert.AreEqual (dict[2], 4);
		}

		[Test, ExpectedException(typeof(NotSupportedException))]
		public void Values_Add()
		{
			var dict = new OrderedDictionary<string, int>();
			dict.Values.Add(1);
		}

		[Test]
		public void Insert()
		{
			var dict = new OrderedDictionary<string, int>();
			dict.Add ("1", 2);
			dict.Add ("3", 4);

			dict.Insert (1, "2", 3);

			Assert.AreEqual (dict[0], 2);
			Assert.AreEqual (dict[1], 3);
			Assert.AreEqual (dict[2], 4);
		}

		[Test, ExpectedException (typeof (ArgumentException))]
		public void Insert_KeyExists()
		{
			var dict = new OrderedDictionary<string, int>();
			dict.Add("1", 2);
			dict.Add("3", 4);

			dict.Insert(1, "3", 3);
		}

		[Test]
		public void KVP_Insert()
		{
			var dict = new OrderedDictionary<string, int>();
			dict.Add("1", 2);
			dict.Add("3", 4);

			((IList<KeyValuePair<string, int>>)dict).Insert (1,
				new KeyValuePair<string, int> ("2", 3));

			Assert.AreEqual(dict[0], 2);
			Assert.AreEqual(dict[1], 3);
			Assert.AreEqual(dict[2], 4);
		}

		[Test, ExpectedException (typeof (NotSupportedException))]
		public void Values_Insert()
		{
			var dict = new OrderedDictionary<string, int>();
			((IList<int>) dict.Values).Insert (1, 1);
		}

		[Test]
		public void Remove()
		{
			var dict = new OrderedDictionary<string, int> { { "1", 2 }, { "2", 3 }, { "3", 4 } };

			Assert.IsTrue(dict.Remove("2"));
			Assert.IsFalse(dict.ContainsKey("2"));
			Assert.IsFalse(dict.Values.Contains(3));
			Assert.AreEqual(dict[1], 4);

			Assert.IsFalse(dict.Remove("2"));
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void Remove_KeyNull()
		{
			var dict = new OrderedDictionary<string, int>();
			dict.Remove (null);
		}

		[Test]
		public void KVP_Remove()
		{
			var dict = new OrderedDictionary<string, string>();
			dict.Add ("foo", "bar");

			var kvp = new KeyValuePair<string, string>("foo", "bar");
			Assert.IsTrue (((ICollection<KeyValuePair<string, string>>)dict).Remove (kvp));
			Assert.AreEqual (0, dict.Count);
		}

		[Test, ExpectedException(typeof(NotSupportedException))]
		public void Values_Remove()
		{
			var dict = new OrderedDictionary<string, int>();
			dict.Values.Remove(1);
		}

		[Test]
		public void ContainsKey()
		{
			var dict = new OrderedDictionary<string, int> { { "1", 2 }, { "2", 3 }, { "3", 4 } };

			Assert.IsFalse(dict.ContainsKey("0"));
			Assert.IsTrue(dict.ContainsKey("1"));
			Assert.IsTrue(dict.ContainsKey("2"));
			Assert.IsTrue(dict.ContainsKey("3"));
			Assert.IsFalse(dict.ContainsKey("4"));
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void ContainsKey_KeyNull()
		{
			var dict = new OrderedDictionary<string, int>();
			dict.ContainsKey (null);
		}

		[Test]
		public void ContainsValue()
		{
			var dict = new OrderedDictionary<string, int> { { "1", 2 }, { "2", 3 }, { "3", 4 } };

			Assert.IsFalse(dict.ContainsValue(1));
			Assert.IsTrue(dict.ContainsValue(2));
			Assert.IsTrue(dict.ContainsValue(3));
			Assert.IsTrue(dict.ContainsValue(4));
			Assert.IsFalse(dict.ContainsValue(5));
		}

		[Test]
		public void KVP_Contains()
		{
			var dict = new OrderedDictionary<string, int> { { "1", 2 }, { "2", 3 }, { "3", 4 } };
			var co = (ICollection<KeyValuePair<string, int>>) dict;

			Assert.IsFalse (co.Contains (new KeyValuePair<string, int>("0", 1)));
			Assert.IsTrue (co.Contains(new KeyValuePair<string, int>("1", 2)));
			Assert.IsTrue(co.Contains(new KeyValuePair<string, int>("2", 3)));
			Assert.IsTrue(co.Contains(new KeyValuePair<string, int>("3", 4)));
			Assert.IsFalse (co.Contains(new KeyValuePair<string, int>("4", 5)));
		}

		[Test]
		public void Values_Contains()
		{
			var dict = new OrderedDictionary<string, int> { { "1", 2 }, { "2", 3 }, { "3", 4 } };

			Assert.IsFalse(dict.Values.Contains(1));
			Assert.IsTrue(dict.Values.Contains(2));
			Assert.IsTrue(dict.Values.Contains(3));
			Assert.IsTrue(dict.Values.Contains(4));
			Assert.IsFalse(dict.Values.Contains(5));
		}

		[Test]
		public void Count()
		{
			var dict = new OrderedDictionary<string, int> { { "1", 2 }, { "2", 3 }, { "3", 4 } };

			Assert.AreEqual(3, dict.Count);

			dict.Add("4", 5);
			dict.Add("5", 6);

			Assert.AreEqual(5, dict.Count);

			dict.Clear();

			Assert.AreEqual(0, dict.Count);
		}

		[Test]
		public void Values_Count()
		{
			var dict = new OrderedDictionary<string, int> { { "1", 2 }, { "2", 3 }, { "3", 4 } };

			Assert.AreEqual(3, dict.Values.Count);

			dict.Add("4", 5);
			dict.Add("5", 6);

			Assert.AreEqual(5, dict.Values.Count);

			dict.Clear();

			Assert.AreEqual(0, dict.Values.Count);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void TryGetValue_NullKey()
		{
			var dict = new OrderedDictionary<string, int>();

			int i;
			dict.TryGetValue (null, out i);
		}

		[Test]
		public void TryGetValue()
		{
			var dict = new OrderedDictionary<string, int> { { "1", 2 }, { "2", 3 }, { "3", 4 } };

			int v;
			Assert.IsTrue(dict.TryGetValue("1", out v));
			Assert.AreEqual(2, v);
		}

		[Test]
		public void TryGetValue_NotFound()
		{
			var dict = new OrderedDictionary<string, int> { { "1", 2 }, { "2", 3 }, { "3", 4 } };

			int v;
			Assert.IsFalse(dict.TryGetValue("4", out v));
		}

		[Test]
		public void RemoveAt()
		{
			var dict = new OrderedDictionary<uint, int>();
			dict.Add(1, 1);
			dict.Add(2, 2);
			dict.Add(3, 3);
			dict.Remove(2);
			dict.Add(4, 4);

			dict.RemoveAt (1);

			Assert.AreEqual(1, dict[(int)0]);
			Assert.AreEqual(4, dict[(int)1]);
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void RemoveAt_IndexOutOfRangeLower()
		{
			var dict = new OrderedDictionary<string, int>();
			dict.Add("foo", 0);
			dict.Add("bar", 1);
			dict.Add("baz", 2);

			dict.RemoveAt (-1);
		}

		[Test, ExpectedException (typeof (NotSupportedException))]
		public void Values_RemoveAt()
		{
			var dict = new OrderedDictionary<string, int>();
			((IList<int>)dict.Values).RemoveAt (0);
		}

		[Test]
		public void IndexOf()
		{
			var dict = new OrderedDictionary<string, int>();
			dict.Add ("foo", 0);
			dict.Add ("bar", 1);

			Assert.AreEqual (1, dict.IndexOf ("bar"));
		}

		[Test]
		public void IndexOf_NotFound()
		{
			var dict = new OrderedDictionary<string, int>();
			dict.Add("foo", 0);
			dict.Add("bar", 1);

			Assert.AreEqual(-1, dict.IndexOf("baz"));
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void IndexOf_KeyNull()
		{
			var dict = new OrderedDictionary<string, int>();
			dict.Add("foo", 0);
			dict.Add("bar", 1);

			dict.IndexOf (null);
		}

		[Test]
		public void IndexOf_StartIndex()
		{
			var dict = new OrderedDictionary<string, int>();
			dict.Add("foo", 0);
			dict.Add("bar", 1);
			dict.Add ("baz", 2);
			dict.Add ("monkeys", 3);

			Assert.AreEqual(2, dict.IndexOf("baz", 1));
			Assert.AreEqual(2, dict.IndexOf("baz", 2));
		}

		[Test]
		public void IndexOf_StartIndex_NotFound()
		{
			var dict = new OrderedDictionary<string, int>();
			dict.Add("foo", 0);
			dict.Add("bar", 1);
			dict.Add("baz", 2);
			dict.Add("monkeys", 3);

			Assert.AreEqual (-1, dict.IndexOf ("asdf", 2));
			Assert.AreEqual (-1, dict.IndexOf ("bar", 2));
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void IndexOf_StartIndex_KeyNull()
		{
			var dict = new OrderedDictionary<string, int>();
			dict.Add("foo", 0);
			dict.Add("bar", 1);
			dict.Add("baz", 2);
			dict.Add("monkeys", 3);

			dict.IndexOf (null, 1);
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void IndexOf_StartIndex_IndexOutOfRangeLower()
		{
			var dict = new OrderedDictionary<string, int>();
			dict.Add("foo", 0);
			dict.Add("bar", 1);
			dict.Add("baz", 2);
			dict.Add("monkeys", 3);

			dict.IndexOf("monkeys", -1);
		}

		[Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void IndexOf_StartIndex_IndexOutOfRangeUpper()
		{
			var dict = new OrderedDictionary<string, int>();
			dict.Add("foo", 0);
			dict.Add("bar", 1);
			dict.Add("baz", 2);
			dict.Add("monkeys", 3);

			dict.IndexOf("monkeys", 5);
		}

		[Test]
		public void IndexOf_StartIndexAndCount()
		{
			var dict = new OrderedDictionary<string, int>();
			dict.Add("foo", 0);
			dict.Add("bar", 1);
			dict.Add("baz", 2);
			dict.Add("monkeys", 3);

			Assert.AreEqual(1, dict.IndexOf("bar", 1, 1));
			Assert.AreEqual(2, dict.IndexOf("baz", 0, 3));
		}

		[Test]
		public void IndexOf_StartIndexAndCount_NotFound()
		{
			var dict = new OrderedDictionary<string, int>();
			dict.Add("foo", 0);
			dict.Add("bar", 1);
			dict.Add("baz", 2);
			dict.Add("monkeys", 3);

			Assert.AreEqual(-1, dict.IndexOf("bar", 2, 1));
			Assert.AreEqual(-1, dict.IndexOf("baz", 0, 2));
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void IndexOf_StartIndexAndCount_KeyNull()
		{
			var dict = new OrderedDictionary<string, int>();
			dict.Add("foo", 0);
			dict.Add("bar", 1);
			dict.Add("baz", 2);
			dict.Add("monkeys", 3);

			dict.IndexOf (null, 1, 2);
		}

		[Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void IndexOf_StartIndexAndCount_IndexOutOfRangeLower()
		{
			var dict = new OrderedDictionary<string, int>();
			dict.Add("foo", 0);
			dict.Add("bar", 1);
			dict.Add("baz", 2);
			dict.Add("monkeys", 3);

			dict.IndexOf("monkeys", -1, 1);
		}

		[Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void IndexOf_StartIndexAndCount_IndexOutOfRangeUpper()
		{
			var dict = new OrderedDictionary<string, int>();
			dict.Add("foo", 0);
			dict.Add("bar", 1);
			dict.Add("baz", 2);
			dict.Add("monkeys", 3);

			dict.IndexOf("monkeys", 5, 1);
		}

		[Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void IndexOf_StartIndexAndCount_CountOutOfRange()
		{
			var dict = new OrderedDictionary<string, int>();
			dict.Add("foo", 0);
			dict.Add("bar", 1);
			dict.Add("baz", 2);
			dict.Add("monkeys", 3);

			dict.IndexOf("monkeys", 2, 3);
		}
	}

	[TestFixture]
	public class OrderedDictionaryListContractTests : ListContract<KeyValuePair<string, string>> {
		protected override ICollection<KeyValuePair<string, string>> CreateCollection (IEnumerable<KeyValuePair<string, string>> values)
		{
			var d = new OrderedDictionary<string, string> ();
			foreach (var v in values)
				d.Add (v.Key, v.Value);
			return d;
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
	public class OrderedDictionaryDictionaryContractTests : DictionaryContract {
		protected override IDictionary<string, string> CreateDictionary (IEnumerable<KeyValuePair<string, string>> values)
		{
			var d = new OrderedDictionary<string, string> ();
			foreach (var v in values)
				d.Add (v.Key, v.Value);
			return d;
		}
	}
}
