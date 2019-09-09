using System.Collections.Generic;
using System.Linq;

namespace CSharp2Crayon.Parser.Nodes
{
    public class FunctionInvocation : Expression
    {
        public Expression Root { get; private set; }
        public Token OpenParen { get; private set; }
        public Expression[] Args { get; private set; }
        public Token[] OutTokens { get; private set; }

        public FunctionInvocation(
            Token firstToken,
            Expression root,
            Token openParen,
            IList<Expression> args,
            IList<Token> outTokens,
            TopLevelEntity parent)
            : base(firstToken, parent)
        {
            this.Root = root;
            this.OpenParen = openParen;
            this.Args = args.ToArray();
            this.OutTokens = outTokens.ToArray();
        }

        public override Expression ResolveTypes(ParserContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}
