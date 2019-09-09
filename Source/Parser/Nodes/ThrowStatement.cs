using System.Collections.Generic;

namespace CSharp2Crayon.Parser.Nodes
{
    public class ThrowStatement : Executable
    {
        public Expression Expression { get; private set; }

        public ThrowStatement(Token throwToken, Expression expr) : base(throwToken)
        {
            this.Expression = expr;
        }

        public override IList<Executable> ResolveTypes(ParserContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}
