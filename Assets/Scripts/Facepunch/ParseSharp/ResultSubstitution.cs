using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
