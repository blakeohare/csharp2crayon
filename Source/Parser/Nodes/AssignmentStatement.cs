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
            this.ValueExpression = this.ValueExpression.ResolveTypes(context, varScope);
            this.TargetExpression = this.TargetExpression.ResolveTypes(context, varScope);

            if (!this.ValueExpression.ResolvedType.CanBeAssignedTo(this.TargetExpression.ResolvedType, context))
            {
                throw new ParserException(this.AssignmentOp, "Cannot assign a " + this.ValueExpression.ToString() + " to a " + this.TargetExpression.ResolvedType.ToString());
            }

            return Listify(this);
        }
    }
}
