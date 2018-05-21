//
// String.cs
//
// Author:
//   Jb Evain (jbevain@novell.com)
//   Jonathan Pryor  <jpryor@novell.com>
//   Bojan Rajkovic <bojanr@brandeis.edu>
//
// Copyright (c) 2007, 2008 Novell, Inc. (http://www.novell.com)
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
using System.Text.RegularExpressions;

using Cadenza.IO;

namespace Cadenza {

	public static class StringCoda {

		public static TEnum ToEnum<TEnum> (this string self)
		{
			Check.Self (self);

			return (TEnum) Enum.Parse (typeof (TEnum), self);
		}

		public static IEnumerable<string> Lines (this string self)
		{
			Check.Self (self);

			return new StringReader (self).Lines ();
		}

		public static IEnumerable<string> WrappedLines (this string self, params int[] widths)
		{
			IEnumerable<int> w = widths;
			return WrappedLines (self, w);
		}

		public static IEnumerable<string> WrappedLines (this string self, IEnumerable<int> widths)
		{
			if (widths == null)
				throw new ArgumentNullException ("widths");
			return CreateWrappedLinesIterator (self, widths);
		}

		private static IEnumerable<string> CreateWrappedLinesIterator (string self, IEnumerable<int> widths)
		{
			if (string.IsNullOrEmpty (self)) {
				yield return string.Empty;
				yield break;
			}
			using (var ewidths = widths.GetEnumerator ()) {
				bool? hw = null;
				int width = GetNextWidth (ewidths, int.MaxValue, ref hw);
				int start = 0, end;
				do {
					end = GetLineEnd (start, width, self);
					char c = self [end-1];
					if (char.IsWhiteSpace (c))
						--end;
					bool needContinuation = end != self.Length && !IsEolChar (c);
					string continuation = "";
					if (needContinuation) {
						--end;
						continuation = "-";
					}
					string line = self.Substring (start, end - start) + continuation;
					yield return line;
					start = end;
					if (char.IsWhiteSpace (c))
						++start;
					width = GetNextWidth (ewidths, width, ref hw);
				} while (start < self.Length);
			}
		}

		private static int GetNextWidth (IEnumerator<int> ewidths, int curWidth, ref bool? eValid)
		{
			if (!eValid.HasValue || (eValid.HasValue && eValid.Value)) {
				curWidth = (eValid = ewidths.MoveNext ()).Value ? ewidths.Current : curWidth;
				// '.' is any character, - is for a continuation
				const string minWidth = ".-";
				if (curWidth < minWidth.Length)
					throw new ArgumentOutOfRangeException ("widths",
							string.Format ("Element must be >= {0}, was {1}.", minWidth.Length, curWidth));
				return curWidth;
			}
			// no more elements, use the last element.
			return curWidth;
		}

		private static bool IsEolChar (char c)
		{
			return !char.IsLetterOrDigit (c);
		}

		private static int GetLineEnd (int start, int length, string description)
		{
			int end = ((start + length) < start)	// overflow
				? description.Length
				: Math.Min (start + length, description.Length);
			int sep = -1;
			for (int i = start; i < end; ++i) {
				if (description [i] == '\n')
					return i+1;
				if (IsEolChar (description [i]))
					sep = i+1;
			}
			if (sep == -1 || end == description.Length)
				return end;
			return sep;
		}

		public static IEnumerable<string> Tokens (this string self, params Func<char?, char, bool>[] categories)
		{
			Check.Self (self);

			return new StringReader (self).Tokens (categories);
		}

		public static IEnumerable<string> Words (this string self)
		{
			Check.Self (self);

			return new StringReader (self).Words ();
		}

		public static string Remove (this string self, params string[] targets)
		{
			Check.Self (self);
			if (targets == null)
				throw new ArgumentNullException ("targets");

			StringBuilder builder = new StringBuilder (self);

			for (int i = 0; i < targets.Length; ++i)
				builder.Replace (targets[i], String.Empty);

			return builder.ToString();
		}

		public static IEnumerable<Match> Matches (this string self, string regex)
		{
			return Matches (self, regex, RegexOptions.None);
		}

		public static IEnumerable<Match> Matches (this string self, string regex, RegexOptions options)
		{
			Check.Self (self);

			return new Regex (regex, options).Matches (self).Cast<Match> ();
		}

		public static IEnumerable<string> MatchValues (this string self, string regex, RegexOptions options)
		{
			Check.Self (self);

			return Matches (self, regex, options)
				.Select (m => m.Value);
		}

		public static IEnumerable<string> MatchValues (this string self, string regex)
		{
			Check.Self (self);

			return MatchValues (self, regex, RegexOptions.None);
		}

		public static IEnumerable<string> Captures (this string self, string regex, RegexOptions options)
		{
			Check.Self (self);

			return Matches (self, regex, options)
				.SelectMany (m => m.Groups.Cast<Group> ().Skip (1))
				.Select (g => g.Value);
		}

		public static IEnumerable<string> Captures (this string self, string regex)
		{
			Check.Self (self);

			return Captures (self, regex, RegexOptions.None);
		}

		private static IEnumerable<KeyValuePair<string, string>> CreateCaptureNamedGroupsIterator (this string self, string regex, RegexOptions options)
		{
			Regex r = new Regex (regex, options);
			foreach (Match match in r.Matches (self)) {
				for (int i = 1; i < match.Groups.Count; i++) {
					Group group = match.Groups[i];
					if (r.GroupNameFromNumber(i) != "0" && group.Value != string.Empty)
						yield return new KeyValuePair<string, string> (r.GroupNameFromNumber(i), group.Value);
				}
			}
		}
		
		public static ILookup<string, string> CaptureNamedGroups (this string self, string regex, RegexOptions options)
		{
			Check.Self (self);

			return CreateCaptureNamedGroupsIterator (self, regex, options).ToLookup (s => s.Key, s => s.Value);
		}

		public static ILookup<string, string> CaptureNamedGroups (this string self, string regex)
		{
			Check.Self (self);

			return CaptureNamedGroups (self, regex, RegexOptions.None);
		}

		public static string Slice (this string self, int start, int end)
		{
			Check.Self (self);

			if (start < 0 || start > self.Length)
				throw new ArgumentOutOfRangeException ("start");

			if (end < 0)
				end += self.Length + 1;

			if (end < start || end > self.Length)
				throw new ArgumentOutOfRangeException ("end");

			return self.Substring (start, end - start);
		}

		public static bool IsNullOrWhitespace (this string self)
		{
			return (self == null || self.Trim() == String.Empty);
		}
	}
}
