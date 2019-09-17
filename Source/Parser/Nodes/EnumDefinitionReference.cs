using System;

namespace CSharp2Crayon.Parser.Nodes
{
    public class EnumDefinitionReference : Expression
    {
        public EnumDefinitionReference(Token firstToken, TopLevelEntity parent, ResolvedType enumAsType)
            : base(firstToken, parent)
        {
            this.ResolvedType = enumAsType;
        }

        public override Expression ResolveTypes(ParserContext context, VariableScope varScope)
        {
            throw new NotImplementedException();
        }
    }
}
