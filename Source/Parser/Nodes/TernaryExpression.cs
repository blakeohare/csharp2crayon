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
            throw new System.NotImplementedException();
        }
    }
}
