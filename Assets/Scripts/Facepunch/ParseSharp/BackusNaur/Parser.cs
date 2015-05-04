using System;

namespace ParseSharp.BackusNaur
{
    public class ExtendedBackusNaurFormParser : WrappedParser
    {
        private static readonly Parser _sRootParser = ConstructRootParser();

        private static Parser ConstructRootParser()
        {
            var parser = LateDefined<ParserGenerator>();
            var rule = LateDefined<Rule>();
            var ruleOption = LateDefined<RuleOption>();
            var ident = LateDefined<Identifier>();
            var identWord = LateDefined<IdentifierWord>(LateDefinedType.Collapse);
            var expr = LateDefined<Expression>();
            var altern = LateDefined<Alternation>();
            var concat = LateDefined<Concatenation>();
            var factor = LateDefined("Factor", LateDefinedType.OmitFromHierarchy);
            var literal = LateDefined<Literal>();
            var extension = LateDefined("Extension", LateDefinedType.OmitFromHierarchy);
            var pattern = LateDefined<Pattern>();
            var regex = LateDefined<RegularExpression>();
            var regexEscapedChar = LateDefined<RegexEscapedCharacter>(LateDefinedType.Collapse);
            var regexChar = LateDefined<RegexSingleCharacter>(LateDefinedType.Collapse);
            var regexOptions = LateDefined<PatternOptions>(LateDefinedType.Collapse);
            var character = LateDefined("Character", LateDefinedType.OmitFromHierarchy);
            var singleChar = LateDefined<SingleCharacter>(LateDefinedType.Collapse);
            var simpleEscapedChar = LateDefined<EscapedCharacter>(LateDefinedType.Collapse);
            var hexEscapedChar = LateDefined<HexEscapedCharacter>(LateDefinedType.Collapse);
            var group = LateDefined<Group>();
            var optional = LateDefined<Optional>();
            var repeated = LateDefined<Repeated>();

            parser.Definition = (rule.Repeated + EndOfInput).IgnoreWhitespace;

            rule.Definition = ("(*" + ruleOption.Repeated + "*)").Optional + ident + "=" + expr + ";";
            ruleOption.Definition = (Parser) "collapse" | "match-whitespace" | "skip-whitespace" | "omit-from-hierarchy";
            ident.Definition = identWord + identWord.Repeated;
            identWord.Definition = Pattern(@"[A-Za-z][A-Za-z0-9]+").MatchWhitespace;

            expr.Definition = altern;
            altern.Definition = concat + ("|" + concat).Repeated;
            concat.Definition = factor + ("," + factor).Repeated;
            factor.Definition = literal | ident | extension | group | optional | repeated;

            literal.Definition = ("\"" + character.Repeated + "\"").MatchWhitespace;

            character.Definition = singleChar | ("\\" + (simpleEscapedChar | ("x" + hexEscapedChar)));
            singleChar.Definition = Pattern(@"[^\\\n""]");
            simpleEscapedChar.Definition = Pattern(@"[\\""rnt]");
            hexEscapedChar.Definition = Pattern(@"[0-9A-Fa-f]{1,4}");

            extension.Definition = "?" + (pattern | ident) + "?";

            pattern.Definition = ("/" + regex + "/" + regexOptions).MatchWhitespace;
            regex.Definition = (("\\" + regexEscapedChar) | regexChar).Repeated;
            regexEscapedChar.Definition = Pattern("[\\rnt]");
            regexChar.Definition = Pattern("[^\\/\n]");
            regexOptions.Definition = Pattern("[i]*");

            group.Definition = "(" + expr + ")";
            optional.Definition = "[" + expr + "]";
            repeated.Definition = "{" + expr + "}";

            return parser;
        }

        public ExtendedBackusNaurFormParser() : base(_sRootParser) { }

        public new ParserGenerator Parse(string input, int offset = 0)
        {
            var result = base.Parse(input, offset);

            if (!result.Success) {
                throw new Exception(result.Error.ToString());
            }

            return result.GetChild<ParserGenerator>(0);
        }
    }
}
