namespace CSharp2Crayon.Parser.Nodes
{
    public class NullConstant : Expression
    {
        public NullConstant(Token firstToken) : base(firstToken)
        {

        }

        public override Expression ResolveTypes(ParserContext context)
        {
            this.Type = ResolvedType.CreateNull();
            return this;
        }
    }
}
