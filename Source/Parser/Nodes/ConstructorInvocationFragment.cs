namespace CSharp2Crayon.Parser.Nodes
{
    // This class should not survive past the resolver phase
    public class ConstructorInvocationFragment : Expression
    {
        public CSharpType ClassName { get; private set; }

        public Token[] InitialDataPropertyNames { get; internal set; }
        public Expression[] InitialDataKeys { get; internal set; }
        public Expression[] InitialDataValues { get; internal set; }

        public ConstructorInvocationFragment(Token newKeywordToken, CSharpType classNameAsAType)
            : base(newKeywordToken)
        {
            this.ClassName = classNameAsAType;
        }

        public override Expression ResolveTypes(ParserContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}
