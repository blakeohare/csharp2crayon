namespace CSharp2Crayon.Parser.Nodes
{
    public abstract class Expression : Entity
    {
        protected TopLevelEntity parent;

        public TopLevelEntity Parent { get { return this.parent; } }
        public ResolvedType ResolvedType { get; internal set; }
        public FileContext FileContext { get { return this.parent.FileContext; } }
        public ClassLikeDefinition ClassContainer { get { return this.parent.ClassContainer; } }

        public Expression(Token firstToken, TopLevelEntity parent)
            : base(firstToken)
        {
            this.parent = parent;
        }

        internal ResolvedType DoTypeLookup(CSharpType type, ParserContext context)
        {
            return this.parent.DoTypeLookup(type, context);
        }

        internal ResolvedType DoTypeLookupFailSilently(CSharpType type, ParserContext context)
        {
            return this.parent.DoTypeLookupFailSilently(type, context);
        }

        public abstract Expression ResolveTypes(ParserContext context, VariableScope varScope);
    }
}
