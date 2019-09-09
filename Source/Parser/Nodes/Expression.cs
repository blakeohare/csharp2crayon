namespace CSharp2Crayon.Parser.Nodes
{
    public abstract class Expression : Entity
    {
        public ResolvedType Type { get; internal set; }

        public Expression(Token firstToken)
            : base(firstToken)
        { }

        public abstract Expression ResolveTypes(ParserContext context);
    }
}
