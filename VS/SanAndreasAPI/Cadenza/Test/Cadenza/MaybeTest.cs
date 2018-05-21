//
// MaybeTest.cs
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

using NUnit.Framework;

using Cadenza;

// "The variable `r'/etc. is assigned but it's value is never used."
// It's value isn't supposed to be used; it's purpose is as a manual check 
// the return type.
#pragma warning disable 0219, 0168

namespace Cadenza.Tests {

	[TestFixture]
	public class MaybeEquatableContract : EquatableContract<Maybe<int>>
	{
		protected override Maybe<int> CreateValueX ()
		{
			return 1.ToMaybe ();
		}

		protected override Maybe<int> CreateValueY ()
		{
			return 2.ToMaybe ();
		}

		protected override Maybe<int> CreateValueZ ()
		{
			return Maybe<int>.Nothing;
		}
	}

	[TestFixture]
	public class MaybeTest : BaseRocksFixture {

		[Test, ExpectedException (typeof (InvalidOperationException))]
		public void Maybe_Nothing ()
		{
			Maybe<int> n = new Maybe<int> ();
			Assert.IsFalse (n.HasValue);
			Assert.AreEqual (n, Maybe<int>.Nothing);
			int x = n.Value;
		}

		[Test]
		public void TryParse ()
		{
			#region TryParse
			Maybe<int> n;

			n = Maybe.TryParse<int> (null);
			Assert.IsFalse (n.HasValue);

			n = Maybe.TryParse<int> ("");
			Assert.IsFalse (n.HasValue);

			n = Maybe.TryParse<int> ("foo");
			Assert.IsFalse (n.HasValue);

			n = Maybe.TryParse<int> ("42.01");
			Assert.IsFalse (n.HasValue);

			n = Maybe.TryParse<int> ("42");
			Assert.IsTrue (n.HasValue);
			Assert.AreEqual (42, n.Value);
			#endregion
		}

		[Test]
		public void TryConvert ()
		{
			#region TryConvert
			Maybe<string> a = Maybe.TryConvert<int, string> (42);
			Assert.IsTrue (a.HasValue);
			Assert.AreEqual ("42", a.Value);

			Maybe<DateTime> b = Maybe.TryConvert<int, DateTime> (42);
			Assert.IsFalse (b.HasValue);
			#endregion
		}

		[Test]
		public void When ()
		{
			#region When
			var r = Maybe.When (true, 42);
			Assert.IsTrue (r.HasValue);
			Assert.AreEqual (42, r.Value);

			r = Maybe.When (false, 42);
			Assert.IsFalse (r.HasValue);
			#endregion

			#region When_Creator
			bool invoked = false;
			r = Maybe.When (false, () => {invoked = true; return 42;});
			Assert.IsFalse (invoked);
			Assert.IsFalse (r.HasValue);

			r = Maybe.When (true, () => {invoked = true; return 42;});
			Assert.IsTrue (invoked);
			Assert.IsTrue (r.HasValue);
			Assert.AreEqual (42, r.Value);
			#endregion
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void When_CreatorNull_True ()
		{
			Func<int> f = null;
			var       r = Maybe.When (true, f);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void When_CreatorNull_False ()
		{
			Func<int> f = null;
			var       r = Maybe.When (false, f);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void Select_SelectorNull ()
		{
			Func<int, string> f = null;
			42.Just ().Select (f);
		}

		[Test]
		public void Select ()
		{
			#region Select
			Assert.AreEqual (2.Just (),
				1.Just ().Select (x => x + 1));
			Assert.AreEqual (2.Just (),
				from x in 1.Just ()
				select x + 1);
			#endregion
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void SelectMany_SelectorNull ()
		{
			Func<int, Maybe<int>>   s  = null;
			Func<int, int, string>  rs = (x, y) => (x+y).ToString ();
			42.ToMaybe ().SelectMany (s, rs);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void SelectMany_ResultSelectorNull ()
		{
			Func<int, Maybe<int>>    s  = x => 5.ToMaybe ();
			Func<int, int, string>   rs = null;
			42.ToMaybe ().SelectMany (s, rs);
		}

		[Test]
		public void SelectMany ()
		{
			#region SelectMany_QueryComprehension
			Assert.AreEqual (Maybe<int>.Nothing,
					from x in 5.ToMaybe ()
					from y in Maybe<int>.Nothing
					select x + y);
			Assert.AreEqual (9.ToMaybe (),
					from x in 5.ToMaybe ()
					from y in 4.ToMaybe ()
					select x + y);
			#endregion

			Assert.AreEqual (Maybe<int>.Nothing, 
					5.ToMaybe().SelectMany(x => Maybe<int>.Nothing,
						(x, y) => x + y));
			Assert.AreEqual (9.ToMaybe (),
					5.ToMaybe().SelectMany(x => 4.ToMaybe(),
						(x, y) => x + y));

			#region SelectMany_TCollection
			Assert.AreEqual (Maybe<int>.Nothing, 
					5.Just().SelectMany(
						x => Maybe<int>.Nothing,
						(x, y) => x + y));
			Assert.AreEqual (Maybe<int>.Nothing,
					from x in 5.Just ()
					from y in Maybe<int>.Nothing
					select x + y);
			Assert.AreEqual (9.Just (),
					5.Just().SelectMany(
						x => 4.Just (),
						(x, y) => x + y));
			Assert.AreEqual (9.Just (),
					from x in 5.Just ()
					from y in 4.Just ()
					select x + y);
			#endregion
		}

		[Test]
		public void Equals ()
		{
			Maybe<int> x = 1.ToMaybe ();
			Assert.IsTrue (x.Equals (x));
			Assert.IsFalse (x.Equals (null));
			Assert.IsFalse (x.Equals (Maybe<int>.Nothing));
			Assert.IsTrue (Maybe<int>.Nothing.Equals (Maybe<int>.Nothing));
			Maybe<int> y = 1.ToMaybe ();
			Assert.IsTrue (x.Equals (y));
			Assert.IsTrue (y.Equals (x));
			Maybe<int> z = 1.ToMaybe ();
			Assert.IsTrue (x.Equals (z));
			Assert.IsTrue (y.Equals (z));
			Maybe<int> w = 2.ToMaybe ();
			Assert.IsFalse (x.Equals (w));
			Assert.IsFalse (w.Equals (x));
		}

		[Test]
		public new void GetHashCode ()
		{
			Assert.AreEqual (0, Maybe<int>.Nothing.GetHashCode ());
			Assert.AreEqual (1.GetHashCode (),
					1.ToMaybe ().GetHashCode ());
		}

		[Test]
		public void GetValueOrDefault ()
		{
			var r = 42.ToMaybe ();
			Assert.AreEqual (42, r.GetValueOrDefault ());
			Assert.AreEqual (42, r.GetValueOrDefault (16));

			r = Maybe<int>.Nothing;
			Assert.AreEqual (0, r.GetValueOrDefault ());
			Assert.AreEqual (16, r.GetValueOrDefault (16));
		}

		[Test]
		public new void ToString ()
		{
			Assert.AreEqual (string.Empty, Maybe<int>.Nothing.ToString ());
			Assert.AreEqual ("42", 42.ToMaybe ().ToString ());
		}
	}
}
