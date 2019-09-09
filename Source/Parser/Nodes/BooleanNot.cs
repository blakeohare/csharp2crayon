namespace CSharp2Crayon.Parser.Nodes
{
    public class BooleanNot : Expression
    {
        public Expression Root { get; private set; }

        public BooleanNot(Token firstToken, Expression root) : base(firstToken)
        {
            this.Root = root;
        }

        public override Expression ResolveTypes(ParserContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}
