using System.Collections.Generic;
using System.Linq;

namespace CSharp2Crayon.Parser.Nodes
{
    public class OpChain : Expression
    {
        public Expression[] Expressions { get; private set; }
        public Token[] Ops { get; private set; }

        public OpChain(IList<Expression> expressions, IList<Token> ops)
            : base(expressions[0].FirstToken)
        {
            this.Expressions = expressions.ToArray();
            this.Ops = ops.ToArray();
        }
    }
}
