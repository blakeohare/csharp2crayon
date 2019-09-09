namespace CSharp2Crayon.Parser.Nodes
{
    public class Variable : Expression
    {
        public Token Name { get; private set; }

        public Variable(Token name, TopLevelEntity parent)
            : base(name, parent)
        {
            this.Name = name;
        }

        public override Expression ResolveTypes(ParserContext context, VariableScope varScope)
        {
            if (this.Name.Value == "this" || this.Name.Value == "base")
            {
                throw new System.NotImplementedException();
            }

            this.ResolvedType = varScope.GetVariableType(this.Name.Value);
            if (this.ResolvedType == null)
            {
                throw new ParserException(this.FirstToken, "This variable is not declared.");
            }
            return this;
        }
    }
}
