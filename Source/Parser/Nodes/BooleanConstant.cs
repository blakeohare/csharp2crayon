namespace CSharp2Crayon.Parser.Nodes
{
    public class BooleanConstant : Expression
    {
        public bool Value { get; private set; }

        public BooleanConstant(Token firstToken, bool value) : base(firstToken)
        {
            this.Value = value;
        }
    }
}
