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

        public IfStatement(Token ifToken, Expression condition, IList<Executable> ifCode, Token elseToken, IList<Executable> elseCode, TopLevelEntity parent)
            : base(ifToken, parent)
        {
            this.Condition = condition;
            this.IfCode = ifCode.ToArray();
            this.ElseToken = elseToken;
            this.ElseCode = elseCode == null ? null : elseCode.ToArray();
        }

        public override IList<Executable> ResolveTypes(ParserContext context, VariableScope varScope)
        {
            this.Condition = this.Condition.ResolveTypes(context, varScope);
            VariableScope ifScope = new VariableScope(varScope);
            this.IfCode = Executable.ResolveTypesForCode(this.IfCode, context, ifScope);
            if (this.ElseCode != null)
            {
                VariableScope elseScope = new VariableScope(varScope);
                this.ElseCode = Executable.ResolveTypesForCode(this.ElseCode, context, elseScope);
            }
            return Listify(this);
        }
    }
}
