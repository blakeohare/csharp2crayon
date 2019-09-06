using System.Collections.Generic;

namespace CSharp2Crayon.Parser.Nodes
{
    public class PropertyBody : TopLevelEntity
    {
        public bool IsGetter { get; private set; }
        public bool IsSetter { get { return !this.IsGetter; } }
        public Executable[] Code { get; internal set; }

        public PropertyBody(Token firstToken, Dictionary<string, Token> modifiers, bool isGetter)
            : base(firstToken)
        {
            this.ApplyModifiers(modifiers);
            this.IsGetter = isGetter;
        }
    }
}
