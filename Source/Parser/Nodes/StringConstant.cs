namespace CSharp2Crayon.Parser.Nodes
{
    public class StringConstant : Expression
    {
        public string Value { get; private set; }

        public StringConstant(Token firstToken, string actualValue, TopLevelEntity parent)
            : base(firstToken, parent)
        {
            this.Value = Value;
        }

        public override Expression ResolveTypes(ParserContext context)
        {
            this.ResolvedType = ResolvedType.CreatePrimitive("string", this.FirstToken);
            return this;
        }
    }
}
