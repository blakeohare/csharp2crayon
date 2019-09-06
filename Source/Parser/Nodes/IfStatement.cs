using System.Collections.Generic;
using System.Linq;

namespace CSharp2Crayon.Parser.Nodes
{
    public class IfStatement : Executable
    {
        public Expression Condition { get; private set; }
        public Executable[] IfCode { get; private set; }
        public Token ElseToken { get; private set; }
        public Executable[] ElseCode { get; private set; }

        public IfStatement(Token ifToken, Expression condition, IList<Executable> ifCode, Token elseToken, IList<Executable> elseCode)
            : base(ifToken)
        {
            this.Condition = condition;
            this.IfCode = ifCode.ToArray();
            this.ElseToken = elseToken;
            this.ElseCode = elseCode == null ? null : elseCode.ToArray();
        }
    }
}
