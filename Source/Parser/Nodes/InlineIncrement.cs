namespace CSharp2Crayon.Parser.Nodes
{
    public class InlineIncrement : Expression
    {
        public Expression Root { get; private set; }
        public bool IsPrefix { get; private set; }
        public bool IsIncrement { get; private set; } // as opposed to decrement
        public Token IncrementToken { get; private set; }

        public InlineIncrement(Token firstToken, Token incrementToken, bool isPrefix, Expression root)
            : base(firstToken)
        {
            this.Root = root;
            this.IncrementToken = incrementToken;
            this.IsPrefix = IsPrefix;
            this.IsIncrement = this.IncrementToken.Value == "++";
        }

        public override Expression ResolveTypes(ParserContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}
