//
// BidirectionalDictionaryTest.cs
//
// Author:
//   Chris Chilvers <chilversc@googlemail.com>
//
// Copyright (c) 2009 Chris Chilvers
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

using Cadenza.Tests;

namespace Cadenza.Collections.Tests
{
	[TestFixture]
	public class BidirectionalDictionaryTests : BaseRocksFixture
	{
		private const string String1 = "Arthur Dent";
		private const string String2 = "Ford Perfect";


		[Test]
		public void Ctor_NegativeCapacity ()
		{
			var ex = Assert.Throws<ArgumentOutOfRangeException> (
				() => new BidirectionalDictionary<string, string> (-1));

			Assert.AreEqual ("capacity", ex.ParamName);
			Assert.AreEqual (-1, ex.ActualValue);

			Assert.DoesNotThrow (() => new BidirectionalDictionary<string, string> (0));
			Assert.DoesNotThrow (() => new BidirectionalDictionary<string, string> (1));
		}


		[Test]
		public void ContainsKey ()
		{
			var dictionary = new BidirectionalDictionary<string, string> ();
			dictionary.Add (String1, String2);
			Assert.IsTrue (dictionary.ContainsKey (String1));
		}

		[Test]
		public void ContainsKey_Null ()
		{
			var dictionary = new BidirectionalDictionary<string, string> ();
			Assert.Throws<ArgumentNullException>(() => dictionary.ContainsKey (null));
		}

		[Test]
		public void ContainsKey_NotFound ()
		{
			var dictionary = new BidirectionalDictionary<string, string> ();
			Assert.IsFalse (dictionary.ContainsKey (""));
		}

		[Test]
		public void ContainsValue ()
		{
			var dictionary = new BidirectionalDictionary<string, string> ();
			dictionary.Add (String1, String2);
			Assert.IsTrue (dictionary.ContainsValue (String2));
		}

		[Test]
		public void ContainsValue_Null ()
		{
			var dictionary = new BidirectionalDictionary<string, string> ();
			Assert.Throws<ArgumentNullException> (() => dictionary.ContainsValue (null));
		}

		[Test]
		public void ContainsValue_NotFound ()
		{
			var dictionary = new BidirectionalDictionary<string, string> ();
			Assert.IsFalse (dictionary.ContainsValue (""));
		}


		[Test]
		public void TryGetKey ()
		{
			var dictionary = new BidirectionalDictionary<string, string> ();
			dictionary.Add (String1, String2);

			string value;
			Assert.IsTrue(dictionary.TryGetKey (String2, out value));
			Assert.AreEqual (String1, value);
		}

		[Test]
		public void TryGetKey_Null ()
		{
			var dictionary = new BidirectionalDictionary<string, string> ();

			string value;
			var ex = Assert.Throws<ArgumentNullException> (() => dictionary.TryGetKey (null, out value));
			Assert.AreEqual ("value", ex.ParamName);
		}

		[Test]
		public void TryGetKey_NotFound ()
		{
			var dictionary = new BidirectionalDictionary<string, string> ();

			string value = String2;
			Assert.IsFalse (dictionary.TryGetKey (String1, out value));
			Assert.IsNull (value);
		}

		[Test]
		public void TryGetValue ()
		{
			var dictionary = new BidirectionalDictionary<string, string> ();
			dictionary.Add (String1, String2);

			string value;
			Assert.IsTrue (dictionary.TryGetValue (String1, out value));
			Assert.AreEqual (String2, value);
		}

		[Test]
		public void TryGetValue_Null ()
		{
			var dictionary = new BidirectionalDictionary<string, string> ();

			string value;
			var ex = Assert.Throws<ArgumentNullException> (() => dictionary.TryGetValue (null, out value));
			Assert.AreEqual ("key", ex.ParamName);
		}

