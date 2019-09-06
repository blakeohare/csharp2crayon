using System.Collections.Generic;

namespace CSharp2Crayon.Parser.Nodes
{
    public class PropertyDefinition : TopLevelEntity
    {
        public CSharpType Type { get; private set; }
        public Token Name { get; private set; }
        public PropertyBody Getter { get; private set; }
        public PropertyBody Setter { get; private set; }

        public PropertyDefinition(
            Token firstToken,
            Dictionary<string, Token> topLevelModifiers,
            CSharpType type,
            Token name,
            PropertyBody getter,
            PropertyBody setter)
            : base(firstToken)
        {
            this.ApplyModifiers(topLevelModifiers);
            this.Type = type;
            this.Name = name;
            this.Getter = getter;
            this.Setter = setter;
        }
    }
}
