namespace CSharp2Crayon.Parser.Nodes
{
    public class BracketIndex : Expression
    {
        public Expression Root { get; private set; }
        public Token OpenBracket { get; private set; }
        public Expression Index { get; private set; }

        public BracketIndex(Expression root, Token openBracket, Expression index)
            : base(root.FirstToken)
        {
            this.Root = root;
            this.OpenBracket = openBracket;
            this.Index = index;
        }

        public override Expression ResolveTypes(ParserContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}
