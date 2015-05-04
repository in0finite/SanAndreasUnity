using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace ParseSharp
{
    [Flags]
    public enum PatternOptions
    {
        None = 0,
        IgnoreCase = 1,
        Multiline = 2,
        ExplicitCapture = 4,
        Compiled = 8,
        Singleline = 16,
        IgnorePatternWhitespace = 32,
        RightToLeft = 64,
        ECMAScript = 256,
        CultureInvariant = 512
    }

    [Flags]
    public enum WhitespacePolicy
    {
        Match = 0,
        Ignore = 1
    }

    public abstract class Parser
    {
        private static readonly PatternParser _sEndOfInput = Pattern("$", PatternOptions.None, true);

        public static Parser Concat(params Parser[] parsers)
        {
            return parsers.Length == 1 ? parsers[0] : new ConcatParser(parsers);
        }

        public static Parser Alternation(params Parser[] parsers)
        {
            return parsers.Length == 1 ? parsers[0] : new AlternationParser(parsers);
        }

        public static LiteralParser Literal(string literal, bool omitFromResult = true)
        {
            return new LiteralParser(literal, omitFromResult);
        }

        public static PatternParser Pattern(string pattern,
            PatternOptions options = PatternOptions.Compiled,
            bool omitFromResult = false)
        {
            return new PatternParser(new Regex(pattern, (RegexOptions) options), omitFromResult);
        }

        public static PatternParser Pattern(Regex regex, bool omitFromResult = false)
        {
            return new PatternParser(regex, omitFromResult);
        }

        public static LateDefinedParser LateDefined(string identifier, LateDefinedType type = LateDefinedType.Default)
        {
            return new LateDefinedParser(identifier, type);
        }

        public static LateDefinedParser<TResult> LateDefined<TResult>(string identifier = null, LateDefinedType type = LateDefinedType.Default)
            where TResult : ResultSubstitution
        {
            return new LateDefinedParser<TResult>(identifier ?? typeof(TResult).Name, type);
        }

        public static LateDefinedParser<TResult> LateDefined<TResult>(LateDefinedType type)
            where TResult : ResultSubstitution
        {
            return LateDefined<TResult>(null, type);
        }

        public static NullParser Null(string reason)
        {
            return new NullParser(reason);
        }

        public static PatternParser EndOfInput
        {
            get { return _sEndOfInput; }
        }

        protected static IEnumerable<ParseResult> Single(ParseResult result)
        {
            yield return result;
        }

        public static implicit operator Parser(String literal)
        {
            return Literal(literal);
        }

        public static implicit operator Parser(Regex pattern)
        {
            return Pattern(pattern);
        }

        public static Parser operator |(Parser a, Parser b)
        {
            return Alternation(a, b);
        }

        public static Parser operator |(String a, Parser b)
        {
            return Alternation(Literal(a), b);
        }

        public static Parser operator |(Parser a, String b)
        {
            return Alternation(a, Literal(b));
        }

        public static Parser operator +(Parser a, Parser b)
        {
            return Concat(a, b);
        }

        public static Parser operator +(String a, Parser b)
        {
            return Concat(Literal(a), b);
        }

        public static Parser operator +(Parser a, String b)
        {
            return Concat(a, Literal(b));
        }

        public virtual String ExpectingDescription { get { return ToString(); } }

        public bool OmitFromResult { get; private set; }
        
        protected abstract IEnumerable<ParseResult> OnParse(ParseContext ctx);

        public virtual string Name { get { return GetType().Name; } }

        protected Parser(bool omitFromResult = false)
        {
            OmitFromResult = omitFromResult;
        }

        public ParseResult Parse(string input, int offset = 0)
        {
            return Parse(new ParseContext(input, offset));
        }

        internal ParseResult Parse(ParseContext parentContext)
        {
            var ctx = new ParseContext(parentContext, this);
            if (!parentContext.IsUnique(ctx)) {
                return ctx.Error("Infinite parsing loop detected.");
            }

            ParseResult failed = null;
            SymbolExpectedError tailError = null;
            var children = OnParse(ctx)
                .TakeWhile(x => (failed = failed ?? (x.Success ? null : x)) == null &&
                    (tailError = x.TailError ?? tailError) == tailError)
                .SelectMany(x => x.FlattenHierarchy ? x : Single(x))
                .ToArray();

            if (failed != null) {
                tailError = ctx.Expected(tailError, failed.Error as SymbolExpectedError);
            }

            return (failed != null && failed.Fatal)
                ? failed.Error
                : SubstituteResult(new ParseResult(ctx, failed != null
                    ? ctx.Expected(failed.Error as SymbolExpectedError, tailError)
                    : tailError, children));
        }

        protected virtual ParseResult SubstituteResult(ParseResult result)
        {
            return result;
        }

        public RepeatedParser Repeated
        {
            get { return new RepeatedParser(this); }
        }

        public OptionalParser Optional
        {
            get { return new OptionalParser(this); }
        }

        public WhitespacePolicyParser MatchWhitespace
        {
            get { return new WhitespacePolicyParser(this, WhitespacePolicy.Match); }
        }

        public WhitespacePolicyParser IgnoreWhitespace
        {
            get { return new WhitespacePolicyParser(this, WhitespacePolicy.Ignore); }
        }
    }

    public abstract class AgregateParser : Parser
    {
        private static IEnumerable<Parser> Single(Parser parser)
        {
            yield return parser;
        }

        protected static Parser[] Collapse<TParser>(Parser[] parsers)
            where TParser : AgregateParser
        {
            if (parsers.OfType<TParser>().Any()) {
                return parsers.SelectMany(x => {
                    var agregate = x as TParser;
                    return agregate != null ? agregate.Parsers : Single(x);
                }).ToArray();
            }

            return parsers;
        }

        private readonly Parser[] _parsers;

        protected Parser[] Parsers { get { return _parsers; } }

        protected AgregateParser(bool omitFromHierarchy, Parser[] parsers)
            : base(omitFromHierarchy)
        {
            _parsers = parsers;

            if (_parsers.Length == 0) {
                throw new ArgumentException("At least one parser is required.", "parsers");
            }
        }
    }

    public sealed class ConcatParser : AgregateParser
    {
        internal ConcatParser(params Parser[] parsers)
            : base(true, Collapse<ConcatParser>(parsers)) { }

        public override string ExpectingDescription
        {
            get { return Parsers.First().ExpectingDescription; }
        }

        protected override IEnumerable<ParseResult> OnParse(ParseContext ctx)
        {
            SymbolExpectedError tailError = null;
// ReSharper disable once ConvertClosureToMethodGroup
            foreach (var result in Parsers.Select(x => ctx.Parse(x))) {
                if (result.Success || tailError == null) {
                    if (tailError != null) {
                        result.TailError = ctx.Expected(result.TailError, tailError);
                    }

                    yield return result;
                    tailError = result.TailError;
                } else {
                    var error = result.Error as SymbolExpectedError;
                    if (error != null) {
                        yield return ctx.Expected(tailError, error);
                    } else {
                        yield return result;
                    }
                }

                if (!result.Success) yield break;
            }
        }

        public override string ToString()
        {
            return String.Format("({0})", String.Join(", ", Parsers.Select(x => x.ToString()).ToArray()));
        }
    }

    public sealed class AlternationParser : AgregateParser
    {
        internal AlternationParser(params Parser[] parsers)
            : base(true, Collapse<AlternationParser>(parsers)) { }

        public override string ExpectingDescription
        {
            get { return Parsers.Length == 1 ? Parsers[0].ExpectingDescription :
                String.Format("{0}, or {1}", String.Join(", ", Parsers.Take(Parsers.Length - 1)
                    .Select(x => x.ExpectingDescription).ToArray()), Parsers.Last().ExpectingDescription); }
        }

        protected override IEnumerable<ParseResult> OnParse(ParseContext ctx)
        {
// ReSharper disable once ConvertClosureToMethodGroup
            var matches = Parsers.Select(x => ctx.Peek(x))
                .OrderByDescending(x => x.Success ? x.Length : x.Error.Context.Offset)
                .ToArray();

            var success = matches.FirstOrDefault(x => x.Success);

            if (success == null && matches.All(x => x.Error.Context.Offset == matches[0].Error.Context.Offset)) {
                yield return matches[0].Error.Context.Expected(Parsers);
                yield break;
            }

            yield return ctx.Apply(success) ?? matches.FirstOrDefault();
        }

        public override string ToString()
        {
            return String.Format("({0})", String.Join(" | ", Parsers.Select(x => x.ToString()).ToArray()));
        }
    }

    public sealed class RepeatedParser : Parser
    {
        private readonly Parser _parser;

        public override string ExpectingDescription
        {
            get { return String.Format("{0} (repeated)", _parser.ExpectingDescription); }
        }

        internal RepeatedParser(Parser parser)
            : base(true)
        {
            _parser = parser;
        }

        protected override IEnumerable<ParseResult> OnParse(ParseContext ctx)
        {
            for (;;) {
                var result = ctx.Parse(_parser);

                if (result.Success) {
                    if (result.Length == 0) break;
                    yield return result;
                } else {
                    yield return new ParseResult(result.Error, false);
                    break;
                }
            }
        }

        public override string ToString()
        {
            return String.Format("{{{0}}}", _parser);
        }
    }

    public sealed class OptionalParser : Parser
    {
        private readonly Parser _parser;

        public override string ExpectingDescription
        {
            get { return String.Format("{0} (optional)", _parser.ExpectingDescription); }
        }

        internal OptionalParser(Parser parser)
            : base(true)
        {
            _parser = parser;
        }

        protected override IEnumerable<ParseResult> OnParse(ParseContext ctx)
        {
            var result = ctx.Parse(_parser);

            if (result.Success) {
                if (result.Length == 0) yield break;
                yield return result;
            } else {
                yield return new ParseResult(result.Error, false);
            }
        }

        public override string ToString()
        {
            return String.Format("[{0}]", _parser);
        }
    }

    public sealed class LiteralParser : Parser
    {
        private readonly String _literal;

        internal LiteralParser(String literal, bool omitFromResult)
            : base(omitFromResult)
        {
            _literal = literal;
        }

        protected override IEnumerable<ParseResult> OnParse(ParseContext ctx)
        {
            if (!ctx.Match(_literal)) {
                yield return ctx.Expected(ExpectingDescription);
            }
        }

        public override string ToString()
        {
            return String.Format("\"{0}\"", _literal);
        }
    }

    public sealed class PatternParser : Parser
    {
        private readonly Regex _regex;

        internal PatternParser(Regex regex, bool omitFromResult)
            : base(omitFromResult)
        {
            _regex = regex;
        }

        protected override IEnumerable<ParseResult> OnParse(ParseContext ctx)
        {
            var match = _regex.Match(ctx.Input, ctx.Offset);
            if (!match.Success || match.Index != ctx.Offset) {
                yield return ctx.Expected(_regex);
                yield break;
            }

            ctx.Advance(match.Length);
        }

        public override string ToString()
        {
            return _regex.ToString();
        }
    }

    public enum LateDefinedType
    {
        Default = 0,
        OmitFromHierarchy = 1,
        Collapse = 2
    }

    public class LateDefinedParser : Parser
    {
        private readonly String _name;
        private readonly bool _collapse;

        public override string Name
        {
            get { return _name; }
        }

        public override string ExpectingDescription
        {
            get { return Name; }
        }

        public Parser Definition { get; set; }

        internal LateDefinedParser(String name, LateDefinedType type)
            : base(type == LateDefinedType.OmitFromHierarchy)
        {
            _name = name;
            _collapse = type == LateDefinedType.Collapse;
        }

        protected override IEnumerable<ParseResult> OnParse(ParseContext ctx)
        {
            if (Definition == null) {
                throw new Exception(String.Format("LateDefined \"{0}\" Not yet defined.", Name));
            }

            var match = ctx.Parse(Definition);

            if (!match.Success) {
                return Single(match.Error.Priority < 0 ? ctx.Expected(this) : match);
            }

            return !_collapse ? Single(match) : Enumerable.Empty<ParseResult>();
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public sealed class LateDefinedParser<TResult> : LateDefinedParser
        where TResult : ResultSubstitution
    {
        private static readonly Func<ParseResult, TResult> _sCtor;

        static LateDefinedParser()
        {
            var ctor = typeof(TResult).GetConstructor(new [] {typeof(ParseResult)});

            if (ctor == null) { 
                throw new Exception(String.Format(
                    "Type \"{0}\" must have a public constructor " +
                    "accepting a ParseResult if it can be a valid substitution.",
                    typeof(TResult).FullName));
            }

            var param = Expression.Parameter(typeof(ParseResult), "result");
            var call = Expression.New(ctor, param);

            _sCtor = Expression.Lambda<Func<ParseResult, TResult>>(call, param).Compile();
        }

        internal LateDefinedParser(String name, LateDefinedType type)
            : base(name, type) { }

        protected override ParseResult SubstituteResult(ParseResult result)
        {
            return _sCtor(result);
        }
    }

    public abstract class WrappedParser : Parser
    {
        private readonly Parser _inner;

        public Parser Inner { get { return _inner; } }

        protected WrappedParser(Parser inner, bool omitFromHierarchy = false)
            : base(omitFromHierarchy)
        {
            _inner = inner;
        }

        protected override IEnumerable<ParseResult> OnParse(ParseContext ctx)
        {
            yield return ctx.Parse(_inner);
        }
    }

    public sealed class WhitespacePolicyParser : WrappedParser
    {
        public readonly WhitespacePolicy Policy;

        internal WhitespacePolicyParser(Parser inner, WhitespacePolicy policy)
            : base(inner, true)
        {
            Policy = policy;
        }

        public override string ToString()
        {
            return Inner.ToString();
        }
    }

    public sealed class NullParser : Parser
    {
        public readonly String Reason;

        internal NullParser(String reason)
        {
            Reason = reason;
        }

        protected override IEnumerable<ParseResult> OnParse(ParseContext ctx)
        {
            yield return ctx.Error(Reason);
        }
    }
}
