using System.Collections.Generic;
using System.Linq;

namespace CSharp2Crayon.Parser.Nodes
{
    public class MultiVariableDeclaration : Executable
    {
        public CSharpType Type { get; private set; }
        public Token[] Names { get; private set; }

        public MultiVariableDeclaration(Token firstToken, CSharpType type, IList<Token> names)
            : base(firstToken)
        {
            this.Type = type;
            this.Names = names.ToArray();
        }

        public override IList<Executable> ResolveTypes(ParserContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}
