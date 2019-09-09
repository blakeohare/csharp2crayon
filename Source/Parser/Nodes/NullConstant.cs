namespace CSharp2Crayon.Parser.Nodes
{
    public class NullConstant : Expression
    {
        public NullConstant(Token firstToken, TopLevelEntity parent)
            : base(firstToken, parent)
        { }

        public override Expression ResolveTypes(ParserContext context, VariableScope varScope)
        {
            this.ResolvedType = ResolvedType.CreateNull();
            return this;
        }
    }
}
