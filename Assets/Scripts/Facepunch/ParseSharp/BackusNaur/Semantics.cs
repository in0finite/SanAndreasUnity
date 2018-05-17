using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace ParseSharp.BackusNaur
{
    public sealed class SemanticError
    {
        private int _lineNumber;
        private int _columnNumber;

        private readonly ParseResult _location;

        public readonly String Message;
        public readonly bool Fatal;

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

        public SemanticError(ParseResult location, string message, bool fatal = true)
        {
            _location = location;

            Message = message;
            Fatal = fatal;
        }

        private void FindLocation()
        {
            _lineNumber = 1;
            _columnNumber = 1;

            var input = _location.Input;

            for (var i = 0; i < input.Length && i < _location.Index; ++i)
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
    }

    public sealed class ParserGenerationException : Exception
    {
        public readonly SemanticError[] Errors;

        internal ParserGenerationException(SemanticError[] errors)
            : base(String.Format("Encountered {0} error{1} during parser generation:{2}",
                errors.Length, errors.Length == 1 ? "" : "s",
                String.Join("", errors.Select(x => String.Format("\n  {0}", x)).ToArray())))
        {
            Errors = errors;
        }
    }

    internal sealed class ParserGenerationContext
    {
        public static Parser Generate(params Rule[] rules)
        {
            if (rules.Length == 0)
            {
                return Parser.Null("No rules specified");
            }

            var ctx = new ParserGenerationContext();
            ctx.AddRules(rules);
            var result = rules.First().Generate(ctx);

            if (ctx._errors.Any())
            {
                throw new ParserGenerationException(ctx._errors.ToArray());
            }

            return result;
        }

        private readonly Dictionary<String, Rule> _rules;
        private readonly List<SemanticError> _errors;

        private ParserGenerationContext()
        {
            _rules = new Dictionary<string, Rule>();
            _errors = new List<SemanticError>();
        }

        private void AddRules(params Rule[] rules)
        {
            foreach (var rule in rules)
            {
                _rules.Add(rule.Identifier, rule);
            }
        }

        public Parser GetRule(ParseResult result, String name)
        {
            if (_rules.ContainsKey(name))
            {
                return _rules[name].Generate(this);
            }

            var reason = String.Format("Rule \"{0}\" is undefined", name);
            _errors.Add(new SemanticError(result, reason));
            return Parser.Null(reason);
        }
    }

    public sealed class ParserGenerator : ResultSubstitution
    {
        public static Parser FromEBnf(string ebnf, string rootParserName = null)
        {
            var parser = new ExtendedBackusNaurFormParser();
            return parser.Parse(ebnf).Generate(rootParserName);
        }

        internal readonly Rule[] Rules;

        public ParserGenerator(ParseResult original) : base(original)
        {
            Rules = GetChildren<Rule>(0, ChildCount);
        }

        public Parser Generate(string rootParserName = null)
        {
            if (rootParserName == null)
            {
                return ParserGenerationContext.Generate(Rules);
            }

            var root = Rules.FirstOrDefault(x => x.Identifier == rootParserName);
            if (root == null) throw new ArgumentException("No rule found with the given name.", "rootParserName");

            return ParserGenerationContext.Generate(new[] { root }.Concat(Rules.Where(x => x != root)).ToArray());
        }
    }

    internal interface IParserGenerator
    {
        Parser Generate(ParserGenerationContext ctx);
    }

    internal sealed class Rule : ResultSubstitution, IParserGenerator
    {
        internal bool Defined { get; private set; }

        public readonly String Identifier;
        public readonly String[] Options;
        public readonly Expression Expression;
        public readonly LateDefinedParser GeneratedParser;

        public Rule(ParseResult original)
            : base(original)
        {
            Options = GetChildren<RuleOption>(0, ChildCount - 2)
                .Select(x => x.Option)
                .ToArray();

            Identifier = GetChild<Identifier>(ChildCount - 2).Value;
            Expression = GetChild<Expression>(ChildCount - 1);

            GeneratedParser = Parser.LateDefined(Identifier,
                Options.Contains("collapse") ? LateDefinedType.Collapse :
                Options.Contains("omit-from-hierarchy") ? LateDefinedType.OmitFromHierarchy :
                LateDefinedType.Default);
        }

        public Parser Generate(ParserGenerationContext ctx)
        {
            if (Defined) return GeneratedParser;
            Defined = true;

            GeneratedParser.Definition = Expression.Generate(ctx);

            var matchWhitespace = Options.Contains("match-whitespace");
            var ignoreWhitespace = Options.Contains("skip-whitespace");

            if (matchWhitespace && !ignoreWhitespace)
            {
                GeneratedParser.Definition = GeneratedParser.Definition.MatchWhitespace;
            }
            else if (ignoreWhitespace && !matchWhitespace)
            {
                GeneratedParser.Definition = GeneratedParser.Definition.IgnoreWhitespace;
            }

            return GeneratedParser;
        }
    }

    internal sealed class RuleOption : ResultSubstitution
    {
        public readonly String Option;

        public RuleOption(ParseResult original) : base(original)
        {
            Option = Value.Trim();
        }
    }

    internal sealed class Expression : ResultSubstitution, IParserGenerator
    {
        public readonly Alternation Alternation;

        public Expression(ParseResult original)
            : base(original)
        {
            Alternation = GetChild<Alternation>(0);
        }

        public Parser Generate(ParserGenerationContext ctx)
        {
            return Alternation.Generate(ctx);
        }
    }

    internal sealed class Alternation : ResultSubstitution, IParserGenerator
    {
        public readonly Concatenation[] Concatenations;

        public Alternation(ParseResult original)
            : base(original)
        {
            Concatenations = GetChildren<Concatenation>(0, ChildCount);
        }

        public Parser Generate(ParserGenerationContext ctx)
        {
            return Parser.Alternation(Concatenations.Select(x => x.Generate(ctx)).ToArray());
        }
    }

    internal sealed class Concatenation : ResultSubstitution, IParserGenerator
    {
        public readonly Factor[] Factors;

        public Concatenation(ParseResult original)
            : base(original)
        {
            Factors = GetChildren<Factor>(0, ChildCount);
        }

        public Parser Generate(ParserGenerationContext ctx)
        {
            return Parser.Concat(Factors.Select(x => x.Generate(ctx)).ToArray());
        }
    }

    internal abstract class Factor : ResultSubstitution, IParserGenerator
    {
        protected Factor(ParseResult original) : base(original)
        {
        }

        public abstract Parser Generate(ParserGenerationContext ctx);
    }

    internal sealed class Identifier : Factor
    {
        public new readonly String Value;

        public Identifier(ParseResult original)
            : base(original)
        {
            Value = String.Join(" ", GetChildren<IdentifierWord>(0, ChildCount)
                .Select(x => x.Value)
                .ToArray());
        }

        public override Parser Generate(ParserGenerationContext ctx)
        {
            return ctx.GetRule(this, Value);
        }
    }

    internal sealed class IdentifierWord : ResultSubstitution
    {
        public new readonly String Value;

        public IdentifierWord(ParseResult original)
            : base(original)
        {
            Value = base.Value.Trim();
        }
    }

    internal sealed class Literal : Factor
    {
        public new readonly String Value;

        public Literal(ParseResult original) : base(original)
        {
            Value = new String(GetChildren<Character>(0, ChildCount).Select(x => x.Char).ToArray());
        }

        public override Parser Generate(ParserGenerationContext ctx)
        {
            return Value;
        }
    }

    internal abstract class Character : ResultSubstitution
    {
        public char Char { get; protected set; }

        protected Character(ParseResult original) : base(original)
        {
        }
    }

    internal sealed class SingleCharacter : Character
    {
        public SingleCharacter(ParseResult original)
            : base(original)
        {
            Char = Value[0];
        }
    }

    internal sealed class EscapedCharacter : Character
    {
        public EscapedCharacter(ParseResult original)
            : base(original)
        {
            switch (Value[0])
            {
                case 'r':
                    Char = '\r';
                    break;

                case 'n':
                    Char = '\n';
                    break;

                case 't':
                    Char = '\t';
                    break;

                default:
                    Char = Value[0];
                    break;
            }
        }
    }

    internal sealed class Pattern : Factor
    {
        public readonly Regex Regex;

        public Pattern(ParseResult original) : base(original)
        {
            var regex = GetChild<RegularExpression>(0);
            var options = GetChild<PatternOptions>(1);

            Regex = new Regex(regex.PatternString, (RegexOptions)options.Options | RegexOptions.Compiled);
        }

        public override Parser Generate(ParserGenerationContext ctx)
        {
            return Regex;
        }
    }

    internal sealed class RegularExpression : ResultSubstitution
    {
        public readonly String PatternString;

        public RegularExpression(ParseResult original) : base(original)
        {
            PatternString = String.Join(String.Empty,
                GetChildren<RegexCharacter>(0, ChildCount)
                    .Select(x => x.Char).ToArray());
        }
    }

    internal abstract class RegexCharacter : ResultSubstitution
    {
        public String Char { get; protected set; }

        protected RegexCharacter(ParseResult original) : base(original)
        {
        }
    }

    internal sealed class RegexEscapedCharacter : RegexCharacter
    {
        public RegexEscapedCharacter(ParseResult original) : base(original)
        {
            switch (Value[0])
            {
                case 'r':
                    Char = "\r";
                    break;

                case 'n':
                    Char = "\n";
                    break;

                case 't':
                    Char = "\t";
                    break;

                case '\\':
                    Char = "\\";
                    break;

                default:
                    Char = "\\" + Value[0];
                    break;
            }
        }
    }

    internal sealed class RegexSingleCharacter : RegexCharacter
    {
        public RegexSingleCharacter(ParseResult original) : base(original)
        {
            Char = new String(new[] { Value[0] });
        }
    }

    internal sealed class PatternOptions : ResultSubstitution
    {
        public readonly ParseSharp.PatternOptions Options;

        public PatternOptions(ParseResult original) : base(original)
        {
            foreach (var c in Value)
            {
                switch (c)
                {
                    case 'i':
                        Options |= ParseSharp.PatternOptions.IgnoreCase;
                        break;
                }
            }
        }
    }

    internal sealed class HexEscapedCharacter : Character
    {
        public HexEscapedCharacter(ParseResult original) : base(original)
        {
            Char = (char)ushort.Parse(Value, NumberStyles.HexNumber);
        }
    }

    internal class Group : Factor
    {
        public readonly Expression Expression;

        public Group(ParseResult original) : base(original)
        {
            Expression = GetChild<Expression>(0);
        }

        public override Parser Generate(ParserGenerationContext ctx)
        {
            return Expression.Generate(ctx);
        }
    }

    internal sealed class Optional : Group
    {
        public Optional(ParseResult original) : base(original)
        {
        }

        public override Parser Generate(ParserGenerationContext ctx)
        {
            return base.Generate(ctx).Optional;
        }
    }

    internal sealed class Repeated : Group
    {
        public Repeated(ParseResult original) : base(original)
        {
        }

        public override Parser Generate(ParserGenerationContext ctx)
        {
            return base.Generate(ctx).Repeated;
        }
    }
}