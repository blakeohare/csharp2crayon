using System.Collections.Generic;

namespace CSharp2Crayon.Parser.Nodes
{
    public class ExpressionAsExecutable : Executable
    {
        public Expression Expression { get; private set; }

        public ExpressionAsExecutable(Expression expr) : base(expr.FirstToken)
        {
            this.Expression = expr;
        }

        public override IList<Executable> ResolveTypes(ParserContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}
