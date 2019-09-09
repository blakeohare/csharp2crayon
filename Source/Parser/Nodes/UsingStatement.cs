using System.Collections.Generic;
using System.Linq;

namespace CSharp2Crayon.Parser.Nodes
{
    public class UsingStatement : Executable
    {
        public CSharpType Type { get; private set; }
        public Token VariableName { get; private set; }
        public Expression Expression { get; private set; }
        public Executable[] Code { get; private set; }

        public UsingStatement(Token firstToken, CSharpType variableType, Token variableName, Expression expression, IList<Executable> code, TopLevelEntity parent)
            : base(firstToken, parent)
        {
            this.Type = variableType;
            this.VariableName = variableName;
            this.Expression = expression;
            this.Code = code.ToArray();
        }

        public override IList<Executable> ResolveTypes(ParserContext context, VariableScope varScope)
        {
            throw new System.NotImplementedException();
        }
    }
}
