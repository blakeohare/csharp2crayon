namespace CSharp2Crayon.Parser.Nodes
{
    public class NegativeSign : Expression
    {
        public Expression Root { get; private set; }

        public NegativeSign(Token firstToken, Expression root) : base(firstToken)
        {
            this.Root = root;
        }
    }
}
