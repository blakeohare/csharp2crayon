using System.Collections.Generic;

namespace CSharp2Crayon.Parser.Nodes
{
    public class FieldDefinition : TopLevelEntity
    {
        public CSharpType Type { get; private set; }
        public Token Name { get; private set; }
        public Expression InitialValue { get; private set; }

        public FieldDefinition(Token firstToken, Dictionary<string, Token> modifiers, CSharpType type, Token name, Expression nullableInitialValue)
            : base(firstToken)
        {
            this.ApplyModifiers(modifiers);
            this.Type = type;
            this.Name = name;
            this.InitialValue = nullableInitialValue;
        }
    }
}
