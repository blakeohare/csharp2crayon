namespace CSharp2Crayon.Parser.Nodes
{
    public class InlineIncrement : Expression
    {
        public Expression Root { get; private set; }
        public bool IsPrefix { get; private set; }
        public bool IsIncrement { get; private set; } // as opposed to decrement
        public Token IncrementToken { get; private set; }

        public InlineIncrement(Token firstToken, Token incrementToken, bool isPrefix, Expression root, TopLevelEntity parent)
            : base(firstToken, parent)
        {
            this.Root = root;
            this.IncrementToken = incrementToken;
            this.IsPrefix = IsPrefix;
            this.IsIncrement = this.IncrementToken.Value == "++";
        }

        public override Expression ResolveTypes(ParserContext context, VariableScope varScope)
        {
            this.Root.ResolveTypes(context, varScope);
            if (!(this.Root is BracketIndex) && !(this.Root is Variable) && !(this.Root is DotField))
            {
                throw new ParserException(
                    this.IncrementToken,
                    this.IncrementToken.Value + " is not valid on this expression.");
            }
            this.ResolvedType = ResolvedType.Int();
            return this;
        }
    }
}
