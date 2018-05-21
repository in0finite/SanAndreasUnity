//
// EnumerableCoda.cs
//
// Author:
//   Jb Evain (jbevain@novell.com)
//   Jonathan Pryor  <jonp@xamarin.com>
//   Distilled Brilliance <contact@dispatcher.distilledb.com>
//   Eric Maupin <me@ermau.com>
//   Chris Howie <cdhowie@gmail.com>
//   Rik Hemsley <rik@rikkus.info>
//
// Copyright (c) 2007-2010 Novell, Inc. (http://www.novell.com)
// Copyright (c) 2010 Rik Hemsley
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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using Cadenza.IO;
using Cadenza.Numerics;

namespace Cadenza.Collections {

	public static class EnumerableCoda {

		public static SequenceComparison<T> CompareWith<T> (this IEnumerable<T> self, IEnumerable<T> update)
		{
			return new SequenceComparison<T> (self, update);
		}

		public static SequenceComparison<T> CompareWith<T> (this IEnumerable<T> self, IEnumerable<T> update, IEqualityComparer<T> comparer)
		{
			return new SequenceComparison<T> (self, update, comparer);
		}

		public static bool TryGetFirst<TSource> (this IEnumerable<TSource> self, out TSource first)
		{
			Check.Self (self);

			first = default(TSource);

			var list = (self as IList<TSource>);
			if (list != null)
			{
				if (list.Count > 0)
				{
					first = list[0];
					return true;
				}
				else
					return false;
			}

			using (var e = self.GetEnumerator ())
			{
				if (e.MoveNext ())
				{
					first = e.Current;
					return true;
				}
			}

			return false;
		}

		public static bool TryGetFirst<TSource> (this IEnumerable<TSource> self, Func<TSource, bool> predicate, out TSource first)
		{
			Check.Self (self);
			Check.Predicate (predicate);

			first = default(TSource);
			foreach (TSource item in self)
			{
				if (!predicate (item))
					continue;

				first = item;
				return true;
			}

			return false;
		}

		public static string Implode<TSource> (this IEnumerable<TSource> self, string separator)
		{
			return Implode (self, separator, e => e.ToString ());
		}

		public static string Implode<TSource> (this IEnumerable<TSource> self)
		{
			return Implode (self, null);
		}

		public static string Implode<TSource> (this IEnumerable<TSource> self, string separator, Func<TSource, string> selector)
		{
			Check.Self (self);
			Check.Selector (selector);

			var c = self as ICollection<TSource>;
			string[] values = new string [c != null ? c.Count : 10];
			int i = 0;
			foreach (var e in self) {
				if (values.Length == i)
					Array.Resize (ref values, i*2);
				values [i++] = selector (e);
			}
			if (i < values.Length)
				Array.Resize (ref values, i);
			return string.Join (separator, values);
		}

		public static IEnumerable<TSource> Repeat<TSource> (this IEnumerable<TSource> self, int number)
		{
			Check.Self (self);

			return CreateRepeatIterator (self, number);
		}

		private static IEnumerable<TSource> CreateRepeatIterator<TSource> (IEnumerable<TSource> self, int number)
		{
			for (int i = 0; i < number; i++) {
				foreach (var element in self)
					yield return element;
			}
		}

		public static string PathCombine (this IEnumerable<string> self)
		{
			Check.Self (self);

			char [] invalid = Path.GetInvalidPathChars ();
			char [] separators = { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar, Path.VolumeSeparatorChar };

			StringBuilder sb = null;
			string previous = null;
			foreach (string s in self) {
				if (s == null)
					throw new ArgumentNullException ("path");
				if (s.Length == 0)
					continue;
				if (s.IndexOfAny (invalid) != -1)
					throw new ArgumentException ("Illegal character in path");

				if (sb == null) {
					sb = new StringBuilder (s);
					previous = s;
				} else {
					if (Path.IsPathRooted (s)) {
						sb = new StringBuilder (s);
						continue;
					}

					char last = ((IEnumerable<char>) previous).Last ();
					if (!separators.Contains (last))
						sb.Append (Path.DirectorySeparatorChar);

					sb.Append (s);
					previous = s;
				}
			}
			return (sb == null) ? String.Empty : sb.ToString ();
		}

		public static void ForEach<TSource> (this IEnumerable<TSource> self, Action<TSource> action)
		{
			Check.Self (self);
			if (action == null)
				throw new ArgumentNullException ("action");

			foreach (var item in self)
				action (item);
		}

		public static void ForEach<TSource> (this IEnumerable<TSource> self, Action<TSource, int> action)
		{
			Check.Self (self);
			if (action == null)
				throw new ArgumentNullException ("action");

			int index = 0;
			foreach (var item in self)
				action (item, checked (index++));
		}

		public static IEnumerable<TSource> Each<TSource> (this IEnumerable<TSource> self, Action<TSource> action)
		{
			Check.Self (self);
			if (action == null)
				throw new ArgumentNullException ("action");

			return CreateEachIterator (self, action);
		}

		private static IEnumerable<TSource> CreateEachIterator<TSource> (IEnumerable<TSource> self, Action<TSource> action)
		{
			foreach (var item in self) {
				action (item);
				yield return item;
			}
		}

		public static IEnumerable<TSource> Each<TSource> (this IEnumerable<TSource> self, Action<TSource, int> action)
		{
			Check.Self (self);
			if (action == null)
				throw new ArgumentNullException ("action");

			return CreateEachIterator (self, action);
		}

		private static IEnumerable<TSource> CreateEachIterator<TSource> (IEnumerable<TSource> self, Action<TSource, int> action)
		{
			int index = 0;
			foreach (var item in self) {
				action (item, checked(index++));
				yield return item;
			}
		}

		public static void Apply<TSource> (this IEnumerable<TSource> self)
		{
			Check.Self (self);
#pragma warning disable 168, 219
			foreach (var item in self) {
				// ignore t
			}
#pragma warning restore 168, 219
		}

		public static IEnumerable<TSource> ApplyPairs<TSource> (this IEnumerable<TSource> self, params Action<TSource>[] actions)
		{
			Check.Self (self);
			if (actions == null)
				throw new ArgumentNullException ("actions");

			return CreateApplyPairsIterator (self, actions);
		}

		private static IEnumerable<TSource> CreateApplyPairsIterator<TSource> (IEnumerable<TSource> self, IEnumerable<Action<TSource>> actions)
		{
			using (IEnumerator<TSource> s = self.GetEnumerator ())
			using (IEnumerator<Action<TSource>> a = actions.GetEnumerator ()) {
				bool have_s;
				while ((have_s = s.MoveNext ()) && a.MoveNext ()) {
					a.Current (s.Current);
					yield return s.Current;
				}

				if (have_s)
					while (s.MoveNext ())
						yield return s.Current;
			}
		}

