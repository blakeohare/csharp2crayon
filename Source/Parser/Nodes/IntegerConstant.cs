namespace CSharp2Crayon.Parser.Nodes
{
    public class IntegerConstant : Expression
    {
        public int Value { get; private set; }

        public IntegerConstant(Token firstToken, int value, TopLevelEntity parent)
            : base(firstToken, parent)
        {
            this.Value = value;
        }

        public override Expression ResolveTypes(ParserContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}
