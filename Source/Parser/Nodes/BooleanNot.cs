namespace CSharp2Crayon.Parser.Nodes
{
    public class BooleanNot : Expression
    {
        public Expression Root { get; private set; }

        public BooleanNot(Token firstToken, Expression root, TopLevelEntity parent)
            : base(firstToken, parent)
        {
            this.Root = root;
        }

        public override Expression ResolveTypes(ParserContext context, VariableScope varScope)
        {
            this.Root = this.Root.ResolveTypes(context, varScope);
            if (this.Root.ResolvedType.PrimitiveType != "bool")
            {
                throw new ParserException(this.Root.FirstToken, "This expression is not a boolean");
            }
            this.ResolvedType = ResolvedType.Bool();
            return this;
        }
    }
}
