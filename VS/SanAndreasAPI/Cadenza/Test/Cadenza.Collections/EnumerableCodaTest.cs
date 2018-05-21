//
// IEnumerableTest.cs
//
// Author:
//   Jb Evain (jbevain@novell.com)
//   Jonathan Pryor (jonp@xamarin.com)
//
// Copyright (c) 2007-2010 Novell, Inc. (http://www.novell.com)
// Copyright (c) 2011 Xamarin Inc. (http://xamarin.com)
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

using Cadenza;
using Cadenza.Collections;
using Cadenza.Tests;
using Cadenza.Numerics.Tests;

// "The variable `r' is assigned but it's value is never used."
// It's value isn't supposed to be used; it's purpose is as a manual check the
// the generated .Curry() methods generate the correct return type.
#pragma warning disable 0219

namespace Cadenza.Collections.Tests {

	[TestFixture]
	public class EnumerableTest : BaseRocksFixture {

		[Test]
		[ExpectedException (typeof(ArgumentNullException))]
		public void TryGetFirst_SelfNull ()
		{
			int oi;
			IEnumerable<int> e = null;
			e.TryGetFirst (i => i > 0, out oi);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TryGetFirst_PredicateNull ()
		{
			int i;
			IEnumerable<int> e = Enumerable.Empty<int> ();
			e.TryGetFirst (null, out i);
		}

		[Test]
		public void TryGetFirst_Predicate ()
		{
			int oi;
			var e = Enumerable.Range (0, 10);

			Assert.IsTrue (e.TryGetFirst (i => i > 5, out oi));
			Assert.AreEqual (6, oi);
		}

		[Test]
		public void TryGetFirst_NotFound ()
		{
			int oi;
			var e = Enumerable.Range (1, 10);

			Assert.IsFalse (e.TryGetFirst (i => i > 10, out oi));
			Assert.AreEqual (default(int), oi);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TryGetFirstNoPredicate_Null()
		{
			int oi;
			IEnumerable<int> e = null;

			e.TryGetFirst (out oi);
		}

		[Test]
		public void TryGetFirstNoPredicate_List ()
		{
			int oi;
			var e = Enumerable.Range (1, 10).ToList();

			Assert.IsTrue (e.TryGetFirst (out oi));
			Assert.AreEqual (1, oi);
		}

		[Test]
		public void TryGetFirstNoPredicate_List_NotFound()
		{
			int oi;
			var e = Enumerable.Empty<int>().ToList();

			Assert.IsFalse (e.TryGetFirst (out oi));
			Assert.AreEqual (default(int), oi);
		}

		[Test]
		public void TryGetFirstNoPredicate()
		{
			int oi;
			var e = Enumerable.Range (1, 10);

			Assert.IsTrue (e.TryGetFirst (out oi));
			Assert.AreEqual (1, oi);
		}

		[Test]
		public void TryGetFirstNoPredicate_NotFound()
		{
			int oi;
			var e = Enumerable.Empty<int>();

			Assert.IsFalse (e.TryGetFirst (out oi));
			Assert.AreEqual (default(int), oi);
		}

		[Test]
		public void TryGetFirst ()
		{
			#region TryGetFirst
			var seq = new int[]{0, 1, 2};
			int first;

			Assert.IsTrue (seq.TryGetFirst (out first));
			Assert.IsTrue (0 == first);

			seq = new int[]{};
			Assert.IsFalse (seq.TryGetFirst (out first));
			Assert.IsTrue (0 == first);
			#endregion
		}

		[Test]
		public void TryGetFirst2 ()
		{
			#region TryGetFirst2
			var seq = new int[]{0, 1, 2};
			int first;

			Assert.IsTrue (seq.TryGetFirst (v => v.IsOdd (), out first));
			Assert.IsTrue (1 == first);

			Assert.IsTrue (seq.TryGetFirst (v => v.IsEven (), out first));
			Assert.IsTrue (0 == first);

			Assert.IsFalse (seq.TryGetFirst (v => v == 5, out first));
			Assert.IsTrue (0 == first);
			#endregion
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Implode_SourceNull ()
		{
			IEnumerable<int> e = null;
			e.Implode ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Implode_SelectorNull ()
		{
			IEnumerable<int> e = new int[0];
			Func<int, string> f = null;
			e.Implode (null, f);
		}

		[Test]
		public void Implode ()
		{
			var data = new [] { 0, 1, 2, 3, 4, 5 };
			var result = "0, 1, 2, 3, 4, 5";

			Assert.AreEqual ("", new string[]{}.Implode (", "));
			Assert.AreEqual (result, data.Implode (", "));
			Assert.AreEqual ("012345", data.Implode ());
			Assert.AreEqual (
					"'foo', 'bar'",
					new[]{"foo", "bar"}.Implode (", ", e => "'" + e + "'"));
		}

		[Test]
		public void ImplodeEmpty ()
		{
			var data = new int [] {};

			Assert.AreEqual (string.Empty, data.Implode ());
		}

		[Test]
		public void Repeat ()
		{
			#region Repeat
			Assert.AreEqual ("foofoofoo", new [] {"foo"}.Repeat (3).Implode ());
			Assert.AreEqual ("foobarfoobar", new [] {"foo", "bar"}.Repeat (2).Implode ());
			#endregion
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Repeat_SelfNull ()
		{
			IEnumerable<string> e = null;
			e.Repeat (0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void PathCombine_SelfNull ()
		{
			string [] data = null;
			data.PathCombine ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void PathCombine_Null ()
		{
			var data = new [] { "a", "b", null, "c" };
			data.PathCombine ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void PathCombine_InvalidPathChars ()
		{
			// 0x00 (NULL) is the only cross-platform invalid path character
			var data = new [] { "a", "b\x00", "c" };
			data.PathCombine ();
		}

		[Test]
		public void PathCombine ()
		{
			#region PathCombine
			var data = new [] {"a", "b", "c"};
			var result = string.Format ("a{0}b{0}c", Path.DirectorySeparatorChar);
			Assert.AreEqual (result, data.PathCombine ());

			data = new [] { "a", String.Empty, "b", "c" };
			Assert.AreEqual (result, data.PathCombine (), "empty elemetns");

			string rooted = Path.DirectorySeparatorChar + "d";
			data = new [] { "a", rooted };
			Assert.AreEqual (rooted, data.PathCombine (), "rooted path2");

			data = new [] { "a", "b", rooted, "c" };
			string expected = Path.Combine (Path.Combine (Path.Combine ("a", "b"), rooted), "c");
			Assert.AreEqual (expected, data.PathCombine (), "rooted path2 (complex)");

			string end1 = "d" + Path.DirectorySeparatorChar;
			data = new [] { rooted, end1, "e" };
			expected = Path.Combine (Path.Combine (rooted, end1), "e");
			Assert.AreEqual (expected, data.PathCombine (), "DirectorySeparatorChar");

			string end2 = "d" + Path.AltDirectorySeparatorChar;
			data = new [] { rooted, end2, "f" };
			expected = Path.Combine (Path.Combine (rooted, end2), "f");
			Assert.AreEqual (expected, data.PathCombine (), "AltDirectorySeparatorChar");

			data = new [] { "a" };
			Assert.AreEqual (Path.Combine ("a", String.Empty), data.PathCombine (), "single string");

			data = new [] { String.Empty };
			Assert.AreEqual (Path.Combine (String.Empty, String.Empty), data.PathCombine (), "single empty string");
			#endregion
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void ForEach_SelfNull ()
		{
			IEnumerable<char> e = null;
			e.ForEach (c => Console.WriteLine (c));
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void ForEach_ActionNull ()
		{
			IEnumerable<char> e = new []{'a'};
			Action<char>      a = null;
			e.ForEach (a);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ForEach2_SelfNull ()
		{
			IEnumerable<char> e = null;
			Action<char, int> a = (c, i) => Console.WriteLine (c);
			e.ForEach (a);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ForEach2_ActionNull ()
		{
			IEnumerable<char> e = new[]{'a'};
			Action<char, int> a = null;
			e.ForEach (a);
		}

		[Test]
		public void ForEach ()
		{
			int count = 0;
			Enumerable.Range (0, 10).ForEach (n => ++count);
			Assert.AreEqual (10, count);

			count = 0;
			int index = 0;
			Enumerable.Range (0, 10).ForEach ((n, i) => {++count; index += i;});
			Assert.AreEqual (10, count);
			Assert.AreEqual (0+1+2+3+4+5+6+7+8+9, index);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Apply_SelfNull ()
		{
			IEnumerable<char> e = null;
			e.Apply ();
		}

		[Test]
		public void Apply ()
		{
			int count = 0;
			"hello".Select (c => ++count).Apply ();
			Assert.AreEqual (5, count);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Each_SelfNull ()
		{
			IEnumerable<char> e = null;
			e.Each (c => Console.WriteLine (c));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Each_ActionNull ()
		{
			IEnumerable<char> e = new []{'a'};
			Action<char>      a = null;
			e.Each (a);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Each2_SelfNull ()
		{
			IEnumerable<char> e = null;
			Action<char, int> a = (c, i) => Console.WriteLine (c);
			e.Each (a);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Each2_ActionNull ()
		{
			IEnumerable<char> e = new[]{'a'};
			Action<char, int> a = null;
			e.Each (a);
		}

		[Test]
		public void Each ()
		{
			int count = 0;
			Enumerable.Range (0, 10).Each (n => ++count).Apply ();
			Assert.AreEqual (10, count);

			count = 0;
			int index = 0;
			Enumerable.Range (0, 10).Each ((n, i) => {++count; index += i;}).Apply ();
			Assert.AreEqual (10, count);
			Assert.AreEqual (0+1+2+3+4+5+6+7+8+9, index);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ApplyPairs_SelfNull ()
		{
			IEnumerable<char> e = null;
			e.ApplyPairs ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ApplyPairs_ActionsNull ()
		{
			IEnumerable<char> e = "a";
			e.ApplyPairs (null);
		}

		[Test]
		public void ApplyPairs ()
		{
			string s = null;
			int n = 0;
			double d = 0;
			"word 10 10.5".Words().ApplyPairs (
					v => s = v,
					v => n = int.Parse (v),
					v => d = double.Parse (v)
			).Apply ();
			Assert.AreEqual ("word", s);
			Assert.AreEqual (10, n);
			Assert.AreEqual (10.5, d);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void Tokens_SelfNull ()
		{
			IEnumerable<int>             s = null;
			Func<int, int, int>          a = (x, y) => x+y;
			Func<int, Tuple<int, int>>  rs = x => Tuple.Create (x, 0);
			Func<int, int, bool>[]    cats = new Func<int, int, bool>[]{
				(p, c) => p + c < 10
			};
			IEnumerable<int>             r = s.Tokens (0, a, rs, cats);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void Tokens_AccumulateNull ()
		{
			IEnumerable<int>             s = new[]{1, 2, 3};
			Func<int, int, int>          a = null;
			Func<int, Tuple<int, int>>  rs = x => Tuple.Create (x, 0);
			Func<int, int, bool>[]    cats = new Func<int, int, bool>[]{
				(p, c) => p + c < 10
			};
			IEnumerable<int>             r = s.Tokens (0, a, rs, cats);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void Tokens_ResultSelectorNull ()
		{
			IEnumerable<int>             s = new[]{1, 2, 3};
			Func<int, int, int>          a = (x, y) => x+y;
			Func<int, Tuple<int, int>>  rs = null;
			Func<int, int, bool>[]    cats = new Func<int, int, bool>[]{
				(p, c) => p + c < 10
			};
			IEnumerable<int>             r = s.Tokens (0, a, rs, cats);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void Tokens_CategoriesNull ()
		{
			IEnumerable<int>             s = new[]{1, 2, 3};
			Func<int, int, int>          a = (x, y) => x+y;
			Func<int, Tuple<int, int>>  rs = x => Tuple.Create (x, 0);
			Func<int, int, bool>[]    cats = null;
			IEnumerable<int>             r = s.Tokens (0, a, rs, cats);
		}

		[Test, ExpectedException (typeof (ArgumentException))]
		public void Tokens_CategoriesEmpty ()
		{
			IEnumerable<int>             s = new[]{1, 2, 3};
			Func<int, int, int>          a = (x, y) => x+y;
			Func<int, Tuple<int, int>>  rs = x => Tuple.Create (x, 0);
			Func<int, int, bool>[]    cats = new Func<int, int, bool>[0];
			IEnumerable<int>             r = s.Tokens (0, a, rs, cats);
		}

		[Test]
		public void Tokens ()
		{
			#region Tokens
			// Turn a sequence of numbers into a new sequence of numbers
			// for which the sum is less than 10.
			IEnumerable<int>  numbers         = new[]{1, 1, 3, 5, 8, 13};
			IEnumerable<int>  sumsLessThan10  = numbers.Tokens (0,
					// accumulate: add the values together.
					// p=previous value, c=current value
					(p, c) => p + c,
					// resultSelector: return the current sum and reset count to 0.
					r => Tuple.Create (r, 0),
					// category: sum is less than 10.
					(p, c) => p + c < 10
			);
			// Notice that the input value of 13 is missing, as it didn't match
			// any category.
			Assert.IsTrue (new[]{5, 5, 8}.SequenceEqual (sumsLessThan10));

			// More "traditional" lexing, with categories as precedence rules
			string expression = " function(value1+value2)  ";
			IEnumerable<string> exprTokens = expression.Tokens ("",
					// accumulate: concatenate the characters together
					(p, c) => p + c,
					r => Tuple.Create (r, ""),
					// category: identifiers: [A-Za-z_][A-Za-z0-9_]*
					(p, c) => p.Length == 0
						? char.IsLetter (c) || c == '_'
						: char.IsLetterOrDigit (c) || c == '_',
					// category: arithmetic operators
					(p, c) => c == '+' || c == '-' || c == '*' || c == '/',
					// category: grouping
					(p, c) => c == '(' || c == ')'
			);
			// Notice that all whitespace has been removed
			Assert.IsTrue (
					new[]{"function", "(", "value1", "+", "value2", ")"}
					.SequenceEqual (exprTokens));
			#endregion
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void ToReadOnlyDictionary_SelfNull_KeySelector ()
		{
			IEnumerable<string>   s  = null;
			Func<string, string>  ks = e => e;
			s.ToReadOnlyDictionary (ks);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void ToReadOnlyDictionary_KeySelectorNull ()
		{
			IEnumerable<string>   s  = new[]{"a"};
			Func<string, string>  ks = null;
			s.ToReadOnlyDictionary (ks);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void ToReadOnlyDictionary_SelfNull_KeySelectorAndValueSelector ()
		{
			IEnumerable<string>   s  = null;
			Func<string, string>  ks = e => e;
			Func<string, string>  vs = e => e;
			s.ToReadOnlyDictionary (ks, vs);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void ToReadOnlyDictionary_KeySelectorNull_WithValueSelector ()
		{
			IEnumerable<string>   s  = new[]{"a"};
			Func<string, string>  ks = null;
			Func<string, string>  vs = e => e;
			s.ToReadOnlyDictionary (ks, vs);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void ToReadOnlyDictionary_ValueSelectorNull ()
		{
			IEnumerable<string>   s  = new[]{"a"};
			Func<string, string>  ks = e => e;
			Func<string, string>  vs = null;
			s.ToReadOnlyDictionary (ks, vs);
		}

		[Test]
		public void ToReadOnlyDictionary ()
		{
			var c = new[]{
				new DateTime (2009, 1, 1), 
				new DateTime (2008, 1, 1),
				new DateTime (2007, 1, 1),
			}.ToReadOnlyDictionary (d => d.Year);

			Assert.AreEqual (3, c.Count);
			Assert.IsTrue (c.ContainsKey (2009));
			Assert.IsTrue (c.ContainsKey (2008));
			Assert.IsTrue (c.ContainsKey (2007));
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void ToValueReader_SelfNull ()
		{
			IEnumerable<string> s = null;
			s.ToValueReader ();
		}

		[Test]
		public void ToValueReader ()
		{
			#region ToValueReader
			string s;
			DateTime c;
			double d;
			int n;

			"a 1970-01-01 2 3.14".Words().ToValueReader ()
				.Read (out s);
			Assert.AreEqual ("a", s);

			"a 1970-01-01 2 3.14".Words().ToValueReader ()
				.Read (out s).Read (out c);
			Assert.AreEqual ("a", s);
			Assert.AreEqual (new DateTime (1970, 1, 1), c);

			"a 1970-01-01 2 3.14".Words().ToValueReader ()
				.Read (out s).Read (out c).Read (out n);
			Assert.AreEqual ("a", s);
			Assert.AreEqual (new DateTime (1970, 1, 1), c);
			Assert.AreEqual (2, n);

			"a 1970-01-01 2 3.14".Words().ToValueReader ()
				.Read (out s).Read (out c).Read (out n).Read (out d);
			Assert.AreEqual ("a", s);
			Assert.AreEqual (new DateTime (1970, 1, 1), c);
			Assert.AreEqual (2, n);
			Assert.AreEqual (3.14, d);
			#endregion
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void ToValueReader_T_SelfNull ()
		{
			IEnumerable<int> s = null;
			s.ToValueReader ();
		}

		[Test]
		public void ToValueReader_T ()
		{
			string a, b, c;
			new[]{1, 2, 3}.ToValueReader ()
				.Read (out a).Read (out b).Read (out c);
			Assert.AreEqual ("1", a);
			Assert.AreEqual ("2", b);
			Assert.AreEqual ("3", c);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Sort_SelfNull ()
		{
			IEnumerable<int> e = null;
			e.Sort ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Sort_SelfNull_Comparison ()
		{
			IEnumerable<int> e = null;
			Comparison<int> c = (x,y) => 0;
			e.Sort (c);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Sort_SelfNull_Comparer ()
		{
			IEnumerable<int> e = null;
			IComparer<int> c = Comparer<int>.Default;
			e.Sort (c);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Sort_Comparison_Null ()
		{
			IEnumerable<int> e = new[]{1};
			Comparison<int> c = null;
			e.Sort (c);
		}

		[Test]
		public void Sort_Comparer_Null ()
		{
			IEnumerable<int> e = new[]{1};
			IComparer<int> c = null;
			e.Sort (c);
		}

		[Test]
		public void Sort ()
		{
			Assert.AreEqual (new[]{4, 3, 2, 1}.Sort ().Implode (), "1234");
			Assert.AreEqual (new[]{1, 2, 3, 4}.Sort ((x,y) => x == y ? 0 : x < y ? 1 : -1).Implode (), "4321");
			Assert.AreEqual (new[]{2, 4, 1, 3}.Sort (Comparer<int>.Default).Implode (), "1234");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void OrderByNatural_SelfNull ()
		{
			IEnumerable<int> e = null;
			e.OrderByNatural (x => x.ToString ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void OrderByNatural_FuncNull ()
		{
			IEnumerable<int> e = new[]{1};
			Func<int,string> f = null;
			e.OrderByNatural (f);
		}

		[Test]
		public void OrderByNatural ()
		{
			#region OrderByNatural
			string[] expected = {
				"a.1.b.2.c.3.d.4.e.5.f.6.g.7.h.8.i.9.j.10.k.11",
				"a.1.b.2.c.3.d.4.e.5.f.6.g.7.h.8.i.9.j.10.k.12",
				"bar",
				"foo",
				"foo",
				"foo1",
				"foo2",
				"foo3",
				"foo4",
				"foo5",
				"foo6",
				"foo7",
				"foo8",
				"foo9",
				"foo10",
			};
			IEnumerable<string> actual = new[]{
				"foo",
				"foo",
				"foo10",
				"foo1",
				"foo4",
				"foo2",
				"foo3",
				"foo9",
				"foo5",
				"foo7",
				"foo8",
				"foo6",
				"bar",
				"a.1.b.2.c.3.d.4.e.5.f.6.g.7.h.8.i.9.j.10.k.12",
				"a.1.b.2.c.3.d.4.e.5.f.6.g.7.h.8.i.9.j.10.k.11",
			}.OrderByNatural (s => s);

			AssertAreSame (expected, actual);
			#endregion
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SortNatural_SelfNull ()
		{
			IEnumerable<string> e = null;
			e.SortNatural ();
		}

		[Test]
		public void SortNatural ()
		{
			string[] expected = {
				"a.1.b.2.c.3.d.4.e.5.f.6.g.7.h.8.i.9.j.10.k.11",
				"a.1.b.2.c.3.d.4.e.5.f.6.g.7.h.8.i.9.j.10.k.12",
				"bar",
				"foo",
				"foo",
				"foo1",
				"foo2",
				"foo3",
				"foo4",
				"foo5",
				"foo6",
				"foo7",
				"foo8",
				"foo9",
				"foo10",
			};
			IEnumerable<string> actual = new[]{
				"foo",
				"foo",
				"foo10",
				"foo1",
				"foo4",
				"foo2",
				"foo3",
				"foo9",
				"foo5",
				"foo7",
				"foo8",
				"foo6",
				"bar",
				"a.1.b.2.c.3.d.4.e.5.f.6.g.7.h.8.i.9.j.10.k.12",
				"a.1.b.2.c.3.d.4.e.5.f.6.g.7.h.8.i.9.j.10.k.11",
			}.SortNatural ();

			AssertAreSame (expected, actual);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void Cache_SelfNull ()
		{
			IEnumerable<object> s = null;
			s.Cache ();
		}

		#region CachedSequence_RandomValuesGenerator
		internal static IEnumerable<int> RandomValues (Random r, int max)
		{
			while (true)
				yield return r.Next (max);
		}
		#endregion

		// Test idea from: http://bartdesmet.net/blogs/bart/archive/2009/09/12/taming-your-sequence-s-side-effects-through-ienumerable-let.aspx
		[Test]
		public void Cache ()
		{
			#region NoCache
			var randSeq = RandomValues (new Random (), 100);
			// this could pass, but that's highly improbable
			Assert.IsFalse (
					randSeq.Take (10).SelectFromEach (randSeq.Take (10),
						(l, r) => l + r)
					.All (x => x % 2 == 0));
			#endregion

			#region Cache
			// We can make the above sane by memoizing the sequence:
			Assert.IsTrue (randSeq.Take (10).Cache ().With (c => c.SelectFromEach (c, (l, r) => l + r)).All (x => x % 2 == 0));
			#endregion
		}

		class CacheIterDisposed {
			public int Disposed;
			public IEnumerable<int> GetValues ()
			{
				try {
					yield return 1;
					yield return 2;
					yield return 3;
					yield return 4;
					yield return 5;
				}
				finally {
					// Console.WriteLine (new System.Diagnostics.StackTrace().ToString ());
					Disposed++;
				}
			}
		}

		[Test]
		public void Cache_Leaf_IteratorIsDisposed ()
		{
			var r = new CacheIterDisposed ();
			int c = 0;
			foreach (var e in r.GetValues ().Take(2).Cache ()) {
				++c;
				Ignore (e);
			}
			Assert.AreEqual (2, c);
			Assert.AreEqual (1, r.Disposed);
		}

		[Test]
		public void Cache_Intermediate_IteratorIsDisposed ()
		{
			var r = new CacheIterDisposed ();
			int c = 0;
			foreach (var e in r.GetValues ().Cache ().Take (2)) {
				++c;
				Ignore (e);
			}
			Assert.AreEqual (2, c);
			Assert.AreEqual (1, r.Disposed);

			r.Disposed=0;
			r.GetValues ().Where (v => v%2==0).Select (v => v*2).Apply ();
			Assert.AreEqual (1, r.Disposed);

			r.Disposed=0;
			r.GetValues().Cache ().Where(v => v%2==0).Select(v => v*2).Apply ();
			Assert.AreEqual (1, r.Disposed);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void ToTuple_SelfNull ()
		{
			IEnumerable<object> s = null;
			s.ToTuple ();
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ToTuple_TooManyValues ()
		{
#if !NET_4_0
			Enumerable.Range (0, Tuple.MaxValues+1).ToTuple ();
#else
			// .NET 4.0 has 1-8 in mscorlib.dll, 9-16 in System.Core.dll; 100 should
			// break. ;-)
			Enumerable.Range (0, 100).ToTuple ();
#endif
		}

		[Test]
		public void ToTuple ()
		{
			#region ToTuple
			IEnumerable<object> s = new object[]{1, '2', 3L, "4"};
			object tl = s.ToTuple ();
			Assert.AreEqual (typeof(Tuple<int, char, long, string>), tl.GetType());

			var t = (Tuple<int, char, long, string>) tl;
			Assert.AreEqual (1,   t.Item1);
			Assert.AreEqual ('2', t.Item2);
			Assert.AreEqual (3L,  t.Item3);
			Assert.AreEqual ("4", t.Item4);
			#endregion

			var a = Tuple.Create (1U, 2L, '\x3', (byte) 4);
			Assert.AreEqual (true,
					a.Equals (new object[]{1U, 2L, '\x3', (byte) 4}.ToTuple ()));
			Assert.AreEqual (a,
					a.ToEnumerable ().ToTuple ());
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void SequenceCompare_SelfNull ()
		{
			IEnumerable<int> s = null;
			s.SequenceCompare (new int[0]);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void SequenceCompare_ListNull ()
		{
			IEnumerable<int> s = new[]{1};
			s.SequenceCompare (null);
		}

		[Test]
		public void SequenceCompare_ComparerNull ()
		{
			IEnumerable<int> s = new[]{1};
			int r = s.SequenceCompare (new[]{1}, null);
			Assert.AreEqual (0, r);
		}

		[Test]
		public void SequenceCompare ()
		{
			#region SequenceCompare
			Assert.AreEqual (0,
					new[]{1, 2}.SequenceCompare (new[]{1, 2}));
			Assert.AreEqual (-1,
					new[]{1, 1}.SequenceCompare (new[]{1, 2}));
			Assert.AreEqual (1,
					new[]{1, 3}.SequenceCompare (new[]{1, 2}));
			Assert.AreEqual (-1,
					new[]{1, 2}.SequenceCompare (new[]{1}));
			Assert.AreEqual (1,
					new[]{1}.SequenceCompare (new[]{1, 2}));
			#endregion
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void Shuffle_SelfNull ()
		{
			IEnumerable<int> s = null;
			s.Shuffle ();
		}

		[Test]
		public void Shuffle_RandomNull ()
		{
			IEnumerable<int> s = new[]{1};
			IEnumerable<int> r = s.Shuffle (null);
			AssertAreSame (s, r);
		}

		[Test]
		public void Shuffle ()
		{
			// how do you adequately test randomness?
			#region Shuffle
			IEnumerable<int> r = new[]{1,2,3,4,5}.Shuffle ();
			Assert.AreEqual (5, r.Count());
			Assert.IsTrue (r.Contains (1));
			Assert.IsTrue (r.Contains (2));
			Assert.IsTrue (r.Contains (3));
			Assert.IsTrue (r.Contains (4));
			Assert.IsTrue (r.Contains (5));

			Assert.IsFalse (r.Contains (0));
			Assert.IsFalse (r.Contains (6));
			#endregion
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void SelectBreadthFirst_SelfNull ()
		{
			TreeNode<int>[] root = null;
			root.SelectBreadthFirst (e => e.Value, e => e.Children);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void SelectBreadthFirst_ValueSelectorNull ()
		{
			var root = new TreeNode<int>[0];
			Func<TreeNode<int>, int> valueSelector = null;
			root.SelectBreadthFirst (valueSelector, x => x.Children);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void SelectBreadthFirst_ChildrenSelectorNull ()
		{
			var root = new TreeNode<int>[0];
			Func<TreeNode<int>, IEnumerable<TreeNode<int>>> childrenSelector = null;
			root.SelectBreadthFirst (x => x.Value, childrenSelector);
		}

		[Test]
		public void SelectBreadthFirst ()
		{
			#region SelectBreadthFirst
			TreeNode<int>[] root = new TreeNode<int>[] {
				new TreeNode<int> {
					Value = 1, Children = new [] {
						new TreeNode<int> { Value = 2 },
						new TreeNode<int> {
							Value = 3, Children = new [] {
								new TreeNode<int> { Value = 5 },
							}
						},
						new TreeNode<int> { Value = 4 },
					}
				},
				new TreeNode<int> { Value = -1 },
			};
			IEnumerable<int> values = root
				.SelectBreadthFirst (x => x.Value, x => x.Children);
			AssertAreSame (new[]{ 1, 2, 3, 4, 5, -1 }, values);
			#endregion
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void SelectDepthFirst_SelfNull ()
		{
			TreeNode<int>[] root = null;
			root.SelectDepthFirst (x => x.Value, x => x.Children);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void SelectDepthFirst_ValueSelectorNull ()
		{
			var root = new TreeNode<int>[0];
			Func<TreeNode<int>, int> valueSelector = null;
			root.SelectDepthFirst (valueSelector, x => x.Children);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void SelectDepthFirst_ChildrenSelectorNull ()
		{
			var root = new TreeNode<int>[0];
			Func<TreeNode<int>, IEnumerable<TreeNode<int>>> childrenSelector = null;
			root.SelectDepthFirst (x => x.Value, childrenSelector);
		}

		[Test]
		public void SelectDepthFirst ()
		{
			#region SelectDepthFirst
			TreeNode<int>[] root = new TreeNode<int>[] {
				new TreeNode<int> {
					Value = 1, Children = new [] {
						new TreeNode<int> { Value = 2 },
						new TreeNode<int> {
							Value = 3, Children = new [] {
								new TreeNode<int> { Value = 5 },
							}
						},
						new TreeNode<int> { Value = 4 },
					}
				},
				new TreeNode<int> { Value = -1 },
			};
			IEnumerable<int> values = root
				.SelectDepthFirst (x => x.Value, x => x.Children);
			AssertAreSame (new[]{ 1, 2, 3, 5, 4, -1 }, values);
			#endregion
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void ContiguousSubsequences_SelfNull ()
		{
			IEnumerable<int> s = null;
			s.ContiguousSubsequences (1);
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ContiguousSubsequences_WindowSizeIsInvalid ()
		{
			IEnumerable<int> s = new[]{1};
			s.ContiguousSubsequences (0);
		}

		[Test]
		public void ContiguousSubsequences ()
		{
			#region ContiguousSubsequences
			IEnumerable<IEnumerable<char>> results = "12345678".ContiguousSubsequences (4);
			Assert.AreEqual (5, results.Count());
			Assert.AreEqual ("1234", results.ElementAt (0).Implode());
			Assert.AreEqual ("2345", results.ElementAt (1).Implode());
			Assert.AreEqual ("3456", results.ElementAt (2).Implode());
			Assert.AreEqual ("4567", results.ElementAt (3).Implode());
			Assert.AreEqual ("5678", results.ElementAt (4).Implode());
			#endregion
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void NotNull_SelfNull ()
		{
			IEnumerable<int?> s = null;
			s.NotNull ();
		}

		[Test]
		public void NotNull ()
		{
			#region NotNull
			IEnumerable<int?> s = new int?[]{
				null,
				2,
				null,
				4
			};
			Assert.IsTrue (new []{
					2, 4
			}.SequenceEqual (s.NotNull ()));
			#endregion
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void CopyTo_Array_SelfNull ()
		{
			IEnumerable<int> s = null;
			var array = new int [2];
			s.CopyTo (array, 0);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void CopyTo_Array_ArrayNull ()
		{
			IEnumerable<int> s = new int [0];
			int[] array = null;
			s.CopyTo (array, 0);
		}

		[Test]
		public void CopyTo_Array_ArrayIndex ()
		{
			IEnumerable<int> s = new int [0];
			int[] array = new int [0];
			Assert.Throws<ArgumentOutOfRangeException>(() => s.CopyTo (array, -1));
			Assert.Throws<ArgumentOutOfRangeException>(() => s.CopyTo (array, 0));
			array = new int [1];
			s.CopyTo (array, 0);
		}

		[Test]
		public void CopyTo_Collection ()
		{
			IEnumerable<int> s = null;
			var c = new List<int> ();
			Assert.Throws<ArgumentNullException>(() => s.CopyTo (c));
			s = new int [0];
			c = null;
			Assert.Throws<ArgumentNullException>(() => s.CopyTo (c));
		}

		[Test]
		public void CopyTo ()
		{
			#region CopyTo
			IEnumerable<int> s = new int []{1, 2, 3, 4};
			int[] array = new int [5];
			s.CopyTo (array, 0);
			// Note: trailing 0 because array.Length > s.Length
			Assert.IsTrue (new[]{
					1, 2, 3, 4, 0
			}.SequenceEqual (array));

			List<int> collection = new List<int> ();
			s.CopyTo (collection);
			// Note: same size as s.Length
			Assert.IsTrue (new[]{
					1, 2, 3, 4
			}.SequenceEqual (collection));
			#endregion
		}

		[Test]
		public void Sum ()
		{
			#region Sum
			IEnumerable<uint> s = null;
			Assert.Throws<ArgumentNullException>(() => s.Sum ());

			s = new []{1U, 2U, 3U};
			Assert.AreEqual (6U, s.Sum ());
			Assert.AreEqual (6U, s.Sum (null));

			IEnumerable<SimpleNumber> s2 = new[]{
				new SimpleNumber (1),
				new SimpleNumber (2),
				new SimpleNumber (3),
			};
			Assert.AreEqual (new SimpleNumber (6), s2.Sum (new SimpleNumberMath ()));
			#endregion
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SelectFromEach2_Source1Null ()
		{
			IEnumerable<int> s1 = null;
			IEnumerable<int> s2 = new[]{2};
			s1.SelectFromEach (s2, (a,b) => "");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SelectFromEach2_Source2Null ()
		{
			IEnumerable<int> s2 = null;
			new[]{1}.SelectFromEach (s2, (a,b) => "");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SelectFromEach2_SelectorNull ()
		{
			IEnumerable<int> s2 = new[]{2};
			Func<int,int,string> f = null;
			new[]{1}.SelectFromEach (s2, f);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SelectFromEach3_Source1Null ()
		{
			IEnumerable<int> e = null;
			IEnumerable<int> s2 = new[]{1};
			IEnumerable<int> s3 = new[]{2};
			e.SelectFromEach (s2, s3, (a,b,c) => "");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SelectFromEach3_Source2Null ()
		{
			IEnumerable<int> s2 = null;
			IEnumerable<int> s3 = new[]{2};
			new[]{1}.SelectFromEach (s2, s3, (a,b,c) => "");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SelectFromEach3_Source3Null ()
		{
			IEnumerable<int> s2 = new[]{2};
			IEnumerable<int> s3 = null;
			new[]{1}.SelectFromEach (s2, s3, (a,b,c) => "");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SelectFromEach3_SelectorNull ()
		{
			IEnumerable<int> s2 = new[]{2};
			IEnumerable<int> s3 = new[]{3};
			Func<int,int,int,string> f = null;
			new[]{1}.SelectFromEach (s2, s3, f);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SelectFromEach4_Source1Null ()
		{
			IEnumerable<int> s1 = null;
			IEnumerable<int> s2 = new[]{2};
			IEnumerable<int> s3 = new[]{3};
			IEnumerable<int> s4 = new[]{4};
			s1.SelectFromEach (s2, s3, s4, (a,b,c,d) => "");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SelectFromEach4_Source2Null ()
		{
			IEnumerable<int> s2 = null;
			IEnumerable<int> s3 = new[]{3};
			IEnumerable<int> s4 = new[]{4};
			new[]{1}.SelectFromEach (s2, s3, s4, (a,b,c,d) => "");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SelectFromEach4_Source3Null ()
		{
			IEnumerable<int> s2 = new[]{2};
			IEnumerable<int> s3 = null;
			IEnumerable<int> s4 = new[]{4};
			new[]{1}.SelectFromEach (s2, s3, s4, (a,b,c,d) => "");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SelectFromEach4_Source4Null ()
		{
			IEnumerable<int> s2 = new[]{2};
			IEnumerable<int> s3 = new[]{3};
			IEnumerable<int> s4 = null;
			new[]{1}.SelectFromEach (s2, s3, s4, (a,b,c,d) => "");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SelectFromEach4_SelectorNull ()
		{
			IEnumerable<int> s2 = new[]{2};
			IEnumerable<int> s3 = new[]{3};
			IEnumerable<int> s4 = new[]{4};
			Func<int,int,int,int,string> f = null;
			new[]{1}.SelectFromEach (s2, s3, s4, f);
		}

		[Test]
		public void SelectFromEach ()
		{
			#region SelectFromEach2
			List<int>  a = new List<int> {1, 2, 3, 4};
			List<char> b = new List<char> {'a', 'b', 'c', 'd', 'e'};
			var c = a.SelectFromEach (b, (x, y) => new { First = x, Second = y }).ToList ();
			Assert.AreEqual (4, c.Count);
			Assert.AreEqual (1,   c [0].First);
			Assert.AreEqual ('a', c [0].Second);
			Assert.AreEqual (2,   c [1].First);
			Assert.AreEqual ('b', c [1].Second);
			Assert.AreEqual (3,   c [2].First);
			Assert.AreEqual ('c', c [2].Second);
			Assert.AreEqual (4,   c [3].First);
			Assert.AreEqual ('d', c [3].Second);
			#endregion

			#region SelectFromEach3
			Assert.AreEqual ("123",
					new[]{1}.SelectFromEach (new[]{2}, new[]{3}, 
						(x,y,z) => x.ToString () + y.ToString () + z.ToString ()).Implode ());
			#endregion
			#region SelectFromEach4
			Assert.AreEqual ("1234",
					new[]{1}.SelectFromEach (new[]{2}, new[]{3}, new[]{4},
						(w,x,y,z) => w.ToString () + x.ToString () + y.ToString () + z.ToString ()).Implode ());
			#endregion
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ExceptLast_SelfNull ()
		{
			IEnumerable<char> e = null;
			e.ExceptLast (1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ExceptLast_NegativeLast ()
		{
			new[]{'a'}.ExceptLast (-1);
		}

		[Test]
		public void ExceptLast ()
		{
			Assert.AreEqual ("1234", new[]{1,2,3,4}.ExceptLast(0).Implode ());
			Assert.AreEqual ("123",  new[]{1,2,3,4}.ExceptLast(1).Implode ());
			Assert.AreEqual ("12",   new[]{1,2,3,4}.ExceptLast(2).Implode ());
			Assert.AreEqual ("1",    new[]{1,2,3,4}.ExceptLast(3).Implode ());
			Assert.AreEqual ("",     new[]{1,2,3,4}.ExceptLast(4).Implode ());
			Assert.AreEqual ("",     new[]{1,2,3,4}.ExceptLast(5).Implode ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Intersperse_SelfNull ()
		{
			IEnumerable<char> e = null;
			e.Intersperse ('b');
		}

		[Test]
		public void Intersperse ()
		{
			#region Intersperse
			Assert.AreEqual ("1929394", new[]{1,2,3,4}.Intersperse (9).Implode ());
			Assert.AreEqual ("a.z",     new[]{'a','z'}.Intersperse ('.').Implode ());
			IEnumerable<IEnumerable<char>> e = new char[][]{ 
				new char[]{'b', 'c', 'd'}, 
				new char[]{'e', 'f', 'g'},
			};
			IEnumerable<char> x = new char[]{'a', 'a'};
			Assert.AreEqual ("bcdaaefg", e.Intersperse (x).Implode ());
			#endregion
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Transpose_SelfNull ()
		{
			IEnumerable<IEnumerable<char>> e = null;
			e.Transpose ();
		}

		[Test]
		public void Transpose ()
		{
			#region Transpose
			IEnumerable<IEnumerable<int>> a = new int[][]{
				new int[]{1, 2, 3},
				new int[]{4, 5, 6},
			};
			IEnumerable<IEnumerable<int>> b = a.Transpose ();
			List<List<int>> c = b.ToList ();
			Assert.AreEqual (3, c.Count);
			Assert.AreEqual (2, c [0].Count);
			Assert.AreEqual (2, c [1].Count);
			Assert.AreEqual (2, c [2].Count);
			Assert.AreEqual (1, c [0][0]);
			Assert.AreEqual (4, c [0][1]);
			Assert.AreEqual (2, c [1][0]);
			Assert.AreEqual (5, c [1][1]);
			Assert.AreEqual (3, c [2][0]);
			Assert.AreEqual (6, c [2][1]);

			// Test non-"rectangular" array
			a = new int[][]{
				new int[]{1, 2},
				new int[]{3},
				new int[]{4, 5},
			};
			b = a.Transpose ();
			Assert.AreEqual (2, b.Count ());
			Assert.IsTrue (new[]{1, 3, 4}.SequenceEqual (b.ElementAt (0)));
			Assert.IsTrue (new[]{2, 5}.SequenceEqual (b.ElementAt (1)));
			#endregion
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ToList_SelfNull ()
		{
			IEnumerable<IEnumerable<char>> e = null;
			e.ToList ();
		}

		[Test]
		public void ToList ()
		{
			#region ToList
			int[][] a = new int[][]{
				new int[]{1, 2, 3},
				new int[]{4, 5, 6},
			};
			IEnumerable<IEnumerable<int>> b = a;
			List<List<int>> c = b.ToList ();
			Assert.AreEqual (a.Length, c.Count);
			Assert.AreEqual (a [0].Length, c [0].Count);
			Assert.AreEqual (a [1].Length, c [1].Count);
			Assert.AreEqual (a [0][0], c [0][0]);
			Assert.AreEqual (a [0][1], c [0][1]);
			Assert.AreEqual (a [0][2], c [0][2]);
			Assert.AreEqual (a [1][0], c [1][0]);
			Assert.AreEqual (a [1][1], c [1][1]);
			Assert.AreEqual (a [1][2], c [1][2]);
			#endregion
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void AsIList_SelfNull ()
		{
			IEnumerable<int>  s = null;
			s.AsIList ();
		}

		[Test]
		public void AsIList_IListReturnsIdentity ()
		{
			IEnumerable<int>  s = new[]{1, 2, 3};
			Assert.IsTrue (object.ReferenceEquals (s, s.AsIList ()));
		}

		[Test]
		public void AsIList_NonListReturnsNewList ()
		{
			IEnumerable<int>  s = Sequence.Iterate (1, v => v + 1).Take (5);
			IList<int>     list = s.AsIList ();
			Assert.IsFalse (object.ReferenceEquals (s, list));
			AssertAreSame (s, list);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AggregateReverse_SF_SourceNull ()
		{
			IEnumerable<int>  s = null;
			Func<int,int,int> f = (a,b) => a-b;
			s.AggregateReverse (f);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void AggregateReverse_SF_SourceEmpty ()
		{
			IEnumerable<int>  s = new int[]{};
			Func<int,int,int> f = (a,b) => a-b;
			s.AggregateReverse (f);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AggregateReverse_SF_FuncNull ()
		{
			IEnumerable<int>  s = new[]{1};
			Func<int,int,int> f = null;
			s.AggregateReverse (f);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AggregateReverse_SSF_SourceNull ()
		{
			IEnumerable<int>  s = null;
			Func<int,int,int> f = (a,b) => a-b;
			s.AggregateReverse (0, f);
		}

		[Test]
		public void AggregateReverse_SSF_SourceEmpty ()
		{
			IEnumerable<int>  s = new int[]{};
			Func<int,int,int> f = (a,b) => a-b;
			s.AggregateReverse (0, f);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AggregateReverse_SSF_FuncNull ()
		{
			IEnumerable<int>  s = new[]{1};
			Func<int,int,int> f = null;
			s.AggregateReverse (0, f);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AggregateReverse_SSFR_SourceNull ()
		{
			IEnumerable<int>  s = null;
			Func<int,int,int> f = (a,b) => a-b;
			Func<int,int>     r = x => x;
			s.AggregateReverse (0, f, r);
		}

		[Test]
		public void AggregateReverse_SSFR_SourceEmpty ()
		{
			IEnumerable<int>  s = new int[]{};
			Func<int,int,int> f = (a,b) => a-b;
			Func<int,int>     r = x => x;
			s.AggregateReverse (0, f, r);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AggregateReverse_SSFR_FuncNull ()
		{
			IEnumerable<int>  s = new[]{1};
			Func<int,int,int> f = null;
			Func<int,int>     r = x => x;
			s.AggregateReverse (0, f, r);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AggregateReverse_SSFR_ResultNull ()
		{
			IEnumerable<int>  s = new[]{1};
			Func<int,int,int> f = (a,b) => a-b;
			Func<int,int>     r = null;
			s.AggregateReverse (0, f, r);
		}

		[Test]
		public void AggregateReverse ()
		{
			IEnumerable<int> s = new []{1, 2, 3, 4, 5};
			Assert.AreEqual (-5, s.AggregateReverse ((a,b) => a - b));
			Assert.AreEqual ("54321",
					s.AggregateReverse (new StringBuilder (), (a,b) => a.Append (b)).ToString ());
			Assert.AreEqual (1,
					new int[]{}.AggregateReverse (1, (a,b) => a+b));
			Assert.AreEqual ("54321",
					s.AggregateReverse (new StringBuilder (), (a,b) => a.Append (b), a => a.ToString ()));
			Assert.AreEqual (1,
					new int[]{}.AggregateReverse (1, (a,b) => a+b, x => x));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Concat_SourceNull ()
		{
			IEnumerable<int> s = null;
			IEnumerable<IEnumerable<int>> ss = new []{new []{1}};
			s.Concat (ss);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Concat_SourceesNull ()
		{
			IEnumerable<int> s = new[]{1};
			IEnumerable<IEnumerable<int>> ss = null;
			s.Concat (ss);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Concat_Params_SourceNull ()
		{
			IEnumerable<int> s = null;
			s.Concat (new[]{1});
		}

		[Test]
		public void Concat ()
		{
			IEnumerable<int> s = new[]{1};
			Assert.AreEqual ("1234567",
					s.Concat (new[]{2, 3}, new[]{4, 5}, new[]{6, 7}).Implode ());
			IEnumerable<IEnumerable<int>> ss = new []{
				new[]{2,3},
				new[]{4,5},
				new[]{6,7},
			};
			Assert.AreEqual ("1234567", s.Concat (ss).Implode ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void And_SourceNull ()
		{
			IEnumerable<bool> s = null;
			s.And ();
		}

		[Test]
		public void And ()
		{
			#region And
			Assert.AreEqual (false,
					new[]{true, false, true, true}.And ());
			Assert.AreEqual (false,
					new[]{false, false, false, false}.And ());
			Assert.AreEqual (true,
					new[]{true, true, true, true}.And ());
			Assert.AreEqual (false,
					new[]{true, true, true, true}.Concat (Sequence.Repeat (false)).And ());
			#endregion
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Or_SourceNull ()
		{
			IEnumerable<bool> s = null;
			s.Or ();
		}

		[Test]
		public void Or ()
		{
			#region Or
			Assert.AreEqual (true,
					new[]{true, false, true, true}.Or ());
			Assert.AreEqual (false,
					new[]{false, false, false, false}.Or ());
			Assert.AreEqual (true,
					new[]{true, true, true, true}.Or ());
			Assert.AreEqual (true,
					new[]{false, false, true}.Concat (Sequence.Repeat (false)).Or ());
			#endregion
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AggregateHistory_SF_SourceNull ()
		{
			IEnumerable<int>  s = null;
			Func<int,int,int> f = (a,b) => a-b;
			s.AggregateHistory (f);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void AggregateHistory_SF_SourceEmpty ()
		{
			IEnumerable<int>  s = new int[]{};
			Func<int,int,int> f = (a,b) => a-b;
			// need .Apply() as check for empty source is delayed until enumeration.
			s.AggregateHistory (f).Apply ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AggregateHistory_SF_FuncNull ()
		{
			IEnumerable<int>  s = new[]{1};
			Func<int,int,int> f = null;
			s.AggregateHistory (f);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AggregateHistory_SSF_SourceNull ()
		{
			IEnumerable<int>  s = null;
			Func<int,int,int> f = (a,b) => a-b;
			s.AggregateHistory (0, f);
		}

		[Test]
		public void AggregateHistory_SSF_SourceEmpty ()
		{
			IEnumerable<int>  s = new int[]{};
			Func<int,int,int> f = (a,b) => a-b;
			s.AggregateHistory (0, f);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AggregateHistory_SSF_FuncNull ()
		{
			IEnumerable<int>  s = new[]{1};
			Func<int,int,int> f = null;
			s.AggregateHistory (0, f);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AggregateHistory_SSFR_SourceNull ()
		{
			IEnumerable<int>  s = null;
			Func<int,int,int> f = (a,b) => a-b;
			Func<int,int>     r = x => x;
			s.AggregateHistory (0, f, r);
		}

		[Test]
		public void AggregateHistory_SSFR_SourceEmpty ()
		{
			IEnumerable<int>  s = new int[]{};
			Func<int,int,int> f = (a,b) => a-b;
			Func<int,int>     r = x => x;
			s.AggregateHistory (0, f, r);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AggregateHistory_SSFR_FuncNull ()
		{
			IEnumerable<int>  s = new[]{1};
			Func<int,int,int> f = null;
			Func<int,int>     r = x => x;
			s.AggregateHistory (0, f, r);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AggregateHistory_SSFR_ResultNull ()
		{
			IEnumerable<int>  s = new[]{1};
			Func<int,int,int> f = (a,b) => a-b;
			Func<int,int>     r = null;
			s.AggregateHistory (0, f, r);
		}

		[Test]
		public void AggregateHistory ()
		{
			IEnumerable<int> s = new []{1, 2, 3, 4, 5};
			Assert.AreEqual (
					"1,-1,-4,-8,-13",
					s.AggregateHistory ((a,b) => a - b).Implode (","));
			Assert.AreEqual (
					",1,12,123,1234,12345",
					s.AggregateHistory (new StringBuilder (), (a,b) => a.Append (b)).Implode (","));
			Assert.AreEqual ("1",
					new int[]{}.AggregateHistory (1, (a,b) => a+b).Implode (","));
			Assert.AreEqual (
					"R,R1,R12,R123,R1234,R12345",
					s.AggregateHistory (new StringBuilder (), 
						(a,b) => a.Append (b), 
						a => "R" + a.ToString ()).Implode (","));
			Assert.AreEqual ("1",
					new int[]{}.AggregateHistory (1, (a,b) => a+b, x => x).Implode (","));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AggregateReverseHistory_SF_SourceNull ()
		{
			IEnumerable<int>  s = null;
			Func<int,int,int> f = (a,b) => a-b;
			s.AggregateReverseHistory (f);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void AggregateReverseHistory_SF_SourceEmpty ()
		{
			IEnumerable<int>  s = new int[]{};
			Func<int,int,int> f = (a,b) => a-b;
			// need .Apply() as check for empty source is delayed until enumeration.
			s.AggregateReverseHistory (f).Apply ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AggregateReverseHistory_SF_FuncNull ()
		{
			IEnumerable<int>  s = new[]{1};
			Func<int,int,int> f = null;
			s.AggregateReverseHistory (f);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AggregateReverseHistory_SSF_SourceNull ()
		{
			IEnumerable<int>  s = null;
			Func<int,int,int> f = (a,b) => a-b;
			s.AggregateReverseHistory (0, f);
		}

		[Test]
		public void AggregateReverseHistory_SSF_SourceEmpty ()
		{
			IEnumerable<int>  s = new int[]{};
			Func<int,int,int> f = (a,b) => a-b;
			s.AggregateReverseHistory (0, f);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AggregateReverseHistory_SSF_FuncNull ()
		{
			IEnumerable<int>  s = new[]{1};
			Func<int,int,int> f = null;
			s.AggregateReverseHistory (0, f);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AggregateReverseHistory_SSFR_SourceNull ()
		{
			IEnumerable<int>  s = null;
			Func<int,int,int> f = (a,b) => a-b;
			Func<int,int>     r = x => x;
			s.AggregateReverseHistory (0, f, r);
		}

		[Test]
		public void AggregateReverseHistory_SSFR_SourceEmpty ()
		{
			IEnumerable<int>  s = new int[]{};
			Func<int,int,int> f = (a,b) => a-b;
			Func<int,int>     r = x => x;
			s.AggregateReverseHistory (0, f, r);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AggregateReverseHistory_SSFR_FuncNull ()
		{
			IEnumerable<int>  s = new[]{1};
			Func<int,int,int> f = null;
			Func<int,int>     r = x => x;
			s.AggregateReverseHistory (0, f, r);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AggregateReverseHistory_SSFR_ResultNull ()
		{
			IEnumerable<int>  s = new[]{1};
			Func<int,int,int> f = (a,b) => a-b;
			Func<int,int>     r = null;
			s.AggregateReverseHistory (0, f, r);
		}

		[Test]
		public void AggregateReverseHistory ()
		{
			IEnumerable<int> s = new []{1, 2, 3, 4, 5};
			Assert.AreEqual (
					"5,1,-2,-4,-5",
					s.AggregateReverseHistory ((a,b) => a - b).Implode (","));
			Assert.AreEqual (
					",5,54,543,5432,54321",
					s.AggregateReverseHistory (new StringBuilder (), (a,b) => a.Append (b)).Implode (","));
			Assert.AreEqual ("1",
					new int[]{}.AggregateReverseHistory (1, (a,b) => a+b).Implode (","));
			Assert.AreEqual (
					"R,R5,R54,R543,R5432,R54321",
					s.AggregateReverseHistory (new StringBuilder (), 
						(a,b) => a.Append (b), 
						a => "R" + a.ToString ()).Implode (","));
			Assert.AreEqual ("1",
					new int[]{}.AggregateReverseHistory (1, (a,b) => a+b, x => x).Implode (","));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SelectAggregated_SelfNull ()
		{
			IEnumerable<int>               s  = null;
			Func<int, int, Tuple<int,int>> f  = (x,y) => Tuple.Create (x, y);
			s.SelectAggregated (0, f);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SelectAggregated_FuncNull ()
		{
			IEnumerable<int>                      s  = new[]{1};
			Func<int, int, Tuple<int,int>>        f  = null;
			s.SelectAggregated (0, f);
		}

		[Test]
		public void SelectAggregated ()
		{
			#region SelectAggregated
			IEnumerable<int> s = new []{2, 3, 4, 5};
			Tuple<int, List<string>> r = s.SelectAggregated (1,
					(a,b) => Tuple.Create (a-b, "s" + (a-b)));
			Assert.AreEqual (-13, r.Item1);
			Assert.IsTrue (new[]{
					"s-1",
					"s-4",
					"s-8",
					"s-13",
			}.SequenceEqual (r.Item2));

			r = new int[]{}.SelectAggregated (42,
					(a,b) => Tuple.Create (a-b, b.ToString ()));
			Assert.AreEqual (42, r.Item1);
			Assert.AreEqual (0, r.Item2.Count);
			#endregion
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SelectReverseAggregated_SelfNull ()
		{
			IEnumerable<int>                s  = null;
			Func<int, int, Tuple<int,int>>  f  = (x,y) => Tuple.Create (x, y);
			s.SelectReverseAggregated (0, f);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SelectReverseAggregated_FuncNull ()
		{
			IEnumerable<int>                s  = new[]{1};
			Func<int, int, Tuple<int,int>>  f  = null;
			s.SelectReverseAggregated (0, f);
		}

		[Test]
		public void SelectReverseAggregated ()
		{
			#region SelectReverseAggregated
			IEnumerable<int> s = new []{1, 2, 3, 4};
			Tuple<int, List<string>> r = s.SelectReverseAggregated (5,
					(a,b) => Tuple.Create (a-b, "s" + (a-b)));
			Assert.AreEqual (-5, r.Item1);
			Assert.IsTrue (new[]{
				"s1",
				"s-2",
				"s-4",
				"s-5",
			}.SequenceEqual (r.Item2));

			r = new int[]{}.SelectReverseAggregated (42,
					(a,b) => Tuple.Create (a-b, b.ToString ()));
			Assert.AreEqual (42, r.Item1);
			Assert.AreEqual (0, r.Item2.Count);
			#endregion
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Cycle_SelfNull ()
		{
			IEnumerable<int> s = null;
			s.Cycle ();
		}

		[Test]
		public void Cycle ()
		{
			// not entirely sure how you sanely test an infinite list...
			var x = new[]{1};
			Assert.AreEqual ("1,1,1,1,1",
					x.Cycle ().Take (5).Implode (","));
			x = new[]{1, 2, 3};
			Assert.AreEqual ("1,2,3,1,2,3,1,2,3,1,2,3,1,2,3",
					x.Cycle ().Take (3*5).Implode (","));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SplitAt_SelfNull ()
		{
			IEnumerable<int> s = null;
			s.SplitAt (0);
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void SplitAt_FirstLength_Negative()
		{
			IEnumerable<int> s = new[]{1,2,3};
			s.SplitAt (-1);
		}

		[Test]
		public void SplitAt ()
		{
			#region SplitAt
			Assert.AreEqual ("Hello |World!",
					"Hello World!".SplitAt (6)
					.Aggregate ((x,y) => x.Implode () + "|" + y.Implode ()));
			Assert.AreEqual ("123|45",
					new[]{1,2,3,4,5}.SplitAt (3)
					.Aggregate ((x,y) => x.Implode () + "|" + y.Implode ()));
			Assert.AreEqual ("1|23",
					new[]{1,2,3}.SplitAt (1)
					.Aggregate ((x,y) => x.Implode () + "|" + y.Implode ()));
			Assert.AreEqual ("123|",
					new[]{1,2,3}.SplitAt (3)
					.Aggregate ((x,y) => x.Implode () + "|" + y.Implode ()));
			Assert.AreEqual ("123|",
					new[]{1,2,3}.SplitAt (4)
					.Aggregate ((x,y) => x.Implode () + "|" + y.Implode ()));
			Assert.AreEqual ("|123",
					new[]{1,2,3}.SplitAt (0)
					.Aggregate ((x,y) => x.Implode () + "|" + y.Implode ()));
			#endregion
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Span_SelfNull ()
		{
			IEnumerable<int> s = null;
			Func<int, bool>  p = x => true;
			s.Span (p);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Span_PredicateNull ()
		{
			IEnumerable<int> s = new[]{1};
			Func<int, bool>  p = null;
			s.Span (p);
		}

		[Test]
		public void Span ()
		{
			#region Span
			Assert.AreEqual ("12|341234",
					new[]{1,2,3,4,1,2,3,4}.Span (e => e < 3)
					.Aggregate ((x, y) => x.Implode () + "|" + y.Implode ()));
			Assert.AreEqual ("123|",
					new[]{1,2,3}.Span (e => e < 9)
					.Aggregate ((x, y) => x.Implode () + "|" + y.Implode ()));
			Assert.AreEqual ("|123",
					new[]{1,2,3}.Span (e => e < 0)
					.Aggregate ((x, y) => x.Implode () + "|" + y.Implode ()));
			#endregion
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Break_SelfNull ()
		{
			IEnumerable<int> s = null;
			Func<int, bool>  p = x => true;
			s.Break (p);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Break_FuncNull ()
		{
			IEnumerable<int> s = new[]{1};
			Func<int, bool>  p = null;
			s.Break (p);
		}

		[Test]
		public void Break ()
		{
			Assert.AreEqual ("123|41234",
					new[]{1,2,3,4,1,2,3,4}.Break (e => e > 3)
					.Aggregate ((x, y) => x.Implode () + "|" + y.Implode ()));
			Assert.AreEqual ("|123",
					new[]{1,2,3}.Break (e => e < 9)
					.Aggregate ((x, y) => x.Implode () + "|" + y.Implode ()));
			Assert.AreEqual ("123|",
					new[]{1,2,3}.Break (e => e > 9)
					.Aggregate ((x, y) => x.Implode () + "|" + y.Implode ()));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SkipPrefix_SelfNull ()
		{
			IEnumerable<int> s = null;
			IEnumerable<int> p = new[]{1};
			s.SkipPrefix (p);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SkipPrefix_PrefixNull ()
		{
			IEnumerable<int> s = new[]{1};
			IEnumerable<int> p = null;
			s.SkipPrefix (p);
		}

		[Test]
		public void SkipPrefix ()
		{
			#region SkipPrefix
			Assert.AreEqual ("bar",
					"foobar".SkipPrefix ("foo").Implode ());
			Assert.AreEqual ("",
					"foo".SkipPrefix ("foo").Implode ());
			Assert.AreEqual (null,
					"barfoo".SkipPrefix ("foo"));
			Assert.AreEqual (null,
					"barfoobaz".SkipPrefix ("foo"));
			#endregion
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void HaskellGroup_SelfNull ()
		{
			IEnumerable<int> s = null;
			s.HaskellGroup ();
		}

		[Test]
		public void HaskellGroup ()
		{
			IEnumerable<IEnumerable<char>> e = "Mississippi".HaskellGroup ();
			var l = e.ToList ();
			Assert.AreEqual (8, l.Count);
			AssertAreSame (new[]{'M'},      l [0]);
			AssertAreSame (new[]{'i'},      l [1]);
			AssertAreSame (new[]{'s', 's'}, l [2]);
			AssertAreSame (new[]{'i'},      l [3]);
			AssertAreSame (new[]{'s', 's'}, l [4]);
			AssertAreSame (new[]{'i'},      l [5]);
			AssertAreSame (new[]{'p', 'p'}, l [6]);
			AssertAreSame (new[]{'i'},      l [7]);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void InitialSegments_SelfNull ()
		{
			IEnumerable<int> s = null;
			s.InitialSegments ();
		}

		[Test]
		public void InitialSegments ()
		{
			#region InitialSegments
			IEnumerable<IEnumerable<char>> e = "abc".InitialSegments ();
			var l = e.ToList ();
			Assert.AreEqual (4, l.Count);
			AssertAreSame (new char[]{},          l [0]);
			AssertAreSame (new[]{'a'},            l [1]);
			AssertAreSame (new[]{'a', 'b'},       l [2]);
			AssertAreSame (new[]{'a', 'b', 'c'},  l [3]);
			#endregion
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TrailingSegments_SelfNull ()
		{
			IEnumerable<int> s = null;
			s.TrailingSegments ();
		}

		[Test]
		public void TrailingSegments ()
		{
			#region TrailingSegments
			IEnumerable<IEnumerable<char>> e = "abc".TrailingSegments ();
			var l = e.ToList ();
			Assert.AreEqual (4, l.Count);
			AssertAreSame (new[]{'a', 'b', 'c'},  l [0]);
			AssertAreSame (new[]{'a', 'b'},       l [1]);
			AssertAreSame (new[]{'a'},            l [2]);
			AssertAreSame (new char[]{},          l [3]);
			#endregion
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Partition_SelfNull ()
		{
			IEnumerable<int>  s = null;
			Func<int, bool>   f = e => true;
			s.Partition (f);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Partition_PredicateNull ()
		{
			IEnumerable<int>  s = new[]{1};
			Func<int, bool>   f = null;
			s.Partition (f);
		}

		[Test]
		public void Partition ()
		{
			#region Partition
			Tuple<IEnumerable<int>, IEnumerable<int>> r =
				Enumerable.Range (1,6).Partition (x => x % 2 == 0);
			Assert.IsTrue (new[]{2, 4, 6}.SequenceEqual (r.Item1));
			Assert.IsTrue (new[]{1, 3, 5}.SequenceEqual (r.Item2));
			#endregion
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void IndexOf_SelfNull ()
		{
			IEnumerable<int> s = null;
			s.IndexOf (0);
		}

		[Test]
		public void IndexOf ()
		{
			Assert.AreEqual (2,
					new[]{0,1,2}.IndexOf (2));
			Assert.AreEqual (-1,
					new[]{0,1,2}.IndexOf (3));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void IndexOfAny_SelfNull ()
		{
			IEnumerable<int> s = null;
			IEnumerable<int> v = new[]{1};
			s.IndexOfAny (v);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void IndexOfAny_ValuesNull ()
		{
			IEnumerable<int> s = new[]{1};
			IEnumerable<int> v = null;
			s.IndexOfAny (v);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void IndexOfAny_ParamsValuesNull ()
		{
			IEnumerable<int> s = new[]{1};
			int[]            v = null;
			s.IndexOfAny (v);
		}

		[Test]
		public void IndexOfAny ()
		{
			Assert.AreEqual (2,
					new[]{0,1,2}.IndexOfAny (2, 3, 4));
			Assert.AreEqual (1,
					new[]{0,1,2}.IndexOfAny (3, 2, 1));
			Assert.AreEqual (-1,
					new[]{0,1,2}.IndexOfAny (3, 4, 5));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void IndicesOf_SelfNull ()
		{
			IEnumerable<int> s = null;
			s.IndicesOf (0);
		}

		[Test]
		public void IndicesOf ()
		{
			Assert.AreEqual ("0,3",
					new[]{0,1,2,0}.IndicesOf (0).Implode (","));
			Assert.AreEqual ("",
					new[]{0,1,2}.IndicesOf (3).Implode (","));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void IndicesOfAny_SelfNull ()
		{
			IEnumerable<int> s = null;
			IEnumerable<int> v = new[]{1};
			s.IndicesOfAny (v);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void IndicesOfAny_ValuesNull ()
		{
			IEnumerable<int> s = new[]{1};
			IEnumerable<int> v = null;
			s.IndicesOfAny (v);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void IndicesOfAny_ParamsValuesNull ()
		{
			IEnumerable<int> s = new[]{1};
			int[]            v = null;
			s.IndicesOfAny (v);
		}

		[Test]
		public void IndicesOfAny ()
		{
			Assert.AreEqual ("0,3",
					new[]{0,1,2,0}.IndicesOfAny (0, 3, 4).Implode (","));
			Assert.AreEqual ("0,1,2,3",
					new[]{0,1,2,0}.IndicesOfAny (2, 1, 0).Implode (","));
			Assert.AreEqual ("",
					new[]{0,1,2}.IndicesOfAny (3, 4, 5).Implode (","));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FindIndex_SelfNull ()
		{
			IEnumerable<int> s = null;
			Func<int, bool>  f = v => v == 2;
			s.FindIndex (f);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FindIndex_PredicateNull ()
		{
			IEnumerable<int> s = new[]{1};
			Func<int, bool>  f = null;
			s.FindIndex (f);
		}

		[Test]
		public void FindIndex ()
		{
			Assert.AreEqual (3,
					new[]{1,3,5,6,7,9}.FindIndex (v => (v % 2) == 0));
			Assert.AreEqual (-1,
					new[]{1,3,5,7,9}.FindIndex (v => (v % 2) == 0));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FindIndices_SelfNull ()
		{
			IEnumerable<int> s = null;
			Func<int, bool>  f = v => v == 2;
			s.FindIndices (f);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FindIndices_PredicateNull ()
		{
			IEnumerable<int> s = new[]{1};
			Func<int, bool>  f = null;
			s.FindIndices (f);
		}

		[Test]
		public void FindIndices ()
		{
			Assert.AreEqual ("3,6",
					new[]{1,3,5,6,7,9,10}.FindIndices (v => (v % 2) == 0).Implode (","));
			Assert.AreEqual ("",
					new[]{1,3,5,7,9}.FindIndices (v => (v % 2) == 0).Implode (","));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Zip_SelfNull ()
		{
			IEnumerable<int>  s = null;
			IEnumerable<int>  v = new[]{1};
			s.Zip (v);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Zip_Source2Null ()
		{
			IEnumerable<int>  s = new[]{1};
			IEnumerable<int>  v = null;
			s.Zip (v);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Zip3_SelfNull ()
		{
			IEnumerable<int>  s  = null;
			IEnumerable<int>  v1 = new[]{1};
			IEnumerable<int>  v2 = new[]{1};
			s.Zip (v1, v2);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Zip3_Source2Null ()
		{
			IEnumerable<int>  s  = new[]{1};
			IEnumerable<int>  v1 = null;
			IEnumerable<int>  v2 = new[]{1};
			s.Zip (v1, v2);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Zip3_Source3Null ()
		{
			IEnumerable<int>  s  = new[]{1};
			IEnumerable<int>  v1 = new[]{1};
			IEnumerable<int>  v2 = null;
			s.Zip (v1, v2);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Zip4_SelfNull ()
		{
			IEnumerable<int>  s  = null;
			IEnumerable<int>  v1 = new[]{1};
			IEnumerable<int>  v2 = new[]{1};
			IEnumerable<int>  v3 = new[]{1};
			s.Zip (v1, v2, v3);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Zip4_Source2Null ()
		{
			IEnumerable<int>  s  = new[]{1};
			IEnumerable<int>  v1 = null;
			IEnumerable<int>  v2 = new[]{1};
			IEnumerable<int>  v3 = new[]{1};
			s.Zip (v1, v2, v3);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Zip4_Source3Null ()
		{
			IEnumerable<int>  s  = new[]{1};
			IEnumerable<int>  v1 = new[]{1};
			IEnumerable<int>  v2 = null;
			IEnumerable<int>  v3 = new[]{1};
			s.Zip (v1, v2, v3);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Zip4_Source4Null ()
		{
			IEnumerable<int>  s  = new[]{1};
			IEnumerable<int>  v1 = new[]{1};
			IEnumerable<int>  v2 = new[]{1};
			IEnumerable<int>  v3 = null;
			s.Zip (v1, v2, v3);
		}

		[Test]
		public void Zip ()
		{
			#region Zip2
			Assert.AreEqual ("1,5|2,4|",
					new[]{1,2}.Zip (new[]{5,4,3})
					.Aggregate (new StringBuilder(), 
						(b, e) => b.AppendFormat ("{0},{1}|", e.Item1, e.Item2)).ToString ());
			Assert.AreEqual ("",
					new int[]{}.Zip (new[]{5,4,3})
					.Aggregate (new StringBuilder(), 
						(b, e) => b.AppendFormat ("{0},{1}|", e.Item1, e.Item2)).ToString ());
			#endregion
			#region Zip3
			Assert.AreEqual ("1,3,5|2,4,6|",
					new[]{1,2}.Zip (new[]{3,4,5,6}, new[]{5,6,7})
					.Aggregate (new StringBuilder(), 
						(b, e) => b.AppendFormat ("{0},{1},{2}|", e.Item1, e.Item2, e.Item3)).ToString ());
			Assert.AreEqual ("",
					new int[]{}.Zip (new[]{5,4,3}, new[]{1,2,3})
					.Aggregate (new StringBuilder(), 
						(b, e) => b.AppendFormat ("{0},{1},{2}|", e.Item1, e.Item2, e.Item3)).ToString ());
			#endregion
			#region Zip4
			Assert.AreEqual ("1,3,5,7|2,4,6,8|",
					new[]{1,2}.Zip (new[]{3,4,5,6}, new[]{5,6,7}, new[]{7,8})
					.Aggregate (new StringBuilder(), 
						(b, e) => b.AppendFormat ("{0},{1},{2},{3}|", e.Item1, e.Item2, e.Item3, e.Item4)).ToString ());
			Assert.AreEqual ("",
					new int[]{}.Zip (new[]{3,4,5,6}, new[]{5,6,7}, new[]{7,8})
					.Aggregate (new StringBuilder(), 
						(b, e) => b.AppendFormat ("{0},{1},{2},{3}|", e.Item1, e.Item2, e.Item3, e.Item4)).ToString ());
			#endregion
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Unzip_SelfNull ()
		{
			IEnumerable<Tuple<int,int>> s = null;
			s.Unzip ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Unzip3_SelfNull ()
		{
			IEnumerable<Tuple<int,int,int>> s = null;
			s.Unzip ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Unzip4_SelfNull ()
		{
			IEnumerable<Tuple<int,int,int>> s = null;
			s.Unzip ();
		}

		[Test]
		public void Unzip ()
		{
			#region Unzip2
			Assert.AreEqual ("1,2|3,4",
					new[]{1,2}.Zip (new[]{3,4,5}).Unzip ()
					.Aggregate ((a, b) => a.Implode (",") + "|" + b.Implode (",")));
			#endregion
			#region Unzip3
			Assert.AreEqual ("1,2|3,4|5,6",
					new[]{1,2}.Zip (new[]{3,4,5}, new[]{5,6}).Unzip ()
					.Aggregate ((a, b, c) => a.Implode (",") + "|" + b.Implode (",") + "|" + c.Implode (",")));
			#endregion
			#region Unzip4
			Assert.AreEqual ("1,2|3,4|5,6|7,8",
					new[]{1,2}.Zip (new[]{3,4,5,6}, new[]{5,6,7}, new[]{7,8}).Unzip ()
					.Aggregate ((a, b, c, d) => a.Implode (",") + "|" + b.Implode (",") + "|" + c.Implode (",") + "|" + d.Implode (",")));
			#endregion
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Insert_SelfNull ()
		{
			IEnumerable<int> s = null;
			s.Insert (0);
		}

		[Test]
		public void Insert ()
		{
			Assert.AreEqual ("1,2,3,4",
					new[]{1,3,4}.Insert (2).Implode (","));
			Assert.AreEqual ("1,2,3,4",
					new[]{1,2,3}.Insert (4).Implode (","));
			Assert.AreEqual ("0",
					new int[]{}.Insert (0).Implode (","));
			Assert.AreEqual ("1,2,3,4,1",
					new[]{1,2,4,1}.Insert (3).Implode (","));
			Assert.AreEqual ("1,2,3,4",
					new[]{4}.Insert (3).Insert (2).Insert (1).Implode (","));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void RemoveFirstOccurrence_SelfNull ()
		{
			IEnumerable<int> s = null;
			s.RemoveFirstOccurrence (0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void RemoveFirstOccurrences_NegativeCount()
		{
			IEnumerable<int> s = new[]{1};
			s.RemoveFirstOccurrences (0, -1);
		}

		[Test]
		public void RemoveFirstOccurrences ()
		{
			#region RemoveFirstOccurrence
			Assert.AreEqual ("bnana",
					"banana".RemoveFirstOccurrence ('a').Implode ());
			#endregion
			#region RemoveFirstOccurrences
			Assert.AreEqual ("bnna",
					"banana".RemoveFirstOccurrences ('a', 2).Implode ());
			#endregion
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void HaskellGroupBy_SelfNull ()
		{
			IEnumerable<int>      s = null;
			Func<int, int, bool>  f = (a, b) => true;
			s.HaskellGroupBy (f);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void HaskellGroupBy_FuncNull ()
		{
			IEnumerable<int>      s = new[]{1};
			Func<int, int, bool>  f = null;
			s.HaskellGroupBy (f);
		}

		[Test]
		public void HaskellGroupBy()
		{
			#region HaskellGroupBy
			// Split a string into pairs
			string s = "123456789";
			int c = 0;
			List<List<char>> pairs = s.HaskellGroupBy(delegate {
				++c;
				if (c < 2)
					return true;
				c = 0;
				return false;
			}).ToList();
			Assert.AreEqual(5, pairs.Count);
			Assert.IsTrue(new[]{'1', '2'}.SequenceEqual (pairs [0]));
			Assert.IsTrue(new[]{'3', '4'}.SequenceEqual (pairs [1]));
			Assert.IsTrue(new[]{'5', '6'}.SequenceEqual (pairs [2]));
			Assert.IsTrue(new[]{'7', '8'}.SequenceEqual (pairs [3]));
			Assert.IsTrue(new[]{'9'}.SequenceEqual (pairs [4]));
			#endregion
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Insert_FuncNull ()
		{
			IEnumerable<int>    s = new[]{1};
			Func<int, int, int> f = null;
			s.Insert (0, f);
		}

		[Test]
		public void Subsets()
		{
			char[] input = { 'a', 'b', 'c', 'd' };
			char[][] expected = {
				new[] { 'a' },
				new[] { 'b' },
				new[] { 'a', 'b' },
				new[] { 'c' },
				new[] { 'a', 'c' },
				new[] { 'b', 'c' },
				new[] { 'a', 'b', 'c' },
				new[] { 'd' },
				new[] { 'a', 'd' },
				new[] { 'b', 'd' },
				new[] { 'a', 'b', 'd' },
				new[] { 'c', 'd' },
				new[] { 'a', 'c', 'd' },
				new[] { 'b', 'c', 'd' },
				new[] { 'a', 'b', 'c', 'd' },
			};

			char[][] output = input.Subsets().Select(x => x.ToArray()).ToArray();
			CollectionAssert.AreEqual(expected, output);
		}

		[Test]
		public void Subsets_SelfNull()
		{
			IEnumerable<char> s = null;
			Assert.Throws<ArgumentNullException>(() => s.Subsets());
		}

		[Test]
		public void Subsets_EmptySet()
		{
			IEnumerable<char> s = Enumerable.Empty<char>();
			CollectionAssert.IsEmpty(s.Subsets());
		}

		[Test]
		public void Subsets_MoreThan63Items()
		{
			IEnumerable<int> a = 1.UpTo(62);
			IEnumerable<int> b = 1.UpTo(63);
			IEnumerable<int> c = 1.UpTo(64);

			int[] expectedFirstResult = { 1 };

			//boundry tests
			CollectionAssert.AreEqual(expectedFirstResult, a.Subsets().First());
			CollectionAssert.AreEqual(expectedFirstResult, b.Subsets().First());

			var ex = Assert.Throws<InvalidOperationException>(() => c.Subsets().First());
			Assert.AreEqual("Cannot create subsets for more than 63 items, the source contained 64 items", ex.Message);
		}

		[Test]
		public void Subsets_Prune () {
			char[] input = { 'a', 'b', 'c', 'd' };
			char[][] expected = {
				new[] { 'a' },
				new[] { 'b' },
				new[] { 'b', 'a' },
				new[] { 'c' },
				new[] { 'c', 'b', 'a' },
				new[] { 'c', 'b' },
				new[] { 'c', 'a' },
				new[] { 'd' },
				new[] { 'd', 'c', 'a' },
				new[] { 'd', 'c' },
				new[] { 'd', 'a' },
			};

			char[][] output = input
				.Subsets (x => !(x.Contains ('b') && x.Contains ('d')))
				.Select (x => x.ToArray ()).ToArray ();

			CollectionAssert.AreEqual (expected, output);
		}

		[Test, ExpectedException (typeof (InvalidOperationException))]
		public void MaxBy_EmptySequenceThrowsInvalidOperation ()
		{
			new int[] { }.MaxBy(i => 0);
		}

		[Test]
		public void MaxBy_SingleItemSequenceDefaultComparerReturnsOnlyItem ()
		{
			Assert.AreEqual(42, new[] { 42 }.MaxBy(i => i));
		}

		[Test]
		public void MaxBy_TwoItemSequenceDefaultComparerReturnsMaxItem ()
		{
			Assert.AreEqual(42, new[] { 42, 41 }.MaxBy(i => i));
		}

		[Test]
		public void MaxBy_TwoItemSequenceInverseComparerReturnsMinItem ()
		{
			Assert.AreEqual(42, new[] { 42, 43 }.MaxBy(i => i, new LambdaComparer<int>((x, y) => y - x)));
		}

		[Test]
		public void MaxBy_TwoItemSequenceCustomValueReturnsMaxItem ()
		{
			#region MaxBy
			Assert.AreEqual ("forty-three",
					new[] {
						new {A = "forty-two",    B = 42},
						new {A = "forty-three",  B = 43},
					}.MaxBy (i => i.B).A);
			#endregion
		}

		[Test]
		public void MaxBy_ThreeItemSequenceCustomValueAndTwoIdenticalKeysReturnsFirstMaxItem ()
		{
			Assert.AreEqual ("first-max",
					new[] {
						new {A = "forty-two",   B = 42},
						new {A = "first-max",   B = 43},
						new {A = "forty-three", B = 43},
					}.MaxBy (i => i.B).A);
		}

		[Test, ExpectedException (typeof (InvalidOperationException))]
		public void MinBy_EmptySequenceThrowsInvalidOperation ()
		{
			new int [0].MinBy (i => 0);
		}

		[Test]
		public void MinBy_SingleItemSequenceDefaultComparerReturnsOnlyItem ()
		{
			Assert.AreEqual (42, new[]{ 42 }.MinBy (i => i));
		}

		[Test]
		public void MinBy_TwoItemSequenceDefaultComparerReturnsMainItem ()
		{
			Assert.AreEqual (42, new[] { 42, 43 }.MinBy (i => i));
		}

		[Test]
		public void MinBy_TwoItemSequenceInverseComparerReturnsMaxItem ()
		{
			Assert.AreEqual(42, new[] { 42, 41 }.MinBy (i => i, new LambdaComparer<int>((x, y) => y - x)));
		}

		[Test]
		public void MinBy_TwoItemSequenceCustomValueReturnsMinItem ()
		{
			#region MinBy
			Assert.AreEqual ("forty-two",
					new[] {
						new {A = "forty-two",   B = 42},
						new {A = "forty-three", B = 43},
					}.MinBy (i => i.B).A);
			#endregion
		}

		[Test]
		public void MinBy_ThreeItemSequenceCustomValueAndTwoIdenticalKeysReturnsFirstMinItem ()
		{
			Assert.AreEqual ("first-min",
					new[] {
						new {A = "first-min",   B = 42},
						new {A = "forty-two",   B = 42},
						new {A = "forty-three", B = 43},
					}.MinBy (i => i.B).A);
		}
	}
}
