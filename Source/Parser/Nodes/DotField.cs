namespace CSharp2Crayon.Parser.Nodes
{
    public class DotField : Expression
    {
        public Expression Root { get; private set; }
        public Token DotToken { get; private set; }
        public Token FieldName { get; private set; }
        public CSharpType[] InlineTypeSpecification { get; set; } // e.g. .Cast<string>()

        public DotField(Token firstToken, Expression root, Token dotToken, Token fieldName)
            : base(firstToken)
        {
            this.Root = root;
            this.DotToken = dotToken;
            this.FieldName = fieldName;
        }

        public override Expression ResolveTypes(ParserContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}
