namespace CSharp2Crayon.Parser.Nodes
{
    public class NegativeSign : Expression
    {
        public Expression Root { get; private set; }

        public NegativeSign(Token firstToken, Expression root, TopLevelEntity parent)
            : base(firstToken, parent)
        {
            this.Root = root;
        }

        public override Expression ResolveTypes(ParserContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}
