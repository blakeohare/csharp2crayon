using System;

namespace CSharp2Crayon.Parser.Nodes
{
    public class ThisKeyword : Expression
    {
        public ThisKeyword(Token firstToken, TopLevelEntity parent)
            : base(firstToken, parent)
        { }

        public override Expression ResolveTypes(ParserContext context, VariableScope varScope)
        {
            throw new NotImplementedException();
        }
    }
}
