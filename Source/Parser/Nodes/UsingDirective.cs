using System.Collections.Generic;
using System.Linq;

namespace CSharp2Crayon.Parser.Nodes
{
    public class UsingDirective : TopLevelEntity
    {
        public Token[] Path { get; private set; }

        public UsingDirective(Token usingToken, IList<Token> parts) : base(usingToken, null)
        {
            this.Path = parts.ToArray();
        }

        public override void ResolveTypesForSignatures(ParserContext context)
        {
            // This should never get called.
            throw new System.NotImplementedException();
        }

        public override void ResolveTypesInCode(ParserContext context)
        {
            // This should never get called
            throw new System.NotImplementedException();
        }
    }
}
