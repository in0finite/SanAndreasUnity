#region License
// IFluentCommandLineParser.cs
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
using Fclp.Internals;

namespace Fclp
{
	/// <summary>
	/// Represents a command line parser which provides methods and properties 
	/// to easily and fluently parse command line arguments. 
	/// </summary>
	public interface IFluentCommandLineParser
	{
		/// <summary>
		/// Setup a new <see cref="ICommandLineOptionFluent{T}"/> using the specified short and long Option name.
		/// </summary>
		/// <param name="shortOption">The short name for the Option. This must not be <c>whitespace</c> or a control character.</param>
		/// <param name="longOption">The long name for the Option. This must not be <c>null</c>, <c>empty</c> or only <c>whitespace</c>.</param>
		/// <returns></returns>
		/// <exception cref="OptionAlreadyExistsException">
		/// A Option with the same <paramref name="shortOption"/> name or <paramref name="longOption"/> name already exists in the <see cref="IFluentCommandLineParser"/>.
		/// </exception>
		/// <exception cref="InvalidOptionNameException">
		/// Either <paramref name="shortOption"/> or <paramref name="longOption"/> are not valid. <paramref name="shortOption"/> must not be <c>whitespace</c>
		/// or a control character. <paramref name="longOption"/> must not be <c>null</c>, <c>empty</c> or only <c>whitespace</c>.
		/// </exception>
		ICommandLineOptionFluent<T> Setup<T>(char shortOption, string longOption);

		/// <summary>
		/// Setup a new <see cref="ICommandLineOptionFluent{T}"/> using the specified short and long Option name.
		/// </summary>
		/// <param name="shortOption">The short name for the Option. This must not be <c>whitespace</c> or a control character.</param>
		/// <param name="longOption">The long name for the Option. This must not be <c>null</c>, <c>empty</c> or only <c>whitespace</c>.</param>
		/// <returns></returns>
		/// <exception cref="OptionAlreadyExistsException">
		/// A Option with the same <paramref name="shortOption"/> name or <paramref name="longOption"/> name already exists in the <see cref="IFluentCommandLineParser"/>.
		/// </exception>
		/// <exception cref="InvalidOptionNameException">
		/// Either <paramref name="shortOption"/> or <paramref name="longOption"/> are not valid. <paramref name="shortOption"/> must not be <c>whitespace</c>
		/// or a control character. <paramref name="longOption"/> must not be <c>null</c>, <c>empty</c> or only <c>whitespace</c>.
		/// </exception>
		[Obsolete("Use new overload Setup<T>(char, string) to specify both a short and long option name instead.")]
		ICommandLineOptionFluent<T> Setup<T>(string shortOption, string longOption);
			
		/// <summary>
		/// Setup a new <see cref="ICommandLineOptionFluent{T}"/> using the specified short Option name.
		/// </summary>
		/// <param name="shortOption">The short name for the Option. This must not be <c>whitespace</c> or a control character.</param>
		/// <returns></returns>
		/// <exception cref="InvalidOptionNameException">if <paramref name="shortOption"/> is invalid for a short option.</exception>
		/// <exception cref="OptionAlreadyExistsException">
		/// A Option with the same <paramref name="shortOption"/> name 
		/// already exists in the <see cref="IFluentCommandLineParser"/>.
		/// </exception>
		ICommandLineOptionFluent<T> Setup<T>(char shortOption);

		/// <summary>
		/// Setup a new <see cref="ICommandLineOptionFluent{T}"/> using the specified long Option name.
		/// </summary>
		/// <param name="longOption">The long name for the Option. This must not be <c>null</c>, <c>empty</c> or only <c>whitespace</c>.</param>
		/// <exception cref="InvalidOptionNameException">if <paramref name="longOption"/> is invalid for a long option.</exception>
		/// <exception cref="OptionAlreadyExistsException">
		/// A Option with the same <paramref name="longOption"/> name already exists in the <see cref="IFluentCommandLineParser"/>.
		/// </exception>
		ICommandLineOptionFluent<T> Setup<T>(string longOption);

		/// <summary>
		/// Setup the help args.
		/// </summary>
		/// <param name="helpArgs">The help arguments to register.</param>
		IHelpCommandLineOptionFluent SetupHelp(params string[] helpArgs);

		/// <summary>
		/// Parses the specified <see><cref>T:System.String[]</cref></see> using the setup Options.
		/// </summary>
		/// <param name="args">The <see><cref>T:System.String[]</cref></see> to parse.</param>
		/// <returns>An <see cref="ICommandLineParserResult"/> representing the results of the parse operation.</returns>
		ICommandLineParserResult Parse(string[] args);

		/// <summary>
		/// Returns the Options that have been setup for this parser.
		/// </summary>
		IEnumerable<ICommandLineOption> Options { get; }

		/// <summary>
		/// Gets or sets whether values that differ by case are considered different. 
		/// </summary>
		bool IsCaseSensitive { get; set; }
	}
}