		public static IEnumerable<TSource> Sort<TSource> (this IEnumerable<TSource> self)
		{
			Check.Self (self);

			List<TSource> s = self.ToList ();
			s.Sort ();
			return s;
		}

		public static IEnumerable<TSource> Sort<TSource> (this IEnumerable<TSource> self, Comparison<TSource> comparison)
		{
			Check.Self (self);
			if (comparison == null)
				throw new ArgumentNullException ("comparison");

			List<TSource> s = self.ToList ();
			s.Sort (comparison);
			return s;
		}

		public static IEnumerable<TSource> Sort<TSource> (this IEnumerable<TSource> self, IComparer<TSource> comparer)
		{
			Check.Self (self);

			List<TSource> s = self.ToList ();
			s.Sort (comparer);
			return s;
		}

		public static IEnumerable<TSource> OrderByNatural<TSource> (this IEnumerable<TSource> self, Func<TSource, string> func)
		{
			Check.SelfAndFunc (self, func);

			return self.OrderBy (func, NaturalStringComparer.Default);
		}

		public static IEnumerable<string> SortNatural (this IEnumerable<string> self)
		{
			return Sort (self, NaturalStringComparer.Default);
		}

		public static CachedSequence<T> Cache<T> (this IEnumerable<T> self)
		{
			Check.Self (self);

			return new CachedSequence<T> (self);
		}

		public static object ToTuple (this IEnumerable self)
		{
			Check.Self (self);

			List<Type> types;
			List<object> args;
			ICollection c = self as ICollection;
			if (c != null) {
				types = new List<Type> (c.Count);
				args  = new List<object> (c.Count);
			}
			else {
				types = new List<Type> ();
				args  = new List<object> ();
			}
			foreach (var val in self) {
				types.Add (val.GetType ());
				args.Add (val);
			}
			Type tuple = Assembly.GetExecutingAssembly().GetType (
				"Cadenza.Tuple`" + types.Count, 
				false
			) ?? Type.GetType ("System.Tuple`" + types.Count, false);
			if (tuple == null)
				throw new NotSupportedException (
						string.Format ("Tuples with {0} values are not supported.", types.Count));
			tuple = tuple.MakeGenericType (types.ToArray ());
			return Activator.CreateInstance (tuple, args.ToArray ());
		}

		public static int SequenceCompare<TSource> (this IEnumerable<TSource> self, IEnumerable<TSource> list)
		{
			return SequenceCompare (self, list, null);
		}

		public static int SequenceCompare<TSource> (this IEnumerable<TSource> self, IEnumerable<TSource> list, IComparer<TSource> comparer)
		{
			Check.Self (self);
			Check.List (list);
			comparer = comparer ?? Comparer<TSource>.Default;

			using (var se = self.GetEnumerator ())
			using (var le = list.GetEnumerator ()) {
				bool hs = se.MoveNext (), hl = le.MoveNext ();
				for ( ; hs && hl; hs = se.MoveNext (), hl = le.MoveNext ()) {
					int c = comparer.Compare (se.Current, le.Current);
					if (c != 0)
						return c;
				}
				if (hs)
					return -1;
				if (hl)
					return 1;
				return 0;
			}
		}

		public static IEnumerable<TSource> Shuffle<TSource> (this IEnumerable<TSource> self)
		{
			return Shuffle (self, null);
		}

		public static IEnumerable<TSource> Shuffle<TSource> (this IEnumerable<TSource> self, Random random)
		{
			Check.Self (self);
			if (random == null)
				random = new Random ();

			return CreateShuffleIterator (self, random);
		}

		private static IEnumerable<TSource> CreateShuffleIterator<TSource> (IEnumerable<TSource> self, Random random)
		{
			IList<TSource> values = self.AsIList ();

			int[] indices = new int [values.Count];
			for (int i = 0; i < indices.Length; ++i)
				indices [i] = i;

			for (int i = indices.Length-1; i > 0; i--)
				Swap (ref indices [i], ref indices [random.Next (i+1)]);

			foreach (int i in indices)
				yield return values [i];
		}

		static void Swap<T> (ref T a, ref T b)
		{
			T t = a;
			a = b;
			b = t;
		}

		public static IEnumerable<TResult> Tokens<TSource, TAccumulate, TResult> (this IEnumerable<TSource> self, 
				TAccumulate seed, 
				Func<TAccumulate, TSource, TAccumulate> accumulate,
				Func<TAccumulate, Tuple<TResult, TAccumulate>> resultSelector, 
				params Func<TAccumulate, TSource, bool>[] categories)
		{
			Check.Self (self);
			Check.Accumulate (accumulate);
			Check.ResultSelector (resultSelector);
			Check.Categories (categories);
			if (categories.Length == 0)
				throw new ArgumentException ("must have one or more elements", "categories");

			return CreateTokensIterator (self, seed, accumulate, resultSelector, categories);
		}

		private static IEnumerable<TResult> CreateTokensIterator<TSource, TAccumulate, TResult> (
				IEnumerable<TSource> self, 
				TAccumulate seed, 
				Func<TAccumulate, TSource, TAccumulate> accumulate,
				Func<TAccumulate, Tuple<TResult, TAccumulate>> resultSelector, 
				Func<TAccumulate, TSource, bool>[] categories)
		{
			bool have_data = false;
			int cat = -1;
			var get_next_cat = Lambda.F<TSource, int> (s => categories
					.Select ((l, i) => l (seed, s) ? i : -1)
					.Where (n => n >= 0)
					.With (e => e.Any () ? e.Min () : -1));
			var accum = Lambda.A<TSource, int> ((s, c) => {
					seed      = accumulate (seed, s);
					cat       = c;
					have_data = true;
			});
			foreach (var s in self) {
				int next_cat = get_next_cat (s);
				if (next_cat == cat && cat >= 0)
					accum (s, next_cat);
				else if (next_cat >= 0) {
					if (have_data) {
						var r = resultSelector (seed);
						yield return r.Item1;
						seed = r.Item2;
					}
					accum (s, next_cat);
				}
				else if (have_data) {
					var r = resultSelector (seed);
					yield return r.Item1;
					seed = r.Item2;
					cat  = -1;
					have_data = false;
					// retry
					if ((next_cat = get_next_cat (s)) >= 0)
						accum (s, next_cat);
				}
				else {
					cat = -1;
					have_data = false;
				}
			}
			if (have_data)
				yield return resultSelector (seed).Item1;
		}

