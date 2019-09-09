using System.Collections.Generic;

namespace CSharp2Crayon.Parser.Nodes
{
    public class WhileLoop : Executable
    {
        public Expression Condition { get; private set; }
        public Executable[] Code { get; private set; }
        public WhileLoop(Token whileToken, Expression condition, Executable[] code, TopLevelEntity parent)
            : base(whileToken, parent)
        {
            this.Condition = condition;
            this.Code = code;
        }

        public override IList<Executable> ResolveTypes(ParserContext context, VariableScope varScope)
        {
            this.Condition = this.Condition.ResolveTypes(context, varScope);
            this.Code = Executable.ResolveTypesForCode(this.Code, context, new VariableScope(varScope));
            return Listify(this);
        }
    }
}
