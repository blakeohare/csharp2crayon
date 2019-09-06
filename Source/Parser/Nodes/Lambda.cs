using System.Collections.Generic;
using System.Linq;

namespace CSharp2Crayon.Parser.Nodes
{
    public class Lambda : Expression
    {
        public Token[] Args { get; private set; }
        public Token ArrowToken { get; private set; }
        public Executable[] Code { get; private set; }

        public Lambda(Token firstToken, IList<Token> args, Token arrowToken, IList<Executable> code)
            : base(firstToken)
        {
            this.Args = args.ToArray();
            this.ArrowToken = arrowToken;
            this.Code = code.ToArray();
        }
    }
}