		public static ReadOnlyDictionary<TKey, TSource> ToReadOnlyDictionary<TSource, TKey> (this IEnumerable<TSource> self, Func<TSource, TKey> keySelector)
		{
			Check.Self (self);
			return new ReadOnlyDictionary<TKey, TSource> (self.ToDictionary (keySelector));
		}

		public static ReadOnlyDictionary<TKey, TValue> ToReadOnlyDictionary<TSource,TKey,TValue> (this IEnumerable<TSource> self, Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector)
		{
			Check.Self (self);
			return new ReadOnlyDictionary<TKey, TValue> (self.ToDictionary (keySelector, valueSelector));
		}

		[CLSCompliant (false)]
		public static TextValueReader ToValueReader (this IEnumerable<string> self)
		{
			Check.Self (self);

			return new TextValueReader (self);
		}

		[CLSCompliant (false)]
		public static EnumerableValueReader<TSource> ToValueReader<TSource> (this IEnumerable<TSource> self)
		{
			Check.Self (self);

			return new EnumerableValueReader<TSource> (self);
		}

		public static IEnumerable<TResult> SelectBreadthFirst<TSource, TResult> (
				this IEnumerable<TSource> self, 
				Func<TSource, TResult> valueSelector,
				Func<TSource, IEnumerable<TSource>> childrenSelector)
		{
			Check.Self (self);
			Check.ValueSelector (valueSelector);
			Check.ChildrenSelector (childrenSelector);

			return self.SelectMany (e => e.TraverseBreadthFirst (valueSelector, childrenSelector));
		}

		public static IEnumerable<TResult> SelectDepthFirst<TSource, TResult> (
				this IEnumerable<TSource> self, 
				Func<TSource, TResult> valueSelector,
				Func<TSource, IEnumerable<TSource>> childrenSelector)
		{
			Check.Self (self);
			Check.ValueSelector (valueSelector);
			Check.ChildrenSelector (childrenSelector);

			return self.SelectMany (e => e.TraverseDepthFirst (valueSelector, childrenSelector));
		}

		/*
		 * ContiguousSubsequences courtesy of Distilled Brilliance / John Feminella:
		 * http://distilledb.com/blog/archives/date/2009/03/12/enumerable-extension-method-for-contiguous-subsequences.page
		 */
		public static IEnumerable<IEnumerable<TSource>> ContiguousSubsequences<TSource>(this IEnumerable<TSource> self, int windowSize)
		{
			Check.Self (self);
			if (windowSize < 1)
				throw new ArgumentOutOfRangeException("windowSize", "must be >= 1");

			return CreateContiguousSubsequencesIterator (self, windowSize);
		}

		private static IEnumerable<IEnumerable<T>> CreateContiguousSubsequencesIterator<T>(this IEnumerable<T> input, int windowSize)
		{
			int index = 0;
			var window = new List<T>(windowSize);
			window.AddRange (new T[windowSize]);
			foreach (var item in input) {
				bool initializing = index < windowSize;

				if (!initializing) {
					window = window.Skip (1).ToList ();
					window.Add (default (T));
				}

				int itemIndex = initializing ? index : windowSize - 1;
				window [itemIndex] = item;

				index++;
				bool initialized = index >= windowSize;
				if (initialized)
					yield return new List<T>(window);
			}
		}

		public static IEnumerable<TSource> NotNull<TSource> (this IEnumerable<TSource?> self)
			where TSource : struct
		{
			Check.Self (self);

			return CreateNotNullIterator (self);
		}

		static IEnumerable<TSource> CreateNotNullIterator<TSource> (IEnumerable<TSource?> self)
			where TSource : struct
		{
			foreach (var e in self)
				if (e.HasValue)
					yield return e.Value;
		}

		public static void CopyTo<TSource> (this IEnumerable<TSource> self, TSource[] array, int arrayIndex)
		{
			Check.Self (self);
			if (array == null)
				throw new ArgumentNullException ("array");
			if (arrayIndex < 0)
				throw new ArgumentOutOfRangeException ("arrayIndex", "arrayIndex is negative.");
			if (arrayIndex >= array.Length)
				throw new ArgumentOutOfRangeException ("arrayIndex", "arrayIndex is larger than list.Count.");

			foreach (var e in self)
				array [arrayIndex++] = e;
		}

		public static void CopyTo<TSource> (this IEnumerable<TSource> self, ICollection<TSource> destination)
		{
			// warning: not exception safe
			Check.Self (self);
			if (destination == null)
				throw new ArgumentNullException ("destination");

			foreach (var e in self)
				destination.Add (e);
		}

		public static TSource Sum<TSource> (this IEnumerable<TSource> self)
		{
			return Sum (self, null);
		}

		public static TSource Sum<TSource> (this IEnumerable<TSource> self, Math<TSource> m)
		{
			Check.Self (self);
			m = m ?? Math<TSource>.Default;

			var sum = m.FromInt32 (0);
			foreach (var e in self) {
				sum = m.Add (sum, e);
			}
			return sum;
		}

		// Haskell: zipWith
		public static IEnumerable<TResult> 
			SelectFromEach<T1, T2, TResult> (
					this IEnumerable<T1> self,
					IEnumerable<T2> source2, 
					Func<T1, T2, TResult> selector)
		{
			Check.Self (self);
			if (source2 == null)
				throw new ArgumentNullException ("source2");
			if (selector == null)
				throw new ArgumentNullException ("selector");

			return CreateSelectFromEachIterator (self, source2, selector);
		}

		private static IEnumerable<TResult>
			CreateSelectFromEachIterator<T1, T2, TResult> (
					IEnumerable<T1> self, 
					IEnumerable<T2> source2, 
					Func<T1, T2, TResult> selector)
		{
			using (IEnumerator<T1>  a = self.GetEnumerator ())
			using (IEnumerator<T2>  b = source2.GetEnumerator ()) {
				while (a.MoveNext () && b.MoveNext ()) {
					yield return selector (a.Current, b.Current);
				}
			}
		}

		// Haskell: zipWith3
		public static IEnumerable<TResult> 
			SelectFromEach<T1, T2, T3, TResult> (
					this IEnumerable<T1> self,
					IEnumerable<T2> source2, 
					IEnumerable<T3> source3,
					Func<T1, T2, T3, TResult> selector)
		{
			Check.Self (self);
			if (source2 == null)
				throw new ArgumentNullException ("source2");
			if (source3 == null)
				throw new ArgumentNullException ("source3");
			if (selector == null)
				throw new ArgumentNullException ("selector");

			return CreateSelectFromEachIterator (self, source2, source3, selector);
		}

