namespace CSharp2Crayon.Parser.Nodes
{
    public class Variable : Expression
    {
        public Token Name { get; private set; }

        public Variable(Token name) : base(name)
        {
            this.Name = name;
        }

        public override Expression ResolveTypes(ParserContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}