		[Test]
		public void TryGetValue_NotFound ()
		{
			var dictionary = new BidirectionalDictionary<string, string> ();

			string value = String2;
			Assert.IsFalse (dictionary.TryGetValue (String1, out value));
			Assert.IsNull (value);
		}


		[Test]
		public void Item_Get ()
		{
			var dictionary = new BidirectionalDictionary<string, string> ();
			dictionary.Add (String1, String2);
			Assert.AreEqual (String2, dictionary [String1]);
		}

		[Test]
		public void Item_Get_Null ()
		{
			var dictionary = new BidirectionalDictionary<string, string> ();
			var ex = Assert.Throws<ArgumentNullException> (() => { var x = dictionary [null]; Ignore (x); });
			Assert.AreEqual ("key", ex.ParamName);
		}

		[Test]
		public void Item_Get_NotFound ()
		{
			var dictionary = new BidirectionalDictionary<string, string> ();
			Assert.Throws<KeyNotFoundException> (() => { var x = dictionary [String1]; Ignore (x); });
		}

		[Test]
		public void Item_Set ()
		{
			var dictionary = new BidirectionalDictionary<string, string> ();
			dictionary [String1] = String2;
			Assert.AreEqual (String2, dictionary [String1]);
		}

		[Test]
		public void Item_Set_NullKey ()
		{
			var dictionary = new BidirectionalDictionary<string, string> ();
			var ex = Assert.Throws<ArgumentNullException> (() => dictionary [null] = String2);
			Assert.AreEqual ("key", ex.ParamName);
		}

		[Test]
		public void Item_Set_NullValue ()
		{
			var dictionary = new BidirectionalDictionary<string, string> ();
			var ex = Assert.Throws<ArgumentNullException> (() => dictionary [String1] = null);
			Assert.AreEqual ("value", ex.ParamName);
		}

		[Test]
		public void Item_Set_ExistingKey ()
		{
			var dictionary = new BidirectionalDictionary<int, string> ();
			dictionary.Add (42, String1);
			dictionary [42] = String2;
			Assert.AreEqual (String2, dictionary [42]);
			Assert.AreEqual (42, dictionary.Inverse [String2]);
			Assert.Throws<KeyNotFoundException> (() => dictionary.Inverse [String1].ToString ());
		}

		[Test]
		public void Item_Set_EquivalentExistingKey ()
		{
			var dictionary = new BidirectionalDictionary<Foo, int> ();

			var key1 = new Foo { Name = "key", Value = "1" };
			var key2 = new Foo { Name = "key", Value = "2" };
			var value1 = 42;
			var value2 = 67;

			dictionary.Add (key1, value1);
			dictionary [key2] = value2;

			Assert.AreSame (key1, dictionary.Keys.First ());
			Assert.AreSame (key1, dictionary.Inverse [value2]);
			Assert.AreEqual (value2, dictionary [key1]);
			Assert.AreEqual (value2, dictionary [key2]);
			Assert.Throws<KeyNotFoundException> (() => dictionary.Inverse [value1].ToString ());
		}

		[Test]
		public void Item_Set_ExistingValue ()
		{
			var dictionary = new BidirectionalDictionary<int, string> ();
			dictionary.Add (42, String1);
			var ex = Assert.Throws<ArgumentException> (() => dictionary [67] = String1);
			Assert.AreEqual ("value", ex.ParamName);
		}

		[Test]
		public void Item_Set_EquivalentExistingValue ()
		{
			var dictionary = new BidirectionalDictionary<int, Foo> ();

			var key1 = 42;
			var key2 = 67;
			var value1 = new Foo { Name = "value", Value = "1" };
			var value2 = new Foo { Name = "value", Value = "2" };

			dictionary.Add (key1, value1);

			var ex = Assert.Throws<ArgumentException> (() => dictionary [key2] = value2);
			Assert.AreEqual ("value", ex.ParamName);
		}

		[Test]
		public void Item_Set_KeyToOwnValue ()
		{
			var dictionary = new BidirectionalDictionary<int, string> ();
			dictionary.Add (42, String1);
			dictionary [42] = String1;
			Assert.AreEqual (String1, dictionary [42]);
		}

