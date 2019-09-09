namespace CSharp2Crayon.Parser.Nodes
{
    public abstract class Expression : Entity
    {
        public Expression(Token firstToken)
            : base(firstToken)
        { }

        public abstract Expression ResolveTypes(ParserContext context);
    }
}
