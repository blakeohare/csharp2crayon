namespace CSharp2Crayon.Parser.Nodes
{
    public class BracketIndex : Expression
    {
        public Expression Root { get; private set; }
        public Token OpenBracket { get; private set; }
        public Expression Index { get; private set; }

        public BracketIndex(Expression root, Token openBracket, Expression index, TopLevelEntity parent)
            : base(root.FirstToken, parent)
        {
            this.Root = root;
            this.OpenBracket = openBracket;
            this.Index = index;
        }

        public override Expression ResolveTypes(ParserContext context, VariableScope varScope)
        {
            this.Root = this.Root.ResolveTypes(context, varScope);
            this.Index = this.Index.ResolveTypes(context, varScope);

            ResolvedType rootType = this.Root.ResolvedType;
            ResolvedType indexType = this.Index.ResolvedType;
            if (rootType.IsArray)
            {
                if (indexType.PrimitiveType != "int") throw new ParserException(this.Index.FirstToken, "Array index must be an integer.");

                this.ResolvedType = rootType.Generics[0];
            }
            else if (rootType.IsIDictionary(context))
            {
                if (!indexType.CanBeAssignedTo(rootType.Generics[0], context))
                {
                    throw new ParserException(this.Index.FirstToken, "Incorrect key type.");
                }
                this.ResolvedType = rootType.Generics[1];
            }
            else
            {
                throw new System.NotImplementedException();
            }

            return this;
        }
    }
}
