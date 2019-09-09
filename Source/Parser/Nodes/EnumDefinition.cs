using System.Collections.Generic;
using System.Linq;

namespace CSharp2Crayon.Parser.Nodes
{
    public class EnumDefinition : TopLevelEntity
    {
        public Token Name { get; private set; }
        public Token[] FieldNames { get; internal set; }
        public Expression[] FieldValues { get; internal set; }

        public override string NameValue { get { return this.Name.Value; } }

        public EnumDefinition(
            Token firstToken,
            Dictionary<string, Token> modifiers,
            Token name,
            TopLevelEntity parent)
            : base(firstToken, parent)
        {
            this.ApplyModifiers(modifiers);
            this.Name = name;
        }

        public override void ResolveTypesForSignatures(ParserContext context)
        {

        }

        public override void ResolveTypesInCode(ParserContext context)
        {
            for (int i = 0; i < this.FieldValues.Length; ++i)
            {
                Expression fieldValue = this.FieldValues[i];
                if (fieldValue != null)
                {
                    this.FieldValues[i] = fieldValue.ResolveTypes(context, new VariableScope());
                }
            }
        }
    }
}
