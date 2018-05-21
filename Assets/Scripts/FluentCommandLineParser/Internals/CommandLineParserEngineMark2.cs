#region License
// CommandLineParserEngineMark2.cs
// Copyright (c) 2013, Simon Williams
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, are permitted provide
// d that the following conditions are met:
// 
// Redistributions of source code must retain the above copyright notice, this list of conditions and the
// following disclaimer.
// 
// Redistributions in binary form must reproduce the above copyright notice, this list of conditions and
// the following disclaimer in the documentation and/or other materials provided with the distribution.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED 
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A 
// PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR
// ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED
// TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
// POSSIBILITY OF SUCH DAMAGE.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Fclp.Internals.Extensions;

namespace Fclp.Internals
{
	/// <summary>
	/// More advanced parser for transforming command line arguments into appropriate <see cref="ParsedOption"/>.
	/// </summary>
	public class CommandLineParserEngineMark2 : ICommandLineParserEngine
	{
		/// <summary>
		/// Parses the specified <see><cref>T:System.String[]</cref></see> into appropriate <see cref="ParsedOption"/> objects..
		/// </summary>
		/// <param name="args">The <see><cref>T:System.String[]</cref></see> to parse.</param>
		/// <returns>An <see cref="ParserEngineResult"/> representing the results of the parse operation.</returns>
		public ParserEngineResult Parse(string[] args)
		{
			args = args ?? new string[0];
			var list = new List<ParsedOption>();

			for (int index = 0; index < args.Length; index++)
			{
				string currentArg = args[index];

				// we only want to find keys at this point
				string prefix = ExtractPrefix(currentArg);

				if (prefix == null) continue;

				var parsedOption = new ParsedOption
				{
					RawKey = currentArg,
					Prefix = prefix,
					Key = currentArg.Remove(0, prefix.Length),
					Suffix = ExtractSuffix(currentArg)
				};

				TrimSuffix(parsedOption);

				DetermineOptionValue(args, index, parsedOption);

				var needToSplitKey = PrefixIsShortOption(prefix) && parsedOption.Key.Length > 1;

				if (needToSplitKey)
					list.AddRange(CloneAndSplit(parsedOption));
				else
					list.Add(parsedOption);
			}

			return new ParserEngineResult(list, null);
		}

		private static IEnumerable<ParsedOption> CloneAndSplit(ParsedOption parsedOption)
		{
			return parsedOption.Key.Select(c =>
			{
				var clone = parsedOption.Clone();
				clone.Key = new string(new[] { c });
				return clone;
			}).ToList();
		}

		private static bool PrefixIsShortOption(string key)
		{
			return SpecialCharacters.ShortOptionPrefix.Contains(key);
		}

		private static void TrimSuffix(ParsedOption parsedOption)
		{
			if (parsedOption.HasSuffix)
			{
				parsedOption.Key = parsedOption.Key.TrimEnd(parsedOption.Suffix.ToCharArray());
			}
		}

		static void DetermineOptionValue(string[] args, int currentIndex, ParsedOption option)
		{
			if (SpecialCharacters.ValueAssignments.Any(option.Key.Contains))
			{
				TryGetValueFromKey(option);
			}

			var allValues = new List<string>();
			var additionalValues = new List<string>();

			var otherValues = CombineValuesUntilNextKey(args, currentIndex + 1);

			if (option.HasValue) allValues.Add(option.Value);

			if (otherValues.IsNullOrEmpty() == false)
			{
				allValues.AddRange(otherValues);
				
				if (otherValues.Count() > 1)
				{
					additionalValues.AddRange(otherValues);
					additionalValues.RemoveAt(0);
				}
			}

			option.Value = allValues.FirstOrDefault();
			option.Values = allValues.ToArray();
			option.AddtionalValues = additionalValues.ToArray();
		}

		private static void TryGetValueFromKey(ParsedOption option)
		{
			var splitted = option.Key.Split(SpecialCharacters.ValueAssignments, 2, StringSplitOptions.RemoveEmptyEntries);

			option.Key = splitted[0];

			if (splitted.Length > 1)
				option.Value = splitted[1].WrapInDoubleQuotesIfContainsWhitespace();
		}

		static IEnumerable<string> CombineValuesUntilNextKey(string[] args, int currentIndex)
		{
			var values = new List<string>();

			for (int index = currentIndex; index < args.Length; index++)
			{
				string currentArg = args[index];

				// we only want to find values at this point
				if (IsAKey(currentArg)) break;

				currentArg = currentArg.WrapInDoubleQuotesIfContainsWhitespace();

				values.Add(currentArg);
			}

			return values;
		}

		/// <summary>
		/// Gets whether the specified <see cref="System.String"/> is a Option key.
		/// </summary>
		/// <param name="arg">The <see cref="System.String"/> to examine.</param>
		/// <returns><c>true</c> if <paramref name="arg"/> is a Option key; otherwise <c>false</c>.</returns>
		static bool IsAKey(string arg)
		{
			return arg != null && SpecialCharacters.OptionPrefix.Any(arg.StartsWith);
		}

		/// <summary>
		/// Extracts the key identifier from the specified <see cref="System.String"/>.
		/// </summary>
		/// <param name="arg">The <see cref="System.String"/> to extract the key identifier from.</param>
		/// <returns>A <see cref="System.String"/> representing the key identifier if found; otherwise <c>null</c>.</returns>
		static string ExtractPrefix(string arg)
		{
			return arg != null ? SpecialCharacters.OptionPrefix.FirstOrDefault(arg.StartsWith) : null;
		}

		/// <summary>
		/// Parses the specified <see><cref>T:System.String[]</cref></see> into key value pairs.
		/// </summary>
		/// <param name="args">The <see><cref>T:System.String[]</cref></see> to parse.</param>
		/// <returns>An <see cref="IEnumerable{T}"/> containing the results of the parse operation.</returns>
		IEnumerable<ParsedOption> ICommandLineParserEngine.Parse(string[] args)
		{
			return Parse(args).ParsedOptions;
		}

		/// <summary>
		/// Gets whether the specified <see cref="System.String"/> has a special suffix;
		/// </summary>
		/// <param name="arg">The <see cref="System.String"/> to examine.</param>
		/// <returns><c>true</c> if the <paramref name="arg"/> ends with a special suffix; otherwise <c>false</c>.</returns>
		static bool HasSpecialSuffix(string arg)
		{
			return arg != null && SpecialCharacters.OptionSuffix.Any(arg.EndsWith);
		}

		/// <summary>
		/// Extracts the key identifier from the specified <see cref="System.String"/>.
		/// </summary>
		/// <param name="arg">The <see cref="System.String"/> to extract the key identifier from.</param>
		/// <returns>A <see cref="System.String"/> representing the key identifier if found; otherwise <c>null</c>.</returns>
		static string ExtractSuffix(string arg)
		{
			return arg != null ? SpecialCharacters.OptionSuffix.FirstOrDefault(arg.EndsWith) : null;
		}
	}
}