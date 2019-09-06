using System.Collections.Generic;
using System.Linq;

namespace CSharp2Crayon.Parser.Nodes
{
    public class ForEachLoop : Executable
    {
        public CSharpType VariableType { get; private set; }
        public Token VariableToken { get; private set; }
        public Expression ListExpression { get; private set; }
        public Executable[] Code { get; private set; }
        public ForEachLoop(Token foreachToken, CSharpType type, Token variableToken, Expression listExpression, IList<Executable> loopBody)
            : base(foreachToken)
        {
            this.VariableType = type;
            this.VariableToken = variableToken;
            this.ListExpression = listExpression;
            this.Code = loopBody.ToArray();
        }
    }
}
