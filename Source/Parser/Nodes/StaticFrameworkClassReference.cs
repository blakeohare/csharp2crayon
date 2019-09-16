namespace CSharp2Crayon.Parser.Nodes
{
    public class StaticFrameworkClassReference : Expression
    {
        public StaticFrameworkClassReference(Token firstToken, TopLevelEntity parent, ResolvedType frameworkType)
            : base(firstToken, parent)
        {
            if (frameworkType.FrameworkClass == null) throw new System.InvalidOperationException();

            this.ResolvedType = frameworkType;
        }

        public override Expression ResolveTypes(ParserContext context, VariableScope varScope)
        {
            return this;
        }
    }
}
