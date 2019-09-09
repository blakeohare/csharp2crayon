namespace CSharp2Crayon.Parser.Nodes
{
    public class CastExpression : Expression
    {
        public CSharpType Type { get; private set; }
        public Expression Expression { get; private set; }

        public CastExpression(Token openParen, CSharpType castType, Expression expression)
            : base(openParen)
        {
            this.Type = castType;
            this.Expression = expression;
        }

        public override Expression ResolveTypes(ParserContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}
