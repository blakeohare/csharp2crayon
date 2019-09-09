namespace CSharp2Crayon.Parser.Nodes
{
    public class CastExpression : Expression
    {
        public CSharpType Type { get; private set; }
        public Expression Expression { get; private set; }

        public CastExpression(Token openParen, CSharpType castType, Expression expression, TopLevelEntity parent)
            : base(openParen, parent)
        {
            this.Type = castType;
            this.Expression = expression;
        }

        public override Expression ResolveTypes(ParserContext context)
        {
            this.ResolvedType = this.DoTypeLookup(this.Type, context);
            this.Expression = this.Expression.ResolveTypes(context);
            return this;
        }
    }
}
