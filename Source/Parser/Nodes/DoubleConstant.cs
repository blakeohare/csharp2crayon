namespace CSharp2Crayon.Parser.Nodes
{
    public class DoubleConstant : Expression
    {
        public double Value { get; private set; }
        public DoubleConstant(Token firstToken, double value)
            : base(firstToken)
        {
            this.Value = value;
        }

        public override Expression ResolveTypes(ParserContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}
