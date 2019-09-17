namespace CSharp2Crayon.Parser.Nodes
{
    public class StaticClassReference : Expression
    {
        public ClassLikeDefinition ClassDef { get; private set; }

        public StaticClassReference(Token firstToken, TopLevelEntity parent, ClassLikeDefinition classDef)
            : base(firstToken, parent)
        {
            this.ClassDef = classDef;
            this.ResolvedType = ResolvedType.FromClass(classDef);
        }

        public override Expression ResolveTypes(ParserContext context, VariableScope varScope)
        {
            return this;
        }
    }
}
