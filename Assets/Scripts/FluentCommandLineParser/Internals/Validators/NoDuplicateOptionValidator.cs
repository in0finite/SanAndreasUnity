#region License
// NoDuplicateOptionValidator.cs
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

namespace Fclp.Internals.Validators
{
	/// <summary>
	/// Validator used to ensure no there are duplicate Options setup.
	/// </summary>
	public class NoDuplicateOptionValidator : ICommandLineOptionValidator
	{
		private readonly IFluentCommandLineParser _parser;

		

		/// <summary>
		/// Initialises a new instance of the <see cref="NoDuplicateOptionValidator"/> class.
		/// </summary>
		/// <param name="parser">The <see cref="IFluentCommandLineParser"/> containing the setup options. This must not be null.</param>
		public NoDuplicateOptionValidator(IFluentCommandLineParser parser)
		{
			if (parser == null) throw new ArgumentNullException("parser");
			_parser = parser;
		}

		/// <summary>
		/// Gets the <see cref="StringComparison"/> type used for duplicates.
		/// </summary>
		private StringComparison ComparisonType
		{
			get { return _parser.IsCaseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase; }
		}
		
		/// <summary>
		/// Verifies that the specified <see cref="ICommandLineOption"/> will not cause any duplication.
		/// </summary>
		/// <param name="commandLineOption">The <see cref="ICommandLineOption"/> to validate.</param>
		public void Validate(ICommandLineOption commandLineOption)
		{
			foreach (var option in _parser.Options)
			{
				if (string.IsNullOrEmpty(commandLineOption.ShortName) == false)
				{
					ValuesAreEqual(commandLineOption.ShortName, option.ShortName);
				}

				if (string.IsNullOrEmpty(commandLineOption.LongName) == false)
				{
					ValuesAreEqual(commandLineOption.LongName, option.LongName);
				}				
			}
		}

		private void ValuesAreEqual(string value, string otherValue)
		{
			if (string.Equals(value, otherValue, ComparisonType))
			{
				throw new OptionAlreadyExistsException(value);
			}
		}
	}
}