		private static IEnumerable<TResult>
			CreateSelectFromEachIterator<T1, T2, T3, TResult> (
					IEnumerable<T1> self, 
					IEnumerable<T2> source2, 
					IEnumerable<T3> source3, 
					Func<T1, T2, T3, TResult> selector)
		{
			using (IEnumerator<T1>  a = self.GetEnumerator ())
			using (IEnumerator<T2>  b = source2.GetEnumerator ())
			using (IEnumerator<T3>  c = source3.GetEnumerator ()) {
				while (a.MoveNext () && b.MoveNext () && c.MoveNext ()) {
					yield return selector (a.Current, b.Current, c.Current);
				}
			}
		}

		// Haskell: zipWith4
		public static IEnumerable<TResult> 
			SelectFromEach<T1, T2, T3, T4, TResult> (
					this IEnumerable<T1> self,
					IEnumerable<T2> source2, 
					IEnumerable<T3> source3, 
					IEnumerable<T4> source4, 
					Func<T1, T2, T3, T4, TResult> selector)
		{
			if (self == null)
				throw new ArgumentNullException ("self");
			if (source2 == null)
				throw new ArgumentNullException ("source2");
			if (source3 == null)
				throw new ArgumentNullException ("source3");
			if (source4 == null)
				throw new ArgumentNullException ("source4");
			if (selector == null)
				throw new ArgumentNullException ("selector");

			return CreateSelectFromEachIterator (self, source2, source3, source4, selector);
		}

		private static IEnumerable<TResult>
			CreateSelectFromEachIterator<T1, T2, T3, T4, TResult> (
					IEnumerable<T1> self, 
					IEnumerable<T2> source2, 
					IEnumerable<T3> source3, 
					IEnumerable<T4> source4, 
					Func<T1, T2, T3, T4, TResult> selector)
		{
			using (IEnumerator<T1>  a = self.GetEnumerator ())
			using (IEnumerator<T2>  b = source2.GetEnumerator ())
			using (IEnumerator<T3>  c = source3.GetEnumerator ())
			using (IEnumerator<T4>  d = source4.GetEnumerator ()) {
				while (a.MoveNext () && b.MoveNext () && c.MoveNext () && d.MoveNext ()) {
					yield return selector (a.Current, b.Current, c.Current, d.Current);
				}
			}
		}

		// Haskell init
		public static IEnumerable<TSource> ExceptLast<TSource> (this IEnumerable<TSource> self)
		{
			return ExceptLast (self, 1);
		}

		public static IEnumerable<TSource> ExceptLast<TSource> (this IEnumerable<TSource> self, int count)
		{
			Check.Self (self);
			if (count < 0)
				throw new ArgumentException ("count", "must be >= 0");
			if (count == 0)
				return self;

			return CreateExceptLastIterator (self, count);
		}

		private static IEnumerable<TSource> CreateExceptLastIterator<TSource> (IEnumerable<TSource> self, int count)
		{
			Queue<TSource> ignore = new Queue<TSource> (count);
			foreach (TSource e in self) {
				if (ignore.Count < count)
					ignore.Enqueue (e);
				else if (ignore.Count == count) {
					yield return ignore.Dequeue ();
					ignore.Enqueue (e);
				}
				else
					throw new InvalidOperationException ("should not happen");
			}
		}

		// Haskell intersperse
		public static IEnumerable<TSource> Intersperse<TSource> (this IEnumerable<TSource> self, TSource value)
		{
			Check.Self (self);
			return CreateIntersperseIterator (self, value);
		}

		private static IEnumerable<TSource> CreateIntersperseIterator<TSource> (IEnumerable<TSource> self, TSource value)
		{
			bool insert = false;
			foreach (TSource v in self) {
				if (insert)
					yield return value;
				yield return v;
				insert = true;
			}
		}

		// Haskell intercalate
		public static IEnumerable<TSource> Intersperse<TSource> (this IEnumerable<IEnumerable<TSource>> self, IEnumerable<TSource> between)
		{
			Check.Self (self);
			if (between == null)
				throw new ArgumentNullException ("lists");

			IEnumerable<IEnumerable<TSource>> e = Intersperse<IEnumerable<TSource>> (self, between);
			return Concat (Enumerable.Empty<TSource>(), e);
		}

		// Haskell transpose
		public static IEnumerable<IEnumerable<TSource>> Transpose<TSource> (this IEnumerable<IEnumerable<TSource>> self)
		{
			Check.Self (self);

			IList<IList<TSource>> items = self as IList<IList<TSource>>;
			if (items == null) {
				items = (IList<IList<TSource>>) self.ToList().Select(x => (IList<TSource>) x).ToList();
			}

			int max = 0;
			for (int i = 0; i < items.Count; ++i)
				max = System.Math.Max (max, items [i].Count);

			return CreateTransposeIterator (items, max);
		}

		private static IEnumerable<IEnumerable<TSource>> CreateTransposeIterator<TSource> (IList<IList<TSource>> items, int max)
		{
			for (int j = 0; j < max; ++j)
				yield return CreateTransposeColumnIterator (items, j);
		}

		private static IEnumerable<TSource> CreateTransposeColumnIterator<TSource> (IList<IList<TSource>> items, int column)
		{
			for (int i = 0; i < items.Count; ++i) {
				if (items [i].Count <= column)
					continue;
				yield return items [i][column];
			}
		}

		public static List<List<TSource>> ToList<TSource> (this IEnumerable<IEnumerable<TSource>> self)
		{
			Check.Self (self);

			List<List<TSource>> r = new List<List<TSource>> ();
			foreach (IEnumerable<TSource> row in self) {
				List<TSource> items = new List<TSource> ();
				r.Add (items);
				foreach (TSource item in row) {
					items.Add (item);
				}
			}
			return r;
		}

		#region AggregateReverse

		// Haskell: foldr1
		public static TSource AggregateReverse<TSource> (this IEnumerable<TSource> self, Func<TSource, TSource, TSource> func)
		{
			Check.SelfAndFunc (self, func);

			IList<TSource> s = self.AsIList ();
			if (s.Count == 0)
				throw new InvalidOperationException ("No elements in self list");

			TSource folded = s [s.Count - 1];
			for (int i = s.Count-2; i >= 0; --i) {
				folded = func (folded, s [i]);
			}
			return folded;
		}

		public static IList<TSource> AsIList<TSource> (this IEnumerable<TSource> self)
		{
			Check.Self (self);
			IList<TSource> s = self as IList<TSource>;
			if (s == null) {
				s = new List<TSource> (self);
			}

			return s;
		}

