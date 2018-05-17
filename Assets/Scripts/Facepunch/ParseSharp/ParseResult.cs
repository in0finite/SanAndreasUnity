using System;
using System.Collections.Generic;
using System.Linq;

namespace ParseSharp
{
    public class ParseResult : IEnumerable<ParseResult>
    {
        private readonly ParseResult[] _children;

        internal SymbolExpectedError TailError { get; set; }

        private string _value;

        public readonly string Input;
        public readonly Parser Parser;
        public readonly int Index;
        public readonly int Length;
        public readonly ParseError Error;
        public readonly bool Fatal;

        public bool Success { get { return Error == null; } }

        public int End { get { return Index + Length; } }

        public bool FlattenHierarchy { get { return Parser.OmitFromResult; } }

        public int ChildCount { get { return _children.Length; } }

        public string Value { get { return _value ?? (_value = Input.Substring(Index, Length)); } }

        internal ParseResult(ParseError e, bool fatal = true)
            : this(e.Context, null)
        {
            Error = e;
            Fatal = fatal;
        }

        internal ParseResult(ParseContext ctx, SymbolExpectedError tailError, params ParseResult[] children)
        {
            Parser = ctx.Parser;
            TailError = tailError;
            Index = ctx.InitialOffset;
            Length = ctx.Offset - ctx.InitialOffset;

            _children = children;
            Input = ctx.Input;
        }

        internal ParseResult(ParseResult original)
        {
            Parser = original.Parser;
            TailError = original.TailError;
            Index = original.Index;
            Length = original.Length;
            Error = original.Error;
            Fatal = original.Fatal;

            _children = original._children;
            Input = original.Input;
        }

        public ParseResult this[int childIndex]
        {
            get { return _children[childIndex]; }
        }

        public TResult GetChild<TResult>(int index)
            where TResult : ResultSubstitution
        {
            return _children[index] as TResult;
        }

        public TResult[] GetChildren<TResult>(int index, int count)
            where TResult : ResultSubstitution
        {
            return Enumerable.Range(index, count)
                .Select(x => _children[x] as TResult)
                .ToArray();
        }

        public IEnumerator<ParseResult> GetEnumerator()
        {
            return _children.AsEnumerable().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _children.GetEnumerator();
        }

        private string ToString(string indent)
        {
            if (!Success)
            {
                return string.Format("Parse error: {0}", Error);
            }

            if (_children.Length == 0)
            {
                return string.Format("{2}{0}: \"{1}\"",
                    Parser.Name, Value, indent);
            }

            return string.Format("{2}{0}:{1}",
                Parser.Name, string.Join("", _children
                    .Select(x => Environment.NewLine + x.ToString(indent + "  "))
                    .ToArray()), indent);
        }

        public override string ToString()
        {
            return ToString("");
        }
    }
}