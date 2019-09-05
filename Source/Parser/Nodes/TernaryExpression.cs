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
            Expression falseExpression)
            : base(condition.FirstToken)
        {
            this.Condition = condition;
            this.TernaryToken = ternaryToken;
            this.TrueExpression = trueExpression;
            this.FalseExpression = falseExpression;
        }
    }
}
