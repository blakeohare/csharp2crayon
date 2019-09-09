namespace CSharp2Crayon.Parser.Nodes
{
    public class NullConstant : Expression
    {
        public NullConstant(Token firstToken, TopLevelEntity parent)
            : base(firstToken, parent)
        { }

        public override Expression ResolveTypes(ParserContext context)
        {
            this.ResolvedType = ResolvedType.CreateNull();
            return this;
        }
    }
}
