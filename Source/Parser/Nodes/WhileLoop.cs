using System.Collections.Generic;

namespace CSharp2Crayon.Parser.Nodes
{
    public class WhileLoop : Executable
    {
        public Expression Condition { get; private set; }
        public Executable[] Code { get; private set; }
        public WhileLoop(Token whileToken, Expression condition, Executable[] code)
            : base(whileToken)
        {
            this.Condition = condition;
            this.Code = code;
        }

        public override IList<Executable> ResolveTypes(ParserContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}
