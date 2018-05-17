namespace ParseSharp
{
    public abstract class ResultSubstitution : ParseResult
    {
        public ResultSubstitution(ParseResult original)
            : base(original)
        {
        }
    }
}