namespace CSharp2Crayon.Parser.Nodes
{
    public class BooleanConstant : Expression
    {
        public bool Value { get; private set; }

        public BooleanConstant(Token firstToken, bool value, TopLevelEntity parent)
            : base(firstToken, parent)
        {
            this.Value = value;
        }

        public override Expression ResolveTypes(ParserContext context, VariableScope varScope)
        {
            throw new System.NotImplementedException();
        }
    }
}
