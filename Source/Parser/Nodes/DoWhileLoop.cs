namespace CSharp2Crayon.Parser.Nodes
{
    public class DoWhileLoop : Executable
    {
        public Executable[] Code { get; private set; }
        public Expression Condition { get; private set; }

        public DoWhileLoop(Token doToken, Executable[] code, Expression condition)
            : base(doToken)
        {
            this.Code = code;
            this.Condition = condition;
        }
    }
}
