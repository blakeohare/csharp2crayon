using System;

namespace CSharp2Crayon.Parser.Nodes
{
    public class EnumDefinitionReference : Expression
    {

        public EnumDefinition EnumDef { get; private set; }

        public EnumDefinitionReference(Token firstToken, TopLevelEntity parent, EnumDefinition enumDef)
            : base(firstToken, parent)
        {
            this.EnumDef = enumDef;

            this.ResolvedType = ResolvedType.CreateEnum(enumDef);
        }

        public override Expression ResolveTypes(ParserContext context, VariableScope varScope)
        {
            throw new NotImplementedException();
        }
    }
}
