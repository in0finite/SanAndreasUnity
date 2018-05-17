using System;
using System.Linq;

namespace ParseSharp
{
    public class ParseError : IEquatable<ParseError>
    {
        public static implicit operator ParseResult(ParseError error)
        {
            return new ParseResult(error, true);
        }

        private int _lineNumber;
        private int _columnNumber;

        internal readonly int Priority;
        internal readonly ParseContext Context;

        public readonly String Message;

        public int LineNumber
        {
            get
            {
                if (_lineNumber == 0) FindLocation();
                return _lineNumber;
            }
        }

        public int ColumnNumber
        {
            get
            {
                if (_columnNumber == 0) FindLocation();
                return _columnNumber;
            }
        }

        protected internal ParseError(ParseContext ctx, string message, int priority = 0)
        {
            Priority = priority;
            Context = ctx;

            Message = message;
        }

        private void FindLocation()
        {
            _lineNumber = 1;
            _columnNumber = 1;

            var input = Context.Input;

            for (var i = 0; i < input.Length && i < Context.Offset; ++i)
            {
                switch (input[i])
                {
                    case '\n':
                        ++_lineNumber;
                        _columnNumber = 1;
                        break;

                    default:
                        ++_columnNumber;
                        break;
                }
            }
        }

        public override string ToString()
        {
            return String.Format("{0} at line {1}, column {2}", Message, LineNumber, ColumnNumber);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ParseError);
        }

        public bool Equals(ParseError other)
        {
            return other != null
                && other.Context.Offset == Context.Offset
                && other.Context.Parser == Context.Parser;
        }

        public override int GetHashCode()
        {
            return Context.Offset.GetHashCode() ^ Context.Parser.GetHashCode();
        }
    }

    public class SymbolExpectedError : ParseError
    {
        private static String FormatOptionList(String[] options)
        {
            if (options.Length == 0)
            {
                throw new ArgumentException("Expected one or more option.", "options");
            }

            if (options.Length == 1)
            {
                return String.Format("Expected {0}", options[0]);
            }

            return String.Format("Expected {0} or {1}",
                String.Join(", ", options.Take(options.Length - 1).ToArray()),
                options.Last());
        }

        public readonly String[] Options;

        internal SymbolExpectedError(ParseContext ctx, String[] options, int priority)
            : base(ctx, FormatOptionList(options), priority)
        {
            Options = options;
        }
    }
}