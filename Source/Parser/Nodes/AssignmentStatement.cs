using System.Collections.Generic;

namespace CSharp2Crayon.Parser.Nodes
{
    public class AssignmentStatement : Executable
    {
        public Expression TargetExpression { get; private set; }
        public Expression ValueExpression { get; private set; }
        public Token AssignmentOp { get; private set; }

        public AssignmentStatement(Token firstToken, Expression targetExpression, Token assignmentToken, Expression valueExpression, TopLevelEntity parent)
            : base(firstToken, parent)
        {
            this.AssignmentOp = assignmentToken;
            this.TargetExpression = targetExpression;
            this.ValueExpression = valueExpression;
        }

        public override IList<Executable> ResolveTypes(ParserContext context, VariableScope varScope)
        {
            throw new System.NotImplementedException();
        }
    }
}