		[Test]
		public void Item_Set_KeyToNewValue ()
		{
			var dictionary = new BidirectionalDictionary<string, string> ();
			dictionary.Add ("key", "value1");
			dictionary ["key"] = "value2";

			Assert.AreEqual ("value2", dictionary["key"]);
			Assert.AreEqual ("key", dictionary.Inverse["value2"]);
			Assert.Throws<KeyNotFoundException> (() => dictionary.Inverse ["value1"].ToString ());
		}

		[Test]
		public void Item_Set_KeyToEquivalentValue ()
		{
			var dictionary = new BidirectionalDictionary<int, Foo> ();

			var key = 42;
			var value1 = new Foo { Name = "value", Value = "1" };
			var value2 = new Foo { Name = "value", Value = "2" };

			dictionary.Add (key, value1);
			dictionary [key] = value2;

			Assert.AreSame (value2, dictionary [key]);
			Assert.AreSame (value2, dictionary.Inverse.Keys.First ());
		}


		[Test]
		public void Add ()
		{
			var dictionary = new BidirectionalDictionary<int, string> ();
			dictionary.Add (42, String1);
			Assert.That (dictionary.ContainsKey (42));
			Assert.That (dictionary.ContainsValue (String1));
			Assert.That (dictionary.Count, Is.EqualTo (1));
		}

		[Test]
		public void Add_NullKey ()
		{
			var dictionary = new BidirectionalDictionary<string, string> ();
			var ex = Assert.Throws<ArgumentNullException> (() => dictionary.Add (null, String1));
			Assert.AreEqual ("key", ex.ParamName);
		}

		[Test]
		public void Add_NullValue ()
		{
			var dictionary = new BidirectionalDictionary<string, string> ();
			var ex = Assert.Throws<ArgumentNullException> (() => dictionary.Add (String1, null));
			Assert.AreEqual ("value", ex.ParamName);
		}

		[Test]
		public void Add_DuplicateKey ()
		{
			var dictionary = new BidirectionalDictionary<int, string> ();
			dictionary.Add (42, String1);
			var ex = Assert.Throws<ArgumentException> (() => dictionary.Add (42, String2));
			Assert.AreEqual ("key", ex.ParamName);
		}

		[Test]
		public void Add_DuplicateValue ()
		{
			var dictionary = new BidirectionalDictionary<int, string> ();
			dictionary.Add (42, String1);
			var ex = Assert.Throws<ArgumentException> (() => dictionary.Add (67, String1));
			Assert.AreEqual ("value", ex.ParamName);
		}


		[Test]
		public void Replace ()
		{
			var dictionary = new BidirectionalDictionary<int, string> ();
			dictionary.Replace (42, String1);
			Assert.AreEqual (String1, dictionary [42]);
		}

		[Test]
		public void Replace_NullKey ()
		{
			var dictionary = new BidirectionalDictionary<string, string> ();
			var ex = Assert.Throws<ArgumentNullException> (() => dictionary.Replace (null, String1));
			Assert.AreEqual ("key", ex.ParamName);
		}

		[Test]
		public void Replace_NullValue ()
		{
			var dictionary = new BidirectionalDictionary<string, string> ();
			var ex = Assert.Throws<ArgumentNullException> (() => dictionary.Replace (String1, null));
			Assert.AreEqual ("value", ex.ParamName);
		}

		[Test]
		public void Replace_ExistingKey ()
		{
			var dictionary = new BidirectionalDictionary<int, string> ();
			dictionary.Add (42, String1);
			dictionary.Replace (42, String2);
			Assert.AreEqual (String2, dictionary [42]);
		}

		[Test]
		public void Replace_ExistingValue ()
		{
			var dictionary = new BidirectionalDictionary<int, string> ();
			dictionary.Add (42, String1);
			dictionary.Replace (67, String2);
			Assert.AreEqual (String2, dictionary [67]);
		}

