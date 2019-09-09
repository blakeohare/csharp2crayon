using System.Collections.Generic;

namespace CSharp2Crayon.Parser.Nodes
{
    public class FieldDefinition : TopLevelEntity
    {
        public CSharpType Type { get; private set; }
        public ResolvedType ResolvedType { get; private set; }

        public Token Name { get; private set; }
        public Expression InitialValue { get; internal set; }

        public override string NameValue { get { return this.Name.Value; } }

        public FieldDefinition(Token firstToken, Dictionary<string, Token> modifiers, CSharpType type, Token name, Expression nullableInitialValue, TopLevelEntity parent)
            : base(firstToken, parent)
        {
            this.ApplyModifiers(modifiers);
            this.Type = type;
            this.Name = name;
            this.InitialValue = nullableInitialValue;
        }

        public override void ResolveTypesForSignatures(ParserContext context)
        {
            this.ResolvedType = this.DoTypeLookup(this.Type, context);
        }

        public override void ResolveTypesInCode(ParserContext context)
        {
            if (this.InitialValue != null)
            {
                this.InitialValue = this.InitialValue.ResolveTypes(context);
            }
        }
    }
}
