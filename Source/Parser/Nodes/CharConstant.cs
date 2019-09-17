namespace CSharp2Crayon.Parser.Nodes
{
    public class CharConstant : Expression
    {
        public char Value { get; private set; }

        public CharConstant(Token firstToken, string value, TopLevelEntity parent)
            : base(firstToken, parent)
        {
            if (value.Length > 1) throw new ParserException(firstToken, "This character is longer than 1 character.");
            this.Value = value[0];
        }

        public override Expression ResolveTypes(ParserContext context, VariableScope varScope)
        {
            this.ResolvedType = ResolvedType.Char();
            return this;
        }
    }
}
