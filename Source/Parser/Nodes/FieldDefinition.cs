using System.Collections.Generic;

namespace CSharp2Crayon.Parser.Nodes
{
    public class FieldDefinition : TopLevelEntity
    {
        public CSharpType Type { get; private set; }
        public ResolvedType ResolvedType { get; private set; }

        public Token Name { get; private set; }
        public Expression InitialValue { get; private set; }

        public override string NameValue { get { return this.Name.Value; } }

        public FieldDefinition(Token firstToken, Dictionary<string, Token> modifiers, CSharpType type, Token name, Expression nullableInitialValue, TopLevelEntity parent)
            : base(firstToken, parent)
        {
            this.ApplyModifiers(modifiers);
            this.Type = type;
            this.Name = name;
            this.InitialValue = nullableInitialValue;
        }

        public override void ResolveTypes(ParserContext context)
        {
            this.ResolvedType = this.DoTypeLookup(this.Type, context);
        }
    }
}
