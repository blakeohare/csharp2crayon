namespace CSharp2Crayon.Parser.Nodes
{
    public class NullConstant : Expression
    {
        public NullConstant(Token firstToken) : base(firstToken)
        {

        }

        public override Expression ResolveTypes(ParserContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}
