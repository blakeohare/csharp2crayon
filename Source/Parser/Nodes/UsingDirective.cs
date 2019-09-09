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
    }
}