		[Test]
		public void Replace_ExistingKeyAndValue ()
		{
			var dictionary = new BidirectionalDictionary<int, string> ();
			dictionary.Add (42, String1);
			dictionary.Add (67, String2);
			dictionary.Replace (42, String2);
			
			Assert.That (!dictionary.ContainsKey (67), "Did not remove key 67");
			Assert.That (!dictionary.ContainsValue (String1), "Did not remove value String1");
			Assert.That (dictionary [42], Is.EqualTo (String2), "Did not add mapping 42 -> String2");
		}


		[Test]
		public void Remove ()
		{
			var dictionary = new BidirectionalDictionary<int, string> ();
			dictionary.Add (42, String1);
			Assert.IsTrue (dictionary.Remove (42));
			Assert.IsFalse (dictionary.ContainsKey (42));
		}

		[Test]
		public void Remove_Null ()
		{
			var dictionary = new BidirectionalDictionary<string, string> ();
			var ex = Assert.Throws<ArgumentNullException> (() => dictionary.Remove (null));
			Assert.AreEqual ("key", ex.ParamName);
		}

		[Test]
		public void Remove_NotFound ()
		{
			var dictionary = new BidirectionalDictionary<int, string> ();
			Assert.IsFalse (dictionary.Remove (42));
		}


		[Test]
		public void Clear ()
		{
			var dictionary = new BidirectionalDictionary<string, string> ();
			dictionary.Add (String1, String2);
			dictionary.Clear ();
			Assert.AreEqual (0, dictionary.Count);
		}


		[Test]
		public void ModifyingInverseUpdatesOriginal ()
		{
			var original = new BidirectionalDictionary<int, string> ();
			var inverse = original.Inverse;
			inverse.Add (String1, 42);
			Assert.That (original.ContainsKey (42));
			Assert.That (original [42], Is.EqualTo (String1));
		}

		[Test]
		public void ModifyingOriginalUpdatesInverse ()
		{
			var original = new BidirectionalDictionary<int, string> ();
			var inverse = original.Inverse;
			original.Add (42, String1);
			Assert.That (inverse.ContainsKey (String1));
			Assert.That (inverse [String1], Is.EqualTo (42));
		}

		[Test]
		public void KeyAndValueCollectionsAreReadOnly ()
		{
			var dictionary = new BidirectionalDictionary<int, string> ();
			Assert.That (dictionary.Keys.IsReadOnly);
			Assert.That (dictionary.Values.IsReadOnly);
		}


		class Foo : IEquatable<Foo>
		{
			public string Name;
			public string Value;

			public override int GetHashCode ()
			{
				return Name.GetHashCode();
			}

			public bool Equals (Foo other)
			{
				if (ReferenceEquals (null, other))
					return false;
				if (ReferenceEquals (this, other))
					return true;

				return Name == other.Name;
			}

			public override bool Equals (object obj)
			{
				var f = (obj as Foo);
				if (f != null)
					return Name == f.Name;

				return false;
			}

			public override string ToString()
			{
				return string.Format("Name = {0}, Value = {1}", Name, Value);
			}
		}
	}

	[TestFixture]
	public class BidirectionalDictionaryCollectionContractTests : CollectionContract<KeyValuePair<string, string>> {
		protected override ICollection<KeyValuePair<string, string>> CreateCollection (IEnumerable<KeyValuePair<string, string>> values)
		{
			var d = new BidirectionalDictionary<string, string> ();
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
	public class BidirectionalDictionaryDictionaryContractTests : DictionaryContract {
		protected override IDictionary<string, string> CreateDictionary (IEnumerable<KeyValuePair<string, string>> values)
		{
			var d = new BidirectionalDictionary<string, string> ();
			foreach (var v in values)
				d.Add (v.Key, v.Value);
			return d;
		}
	}
}
