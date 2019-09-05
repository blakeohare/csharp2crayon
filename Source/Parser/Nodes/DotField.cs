namespace CSharp2Crayon.Parser.Nodes
{
    public class DotField : Expression
    {
        public Expression Root { get; private set; }
        public Token DotToken { get; private set; }
        public Token FieldName { get; private set; }

        public DotField(Token firstToken, Expression root, Token dotToken, Token fieldName)
            : base(firstToken)
        {
            this.Root = root;
            this.DotToken = dotToken;
            this.FieldName = fieldName;
        }
    }
}
