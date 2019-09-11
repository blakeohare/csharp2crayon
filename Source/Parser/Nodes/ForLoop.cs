using System.Collections.Generic;
using System.Linq;

namespace CSharp2Crayon.Parser.Nodes
{
    public class ForLoop : Executable
    {
        public Executable[] InitCode { get; private set; }
        public Expression Condition { get; private set; }
        public Executable[] StepCode { get; private set; }
        public Executable[] Code { get; private set; }

        public ForLoop(Token forToken, IList<Executable> initCode, Expression condition, IList<Executable> stepCode, IList<Executable> loopBody, TopLevelEntity parent)
            : base(forToken, parent)
        {
            this.InitCode = initCode.ToArray();
            this.Condition = condition;
            this.StepCode = stepCode.ToArray();
            this.Code = loopBody.ToArray();
        }

        public override IList<Executable> ResolveTypes(ParserContext context, VariableScope varScope)
        {
            VariableScope loopScope = new VariableScope(varScope);
            this.InitCode = Executable.ResolveTypesForCode(this.InitCode, context, loopScope);
            this.Condition = this.Condition.ResolveTypes(context, loopScope);
            this.StepCode = Executable.ResolveTypesForCode(this.StepCode, context, loopScope);
            this.Code = Executable.ResolveTypesForCode(this.Code, context, loopScope);
            return Listify(this);
        }
    }
}
