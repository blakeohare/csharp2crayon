namespace CSharp2Crayon.Parser.Nodes
{
    // This class should not survive past the resolver phase
    public class ConstructorInvocationFragment : Expression
    {
        public CSharpType ClassName { get; private set; }

        public ConstructorInvocationFragment(Token newKeywordToken, CSharpType classNameAsAType)
            : base(newKeywordToken)
        {
            this.ClassName = classNameAsAType;
        }
    }
}