		// Haskell: foldr
		public static TAccumulate AggregateReverse<TSource, TAccumulate> (this IEnumerable<TSource> self,
			TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
		{
			Check.SelfAndFunc (self, func);

			IList<TSource> s = self.AsIList ();

			TAccumulate folded = seed;
			for (int i = s.Count-1; i >= 0; --i) {
				folded = func (folded, s [i]);
			}

			return folded;
		}

		public static TResult AggregateReverse<TSource, TAccumulate, TResult> (this IEnumerable<TSource> self, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
		{
			Check.SelfAndFunc (self, func);
			if (resultSelector == null)
				throw new ArgumentNullException ("resultSelector");

			IList<TSource> s = self.AsIList ();

			var result = seed;
			for (int i = s.Count-1; i >= 0; --i)
				result = func (result, s [i]);

			return resultSelector (result);
		}

		#endregion

		// Haskell concat
		public static IEnumerable<TSource> Concat<TSource> (this IEnumerable<TSource> self, params IEnumerable<TSource>[] selfs)
		{
			IEnumerable<IEnumerable<TSource>> e = selfs;
			return Concat (self, e);
		}

		public static IEnumerable<TSource> Concat<TSource> (this IEnumerable<TSource> self, IEnumerable<IEnumerable<TSource>> selfs)
		{
			Check.Self (self);
			if (selfs == null)
				throw new ArgumentNullException ("selfs");

			return CreateConcatIterator (self, selfs);
		}

		private static IEnumerable<TSource> CreateConcatIterator<TSource> (IEnumerable<TSource> self, IEnumerable<IEnumerable<TSource>> selfs)
		{
			foreach (TSource e in self)
				yield return e;
			foreach (IEnumerable<TSource> outer in selfs) {
				foreach (TSource e in outer)
					yield return e;
			}
		}

		// Haskell: and
		public static bool And (this IEnumerable<bool> self)
		{
			Check.Self (self);

			foreach (bool e in self)
				if (!e)
					return false;

			return true;
		}

		// Haskell: or
		public static bool Or (this IEnumerable<bool> self)
		{
			Check.Self (self);

			foreach (bool e in self)
				if (e)
					return true;

			return false;
		}

		#region AggregateHistory

		// Haskell: scanl1
		public static IEnumerable<TSource> AggregateHistory<TSource> (this IEnumerable<TSource> self, Func<TSource, TSource, TSource> func)
		{
			Check.SelfAndFunc (self, func);

			return CreateAggregateHistoryIterator (self, func);
		}

		private static IEnumerable<TSource> CreateAggregateHistoryIterator<TSource> (IEnumerable<TSource> self, Func<TSource, TSource, TSource> func)
		{
			// custom foreach so that we can efficiently throw an exception
			// if zero elements and treat the first element differently
			using (var enumerator = self.GetEnumerator ()) {
				if (!enumerator.MoveNext ())
					throw new InvalidOperationException ("No elements in self list");

				TSource folded;
				yield return (folded = enumerator.Current);
				while (enumerator.MoveNext ())
					yield return (folded = func (folded, enumerator.Current));

				enumerator.Dispose ();
			}
		}

		// Haskell: scanl
		public static IEnumerable<TAccumulate> AggregateHistory<TSource, TAccumulate> (this IEnumerable<TSource> self,
			TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
		{
			Check.SelfAndFunc (self, func);

			return CreateAggregateHistoryIterator (self, seed, func);
		}

		private static IEnumerable<TAccumulate> CreateAggregateHistoryIterator<TSource, TAccumulate> (IEnumerable<TSource> self,
			TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
		{
			TAccumulate folded;
			yield return (folded = seed);
			foreach (TSource element in self)
				yield return (folded = func (folded, element));
		}

		public static IEnumerable<TResult> AggregateHistory<TSource, TAccumulate, TResult> (this IEnumerable<TSource> self, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
		{
			Check.SelfAndFunc (self, func);
			if (resultSelector == null)
				throw new ArgumentNullException ("resultSelector");

			return CreateAggregateHistoryIterator (self, seed, func, resultSelector);
		}

		private static IEnumerable<TResult> CreateAggregateHistoryIterator<TSource, TAccumulate, TResult> (IEnumerable<TSource> self, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
		{
			var result = seed;
			yield return resultSelector (result);
			foreach (var e in self)
				yield return resultSelector (result = func (result, e));
		}

		#endregion

		#region AggregateReverseHistory

		// Haskell: scanr1
		public static IEnumerable<TSource> AggregateReverseHistory<TSource> (this IEnumerable<TSource> self, Func<TSource, TSource, TSource> func)
		{
			Check.SelfAndFunc (self, func);

			return CreateAggregateReverseHistoryIterator (self, func);
		}

		private static IEnumerable<TSource> CreateAggregateReverseHistoryIterator<TSource> (IEnumerable<TSource> self, Func<TSource, TSource, TSource> func)
		{
			IList<TSource> s = self.AsIList ();
			if (s.Count == 0)
				throw new InvalidOperationException ("No elements in self list");

			TSource folded;
			yield return (folded = s [s.Count - 1]);
			for (int i = s.Count-2; i >= 0; --i)
				yield return (folded = func (folded, s [i]));
		}

		// Haskell: scanr
		public static IEnumerable<TAccumulate> AggregateReverseHistory<TSource, TAccumulate> (this IEnumerable<TSource> self,
			TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
		{
			Check.SelfAndFunc (self, func);

			return CreateAggregateReverseHistoryIterator (self, seed, func);
		}

		private static IEnumerable<TAccumulate> CreateAggregateReverseHistoryIterator<TSource, TAccumulate> (IEnumerable<TSource> self,
			TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
		{
			IList<TSource> s = self.AsIList ();

			TAccumulate folded;
			yield return (folded = seed);
			for (int i = s.Count-1; i >= 0; --i)
				yield return (folded = func (folded, s [i]));
		}

		public static IEnumerable<TResult> AggregateReverseHistory<TSource, TAccumulate, TResult> (this IEnumerable<TSource> self, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
		{
			Check.SelfAndFunc (self, func);
			if (resultSelector == null)
				throw new ArgumentNullException ("resultSelector");

			return CreateAggregateReverseHistoryIterator (self, seed, func, resultSelector);
		}

		private static IEnumerable<TResult> CreateAggregateReverseHistoryIterator<TSource, TAccumulate, TResult> (IEnumerable<TSource> self, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
		{
			IList<TSource> s = self.AsIList ();

			var result = seed;
			yield return resultSelector (result);
			for (int i = s.Count-1; i >= 0; --i)
				yield return resultSelector (result = func (result, s [i]));
		}

		#endregion
		
		// Haskell: mapAccumL
		public static Tuple<TAccumulate, List<TResult>> SelectAggregated<TSource, TAccumulate, TResult> (this IEnumerable<TSource> self, TAccumulate seed, Func<TAccumulate, TSource, Tuple<TAccumulate,TResult>> func)
		{
			Check.SelfAndFunc (self, func);

			ICollection<TSource> cself = self as ICollection<TSource>;
			var aggregates = cself != null 
				?  new List<TResult> (cself.Count)
				:  new List<TResult> ();

			var result = seed;
			foreach (var element in self) {
				var r = func (result, element);
				result = r.Item1;
				aggregates.Add (r.Item2);
			}

			return new Tuple<TAccumulate, List<TResult>> (result, aggregates);
		}

		// Haskell: mapAccumR
		public static Tuple<TAccumulate, List<TResult>> SelectReverseAggregated<TSource, TAccumulate, TResult> (this IEnumerable<TSource> self, TAccumulate seed, Func<TAccumulate, TSource, Tuple<TAccumulate,TResult>> func)
		{
			Check.SelfAndFunc (self, func);

			var s = self.AsIList ();
			var aggregates = new List<TResult> (s.Count);
			var result = seed;

			for (int i = s.Count-1; i >= 0; --i) {
				var r = func (result, s [i]);
				result = r.Item1;
				aggregates.Add (r.Item2);
			}

			return new Tuple<TAccumulate, List<TResult>> (result, aggregates);
		}

		// Haskell: cycle
		public static IEnumerable<TSource> Cycle<TSource> (this IEnumerable<TSource> self)
		{
			Check.Self (self);
			return CreateCycleIterator (self);
		}

		private static IEnumerable<TSource> CreateCycleIterator<TSource> (IEnumerable<TSource> self)
		{
			while (true)
				foreach (var e in self)
					yield return e;
		}

		// Haskell: splitAt
		public static Tuple<IEnumerable<TSource>, IEnumerable<TSource>> SplitAt<TSource> (this IEnumerable<TSource> self, int firstLength)
		{
			Check.Self (self);

			if (firstLength < 0)
				throw new ArgumentOutOfRangeException ("firstLength", "must not be negative");

			return Tuple.Create (self.Take (firstLength), self.Skip (firstLength));
		}

		// Haskell: span
		public static Tuple<IEnumerable<TSource>, IEnumerable<TSource>> Span<TSource> (this IEnumerable<TSource> self, Func<TSource, bool> predicate)
		{
			Check.Self (self);
			Check.Predicate (predicate);

			return Tuple.Create (self.TakeWhile (predicate), self.SkipWhile (predicate));
		}

		// Haskell: break
		public static Tuple<IEnumerable<TSource>, IEnumerable<TSource>> Break<TSource> (this IEnumerable<TSource> self, Func<TSource, bool> func)
		{
			Check.SelfAndFunc (self, func);

			return Tuple.Create (self.TakeWhile (e => !func (e)), self.SkipWhile (e => !func (e)));
		}

		// Haskell: stripPrefix
		public static IEnumerable<TSource> SkipPrefix<TSource> (this IEnumerable<TSource> self, IEnumerable<TSource> prefix)
		{
			return SkipPrefix (self, prefix, null);
		}

		public static IEnumerable<TSource> SkipPrefix<TSource> (this IEnumerable<TSource> self, IEnumerable<TSource> prefix, IEqualityComparer<TSource> comparer)
		{
			Check.Self (self);
			if (prefix == null)
				throw new ArgumentNullException ("prefix");
			comparer = comparer ?? EqualityComparer<TSource>.Default;

			using (IEnumerator<TSource> s = self.GetEnumerator ())
			using (IEnumerator<TSource> p = prefix.GetEnumerator ()) {
				int c = 0;
				bool have_s = s.MoveNext(), have_p = p.MoveNext(), have_match = true;
				do {
					++c;
					if ((have_p && !have_s) || !comparer.Equals (s.Current, p.Current)) {
						have_match = false;
						break;
					}
				} while ((have_s = s.MoveNext ()) && (have_p = p.MoveNext ()));
				if (have_match)
					return self.Skip (c);
				return null;
			}
		}

		// Haskell: group
		public static IEnumerable<IEnumerable<TSource>> HaskellGroup<TSource> (this IEnumerable<TSource> self)
		{
			return HaskellGroupBy (self, (a, b) => EqualityComparer<TSource>.Default.Equals (a, b));
		}

		// Haskell: inits
		public static IEnumerable<IEnumerable<TSource>> InitialSegments<TSource> (this IEnumerable<TSource> self)
		{
			Check.Self (self);

			return CreateInitialSegmentsIterator (self);
		}

		private static IEnumerable<IEnumerable<TSource>> CreateInitialSegmentsIterator<TSource> (IEnumerable<TSource> self)
		{
			var e = new List<TSource> ();
			yield return e;
			using (IEnumerator<TSource> s = self.GetEnumerator ()) {
				while (s.MoveNext ()) {
					e.Add (s.Current);
					yield return e;
				}
			}
		}

		// Haskell: tails
		public static IEnumerable<IEnumerable<TSource>> TrailingSegments<TSource> (this IEnumerable<TSource> self)
		{
			Check.Self (self);

			return CreateTrailingSegmentsIterator (self);
		}

		private static IEnumerable<IEnumerable<TSource>> CreateTrailingSegmentsIterator<TSource> (IEnumerable<TSource> self)
		{
			var e = self.ToList ();
			yield return e;
			while (e.Count > 0) {
				e.RemoveAt (e.Count-1);
				yield return e;
			}
		}

		// Haskell: partition
		public static Tuple<IEnumerable<TSource>, IEnumerable<TSource>> Partition<TSource> (this IEnumerable<TSource> self, Func<TSource, bool> predicate)
		{
			Check.Self (self);
			Check.Predicate (predicate);

			return Tuple.Create (
					self.Where (predicate), 
					self.Where (e => !predicate (e)));
		}

		// Haskell: elemIndex
		public static int IndexOf<TSource> (this IEnumerable<TSource> self, TSource value)
		{
			Check.Self (self);

			return FindIndex (self, v => EqualityComparer<TSource>.Default.Equals (v, value));
		}

		public static int IndexOfAny<TSource> (this IEnumerable<TSource> self, params TSource[] values)
		{
			IEnumerable<TSource> v = values;

			return IndexOfAny (self, v);
		}

		public static int IndexOfAny<TSource> (this IEnumerable<TSource> self, IEnumerable<TSource> values)
		{
			Check.Self (self);
			Check.Values (values);

			return FindIndex (self, v => values.Contains (v));
		}

		// Haskell: elemIndices
		public static IEnumerable<int> IndicesOf<TSource> (this IEnumerable<TSource> self, TSource value)
		{
			Check.Self (self);

			return FindIndices (self, v => EqualityComparer<TSource>.Default.Equals (v, value));
		}

		public static IEnumerable<int> IndicesOfAny<TSource> (this IEnumerable<TSource> self, params TSource[] values)
		{
			IEnumerable<TSource> v = values;

			return IndicesOfAny (self, v);
		}

		public static IEnumerable<int> IndicesOfAny<TSource> (this IEnumerable<TSource> self, IEnumerable<TSource> values)
		{
			Check.Self (self);
			Check.Values (values);

			return FindIndices (self, v => values.Contains (v));
		}

		// Haskell: findIndex
		public static int FindIndex<TSource> (this IEnumerable<TSource> self, Func<TSource, bool> predicate)
		{
			Check.Self (self);
			Check.Predicate (predicate);

			int c = -1;
			foreach (var e in self) {
				++c;
				if (predicate (e))
					return c;
			}
			return -1;
		}

		// Haskell: findIndices
		public static IEnumerable<int> FindIndices<TSource> (this IEnumerable<TSource> self, Func<TSource, bool> predicate)
		{
			Check.Self (self);
			Check.Predicate (predicate);

			return CreateFindIndicesIterator (self, predicate);
		}

		private static IEnumerable<int> CreateFindIndicesIterator<TSource> (this IEnumerable<TSource> self, Func<TSource, bool> predicate)
		{
			int c = -1;
			foreach (var e in self) {
				++c;
				if (predicate (e))
					yield return c;
			}
		}

		// Haskell: zip
		public static IEnumerable<Tuple<T1, T2>> Zip<T1, T2> (this IEnumerable<T1> self, IEnumerable<T2> source2)
		{
			return SelectFromEach (self, source2, (a, b) => Tuple.Create (a, b));
		}

		// Haskell: zip3
		public static IEnumerable<Tuple<T1, T2, T3>> Zip<T1, T2, T3> (this IEnumerable<T1> self, IEnumerable<T2> source2, IEnumerable<T3> source3)
		{
			return SelectFromEach (self, source2, source3, (a, b, c) => Tuple.Create (a, b, c));
		}

		// Haskell: zip4
		public static IEnumerable<Tuple<T1, T2, T3, T4>> Zip<T1, T2, T3, T4> (this IEnumerable<T1> self, IEnumerable<T2> source2, IEnumerable<T3> source3, IEnumerable<T4> source4)
		{
			return SelectFromEach (self, source2, source3, source4, (a, b, c, d) => Tuple.Create (a, b, c, d));
		}

		// Haskell: unzip
		public static Tuple<IEnumerable<T1>, IEnumerable<T2>> Unzip<T1, T2> (this IEnumerable<Tuple<T1, T2>> self)
		{
			Check.Self (self);

			return Tuple.Create (
					self.Select (t => t.Item1),
					self.Select (t => t.Item2));
		}

		// Haskell: unzip3
		public static Tuple<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>> Unzip<T1, T2, T3> (this IEnumerable<Tuple<T1, T2, T3>> self)
		{
			Check.Self (self);

			return Tuple.Create (
					self.Select (t => t.Item1),
					self.Select (t => t.Item2),
					self.Select (t => t.Item3));
		}

		// Haskell: unzip4
		public static Tuple<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>> Unzip<T1, T2, T3, T4> (this IEnumerable<Tuple<T1, T2, T3, T4>> self)
		{
			Check.Self (self);

			return Tuple.Create (
					self.Select (t => t.Item1),
					self.Select (t => t.Item2),
					self.Select (t => t.Item3),
					self.Select (t => t.Item4));
		}

		// Haskell: insert
		public static IEnumerable<TSource> Insert<TSource> (this IEnumerable<TSource> self, TSource value)
		{
			return Insert (self, value, Comparer<TSource>.Default.Compare);
		}

		// Haskell: delete
		public static IEnumerable<TSource> RemoveFirstOccurrence<TSource> (this IEnumerable<TSource> self, TSource value)
		{
			return RemoveFirstOccurrences (self, value, 1, null);
		}

		public static IEnumerable<TSource> RemoveFirstOccurrence<TSource> (this IEnumerable<TSource> self, TSource value, IEqualityComparer<TSource> comparer)
		{
			return RemoveFirstOccurrences (self, value, 1, comparer);
		}

		public static IEnumerable<TSource> RemoveFirstOccurrences<TSource> (this IEnumerable<TSource> self, TSource value, int count)
		{
			return RemoveFirstOccurrences (self, value, count, null);
		}

		public static IEnumerable<TSource> RemoveFirstOccurrences<TSource> (this IEnumerable<TSource> self, TSource value, int count, IEqualityComparer<TSource> comparer)
		{
			Check.Self (self);
			if (count < 0)
				throw new ArgumentOutOfRangeException ("count < 0");

			return self.Where (CreateRemoveOccurrencesPredicate (value, count, comparer));
		}

		private static Func<TSource, bool> CreateRemoveOccurrencesPredicate<TSource> (TSource value, int maxCount, IEqualityComparer<TSource> comparer)
		{
			comparer = comparer ?? EqualityComparer<TSource>.Default;
			int count = 0;
			return e => {
				if (count < maxCount && comparer.Equals (e, value)) {
					++count;
					return false;
				}
				return true;
			};
		}

		// Haskell: groupBy
		public static IEnumerable<IEnumerable<TSource>> HaskellGroupBy<TSource> (this IEnumerable<TSource> self, Func<TSource, TSource, bool> func)
		{
			Check.SelfAndFunc (self, func);

			return CreateHaskellGroupByIterator (self, func);
		}

		private static IEnumerable<IEnumerable<TSource>> CreateHaskellGroupByIterator<TSource> (IEnumerable<TSource> self, Func<TSource, TSource, bool> func)
		{
			using (IEnumerator<TSource> s = self.GetEnumerator ()) {
				var e = new List<TSource> ();
				while (s.MoveNext ()) {
					if (e.Count == 0)
						e.Add (s.Current);
					else if (func (e [0], s.Current))
						e.Add (s.Current);
					else {
						yield return e;
						e.Clear ();
						e.Add (s.Current);
					}
				}
				if (e.Count > 0)
					yield return e;
			}
		}

		// Haskell: insertBy
		public static IEnumerable<TSource> Insert<TSource> (this IEnumerable<TSource> self, TSource value, Func<TSource, TSource, int> func)
		{
			Check.Self (self);
			Check.Func (func);

			return CreateInsertIterator (self, value, func);
		}

		private static IEnumerable<TSource> CreateInsertIterator<TSource> (IEnumerable<TSource> self, TSource value, Func<TSource, TSource, int> func)
		{
			bool inserted = false;
			foreach (var e in self) {
				if (!inserted && func (e, value) > 0) {
					inserted = true;
					yield return value;
				}
				yield return e;
			}
			if (!inserted)
				yield return value;
		}


		public static IEnumerable<IEnumerable<TSource>> Subsets<TSource>(this IEnumerable<TSource> self)
		{
			Check.Self(self);

			return CreateSubsetsIterator(self);
		}

		private static IEnumerable<IEnumerable<TSource>> CreateSubsetsIterator<TSource>(IEnumerable<TSource> self)
		{
			var source = self.ToList();
			if (source.Count == 0) yield break;
			if (source.Count > 63) throw new InvalidOperationException(string.Format("Cannot create subsets for more than 63 items, the source contained {0} items", source.Count));

			ulong max = 1UL << source.Count; //2 ** source.Length

			for (ulong row = 1; row < max; row++)
			{
				yield return CreateSubsetsIterator(source, row);
			}
		}

		private static IEnumerable<TSource> CreateSubsetsIterator<TSource>(List<TSource> source, ulong row)
		{
			for (int index = 0; index < source.Count; index++)
			{
				ulong mask = 1UL << index;

				if ((row & mask) != 0)
					yield return source[index];
			}
		}


		public static IEnumerable<IEnumerable<TSource>> Subsets<TSource> (this IEnumerable<TSource> self, Func<IEnumerable<TSource>, bool> predicate)
		{
			Check.Self (self);
			Check.Predicate (predicate);

			return CreateSubsetsIterator (self, predicate);
		}

		private static IEnumerable<IEnumerable<TSource>> CreateSubsetsIterator<TSource> (IEnumerable<TSource> self, Func<IEnumerable<TSource>, bool> predicate)
		{
			CachedSequence<CachedSequence<TSource>> subsets = null;

			foreach (var value in self) {
				var newSubsets = CreateSubsetsIterator (value, subsets, predicate);

				foreach (var s in newSubsets) {
					yield return s;
					subsets = new CachedSequence<CachedSequence<TSource>> (s, subsets);
				}
			}
		}

		private static IEnumerable<CachedSequence<TSource>> CreateSubsetsIterator<TSource> (TSource value, IEnumerable<CachedSequence<TSource>> subsets, Func<IEnumerable<TSource>, bool> predicate)
		{
			var item = new CachedSequence<TSource> (value);

			if (predicate (item)) {
				yield return item;

				if (subsets == null)
					yield break;

				foreach (var p in subsets) {
					var combined = p.Prepend (value);

					if (predicate (combined))
						yield return combined;
				}
			}
		}

		/// <summary>
		/// Gets the min element of <paramref name="source"/>, based on the result of using
		/// the default comparer on the result of <paramref name="valueFunc"/>.
		/// </summary>
		/// <param name="source">The input sequence.</param>
		/// <param name="valueFunc">Used to produce a value for comparison.</param>
		/// <returns>The min element of <paramref name="source"/>.</returns>
		/// <example>
		/// Given an IEnumerable of Pig, where a Pig has attributes Name and Size:
		/// var smallestPig = pigs.MaxBy(pig => pig.Size);
		/// </example>
		/// <exception cref="InvalidOperationException"><paramref name="source"/> contained no elements.</exception>
		public static TSource MinBy<TSource, TValue>(this IEnumerable<TSource > self, Func<TSource, TValue> selector)
		{
			return MinBy (self, selector, null);
		}

		/// <summary>
		/// Gets the min element of <paramref name="source"/>, based on the result of using
		/// <paramref name="comparer"/> on the result of <paramref name="valueFunc"/>.
		/// </summary>
		/// <param name="source">The input sequence.</param>
		/// <param name="valueFunc">Used to produce a value for comparison.</param>
		/// <param name="comparer">Used to compare values produced by valueFunc.</param>
		/// <returns>The min element of <paramref name="source"/>.</returns>
		/// <example>
		/// Given an IEnumerable of Pig, where a Pig has attributes Name and Hue:
		/// var leastPrettyPig = pigs.MaxBy(pig => pig.Hue, (hue1, hue2) => HueComparer.Compare(hue1, hue2));
		/// </example>
		/// <exception cref="InvalidOperationException"><paramref name="source"/> contained no elements.</exception>
		public static TSource MinBy<TSource, TValue>(this IEnumerable<TSource> self, Func<TSource, TValue> selector, IComparer<TValue> comparer)
		{
			comparer = comparer ?? Comparer<TValue>.Default;
			return MaxBy (self, selector, new LambdaComparer<TValue>((x, y) => comparer.Compare (y, x)));
		}

		/// <summary>
		/// Gets the max element of <paramref name="source"/>, based on the result of using
		/// the default comparer on the result of <paramref name="valueFunc"/>.
		/// </summary>
		/// <param name="source">The input sequence.</param>
		/// <param name="valueFunc">Used to produce a value for comparison.</param>
		/// <returns>The max element of <paramref name="source"/>.</returns>
		/// <example>
		/// Given an IEnumerable of Pig, where a Pig has attributes Name and Size:
		/// var largestPig = pigs.MaxBy(pig => pig.Size);
		/// </example>
		/// <exception cref="InvalidOperationException"><paramref name="source"/> contained no elements.</exception>
		public static TSource MaxBy<TSource, TValue>(this IEnumerable<TSource> self, Func<TSource, TValue> selector)
		{
			return MaxBy (self, selector, null);
		}

		/// <summary>
		/// Gets the max element of <paramref name="source"/>, based on the result of using
		/// <paramref name="comparer"/> on the result of <paramref name="valueFunc"/>.
		/// </summary>
		/// <param name="source">The input sequence.</param>
		/// <param name="valueFunc">Used to produce a value for comparison.</param>
		/// <param name="comparer">Used to compare values produced by valueFunc.</param>
		/// <returns>The max element of <paramref name="source"/>.</returns>
		/// <example>
		/// Given an IEnumerable of Pig, where a Pig has attributes Name and Hue:
		/// var prettiestPig = pigs.MaxBy(pig => pig.Hue, (hue1, hue2) => HueComparer.Compare(hue1, hue2));
		/// </example>
		/// <exception cref="InvalidOperationException"><paramref name="source"/> contained no elements.</exception>
		public static TSource MaxBy<TSource, TValue>(this IEnumerable<TSource> self, Func<TSource, TValue> selector, IComparer<TValue> comparer)
		{
			Check.Self (self);
			Check.Selector (selector);

			comparer = comparer ?? Comparer<TValue>.Default;

			TSource max       = default (TSource);
			TValue  maxValue  = default (TValue);
			bool    haveMax   = false;

			foreach (var e in self) {
				var newValue = selector (e);
				if (!haveMax || comparer.Compare (newValue, maxValue) > 0) {
					max = e;
					maxValue = newValue;
					haveMax = true;
				}
			}

			if (!haveMax)
				throw new InvalidOperationException("Sequence contains no elements.");

			return max;
		}
	}
}
