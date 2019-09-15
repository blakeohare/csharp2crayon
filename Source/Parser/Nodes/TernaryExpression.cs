namespace CSharp2Crayon.Parser.Nodes
{
    public class TernaryExpression : Expression
    {
        public Expression Condition { get; private set; }
        public Token TernaryToken { get; private set; }
        public Expression TrueExpression { get; private set; }
        public Expression FalseExpression { get; private set; }

        public TernaryExpression(
            Expression condition,
            Token ternaryToken,
            Expression trueExpression,
            Expression falseExpression,
            TopLevelEntity parent)
            : base(condition.FirstToken, parent)
        {
            this.Condition = condition;
            this.TernaryToken = ternaryToken;
            this.TrueExpression = trueExpression;
            this.FalseExpression = falseExpression;
        }

        public override Expression ResolveTypes(ParserContext context, VariableScope varScope)
        {
            this.Condition = this.Condition.ResolveTypes(context, varScope);
            if (!this.Condition.ResolvedType.IsBool)
            {
                throw new ParserException(this.Condition.FirstToken, "The ternary condition must be based on a boolean.");
            }

            this.TrueExpression = this.TrueExpression.ResolveTypes(context, varScope);
            this.FalseExpression = this.FalseExpression.ResolveTypes(context, varScope);

            if (this.TrueExpression.ResolvedType.CanBeAssignedTo(this.FalseExpression.ResolvedType, context))
            {
                this.ResolvedType = this.FalseExpression.ResolvedType;
            }
            else if (this.FalseExpression.ResolvedType.CanBeAssignedTo(this.TrueExpression.ResolvedType, context))
            {
                this.ResolvedType = this.TrueExpression.ResolvedType;
            }
            else
            {
                throw new ParserException(this.TernaryToken, "I can't really figure out what type this expression is supposed to be.");
            }

            return this;
        }
    }
}
