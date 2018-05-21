#region License

// FluentCommandLineParser.cs
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

#endregion License

using Fclp.Internals;
using Fclp.Internals.Errors;
using Fclp.Internals.Extensions;
using Fclp.Internals.Validators;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Fclp
{
    /// <summary>
    /// A command line parser which provides methods and properties
    /// to easily and fluently parse command line arguments.
    /// </summary>
    public class FluentCommandLineParser : IFluentCommandLineParser
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="FluentCommandLineParser"/> class.
        /// </summary>
        public FluentCommandLineParser()
        {
            IsCaseSensitive = true;
        }

        /// <summary>
        /// The <see cref="StringComparison"/> type used for case sensitive comparisons.
        /// </summary>
        public const StringComparison CaseSensitiveComparison = StringComparison.CurrentCulture;

        /// <summary>
        /// The <see cref="StringComparison"/> type used for case in-sensitive comparisons.
        /// </summary>
        public const StringComparison IgnoreCaseComparison = StringComparison.CurrentCultureIgnoreCase;

        private List<ICommandLineOption> _options;
        private ICommandLineOptionFactory _optionFactory;
        private ICommandLineParserEngine _parserEngine;
        private ICommandLineOptionFormatter _optionFormatter;
        private IHelpCommandLineOption _helpOption;
        private ICommandLineParserErrorFormatter _errorFormatter;
        private ICommandLineOptionValidator _optionValidator;

        /// <summary>
        /// Gets or sets whether values that differ by case are considered different.
        /// </summary>
        public bool IsCaseSensitive
        {
            get { return StringComparison == CaseSensitiveComparison; }
            set { StringComparison = value ? CaseSensitiveComparison : IgnoreCaseComparison; }
        }

        /// <summary>
        /// Gets the <see cref="StringComparison"/> to use when matching values.
        /// </summary>
        internal StringComparison StringComparison { get; private set; }

        /// <summary>
        /// Gets the list of Options
        /// </summary>
        public List<ICommandLineOption> Options
        {
            get { return _options ?? (_options = new List<ICommandLineOption>()); }
        }

        /// <summary>
        /// Gets or sets the default option formatter.
        /// </summary>
        public ICommandLineOptionFormatter OptionFormatter
        {
            get { return _optionFormatter ?? (_optionFormatter = new CommandLineOptionFormatter()); }
            set { _optionFormatter = value; }
        }

        /// <summary>
        /// Gets or sets the default option formatter.
        /// </summary>
        public ICommandLineParserErrorFormatter ErrorFormatter
        {
            get { return _errorFormatter ?? (_errorFormatter = new CommandLineParserErrorFormatter()); }
            set { _errorFormatter = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="ICommandLineOptionFactory"/> to use for creating <see cref="ICommandLineOptionFluent{T}"/>.
        /// </summary>
        /// <remarks>If this property is set to <c>null</c> then the default <see cref="OptionFactory"/> is returned.</remarks>
        public ICommandLineOptionFactory OptionFactory
        {
            get { return _optionFactory ?? (_optionFactory = new CommandLineOptionFactory()); }
            set { _optionFactory = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="ICommandLineOptionValidator"/> used to validate each setup Option.
        /// </summary>
        public ICommandLineOptionValidator OptionValidator
        {
            get { return _optionValidator ?? (_optionValidator = new CommandLineOptionValidator(this)); }
            set { _optionValidator = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="ICommandLineParserEngine"/> to use for parsing the command line args.
        /// </summary>
        public ICommandLineParserEngine ParserEngine
        {
            get { return _parserEngine ?? (_parserEngine = new CommandLineParserEngineMark2()); }
            set { _parserEngine = value; }
        }

        internal IHelpCommandLineOption HelpOption
        {
            get { return _helpOption ?? (_helpOption = new EmptyHelpCommandLineOption()); }
            set { _helpOption = value; }
        }

        /// <summary>
        /// Setup a new <see cref="ICommandLineOptionFluent{T}"/> using the specified short and long Option name.
        /// </summary>
        /// <param name="shortOption">The short name for the Option. This must not be <c>null</c>, <c>empty</c> or only <c>whitespace</c>.</param>
        /// <param name="longOption">The long name for the Option or <c>null</c> if not required.</param>
        /// <returns></returns>
        /// <exception cref="OptionAlreadyExistsException">
        /// A Option with the same <paramref name="shortOption"/> name or <paramref name="longOption"/> name
        /// already exists in the <see cref="IFluentCommandLineParser"/>.
        /// </exception>
        public ICommandLineOptionFluent<T> Setup<T>(char shortOption, string longOption)
        {
            return SetupInternal<T>(shortOption.ToString(CultureInfo.InvariantCulture), longOption);
        }

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
        public ICommandLineOptionFluent<T> Setup<T>(string shortOption, string longOption)
        {
            return SetupInternal<T>(shortOption, longOption);
        }

        private ICommandLineOptionFluent<T> SetupInternal<T>(string shortOption, string longOption)
        {
            var argOption = this.OptionFactory.CreateOption<T>(shortOption, longOption);

            if (argOption == null)
                throw new InvalidOperationException("OptionFactory is producing unexpected results.");

            OptionValidator.Validate(argOption);

            this.Options.Add(argOption);

            return argOption;
        }

        /// <summary>
        /// Setup a new <see cref="ICommandLineOptionFluent{T}"/> using the specified short Option name.
        /// </summary>
        /// <param name="shortOption">The short name for the Option. This must not be <c>whitespace</c> or a control character.</param>
        /// <returns></returns>
        /// <exception cref="OptionAlreadyExistsException">
        /// A Option with the same <paramref name="shortOption"/> name already exists in the <see cref="IFluentCommandLineParser"/>.
        /// </exception>
        public ICommandLineOptionFluent<T> Setup<T>(char shortOption)
        {
            return SetupInternal<T>(shortOption.ToString(CultureInfo.InvariantCulture), null);
        }

        /// <summary>
        /// Setup a new <see cref="ICommandLineOptionFluent{T}"/> using the specified long Option name.
        /// </summary>
        /// <param name="longOption">The long name for the Option. This must not be <c>null</c>, <c>empty</c> or only <c>whitespace</c>.</param>
        /// <returns></returns>
        /// <exception cref="OptionAlreadyExistsException">
        /// A Option with the same <paramref name="longOption"/> name already exists in the <see cref="IFluentCommandLineParser"/>.
        /// </exception>
        public ICommandLineOptionFluent<T> Setup<T>(string longOption)
        {
            return SetupInternal<T>(null, longOption);
        }

        /// <summary>
        /// Parses the specified <see><cref>T:System.String[]</cref></see> using the setup Options.
        /// </summary>
        /// <param name="args">The <see><cref>T:System.String[]</cref></see> to parse.</param>
        /// <returns>An <see cref="ICommandLineParserResult"/> representing the results of the parse operation.</returns>
        public ICommandLineParserResult Parse(string[] args)
        {
            var parsedOptions = this.ParserEngine.Parse(args).ToList();

            var result = new CommandLineParserResult { EmptyArgs = parsedOptions.IsNullOrEmpty() };

            if (this.HelpOption.ShouldShowHelp(parsedOptions, StringComparison))
            {
                result.HelpCalled = true;
                this.HelpOption.ShowHelp(this.Options);
                return result;
            }

            foreach (var setupOption in this.Options)
            {
                /*
				 * Step 1. match the setup Option to one provided in the args by either long or short names
				 * Step 2. if the key has been matched then bind the value
				 * Step 3. if the key is not matched and it is required, then add a new error
				 * Step 4. the key is not matched and optional, bind the default value if available
				 */

                // Step 1
                ICommandLineOption option = setupOption;
                var match = parsedOptions.FirstOrDefault(pair =>
                    pair.Key.Equals(option.ShortName, this.StringComparison) // tries to match the short name
                    || pair.Key.Equals(option.LongName, this.StringComparison)); // or else the long name

                if (match != null) // Step 2
                {
                    try
                    {
                        option.Bind(match);
                    }
                    catch (OptionSyntaxException)
                    {
                        result.Errors.Add(new OptionSyntaxParseError(option, match));
                        if (option.HasDefault)
                            option.BindDefault();
                    }

                    parsedOptions.Remove(match);
                }
                else
                {
                    if (option.IsRequired) // Step 3
                        result.Errors.Add(new ExpectedOptionNotFoundParseError(option));
                    else if (option.HasDefault)
                        option.BindDefault(); // Step 4

                    result.UnMatchedOptions.Add(option);
                }
            }

            parsedOptions.ForEach(item => result.AdditionalOptionsFound.Add(new KeyValuePair<string, string>(item.Key, item.Value)));

            result.ErrorText = ErrorFormatter.Format(result.Errors);

            return result;
        }

        /// <summary>
        /// Setup the help args.
        /// </summary>
        /// <param name="helpArgs">The help arguments to register.</param>
        public IHelpCommandLineOptionFluent SetupHelp(params string[] helpArgs)
        {
            var helpOption = this.OptionFactory.CreateHelpOption(helpArgs);
            this.HelpOption = helpOption;
            return helpOption;
        }

        /// <summary>
        /// Returns the Options that have been setup for this parser.
        /// </summary>
        IEnumerable<ICommandLineOption> IFluentCommandLineParser.Options
        {
            get { return Options; }
        }
    }
}