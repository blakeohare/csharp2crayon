namespace CSharp2Crayon.Parser.Nodes
{
    public class AsCasting : Expression
    {
        public Expression Expression { get; private set; }
        public Token AsToken { get; private set; }
        public CSharpType Type { get; private set; }

        public AsCasting(Token firstToken, Expression expression, Token asToken, CSharpType type, TopLevelEntity parent)
            : base(firstToken, parent)
        {
            this.Expression = expression;
            this.AsToken = asToken;
            this.Type = type;
        }

        public override Expression ResolveTypes(ParserContext context, VariableScope varScope)
        {
            throw new System.NotImplementedException();
        }
    }
}
