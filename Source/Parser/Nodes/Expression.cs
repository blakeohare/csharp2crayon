namespace CSharp2Crayon.Parser.Nodes
{
    public abstract class Expression : Entity
    {
        private TopLevelEntity parent;

        public ResolvedType ResolvedType { get; internal set; }

        public Expression(Token firstToken, TopLevelEntity parent)
            : base(firstToken)
        {
            this.parent = parent;
        }

        protected ResolvedType DoTypeLookup(CSharpType type, ParserContext context)
        {
            return this.parent.DoTypeLookup(type, context);
        }

        public abstract Expression ResolveTypes(ParserContext context);
    }
}
