using System.Collections.Generic;

namespace CSharp2Crayon.Parser.Nodes
{
    public enum AccessLevel
    {
        PUBLIC,
        PRIVATE,
        PROTECTED,
        INTERNAL,

        UNSPECIFIED,
    }
    public abstract class TopLevelEntity : Entity
    {
        public bool IsStatic { get; private set; }
        public bool IsAbstract { get; private set; }
        public AccessLevel Access { get; private set; }
        public bool IsOverride { get; private set; }

        public TopLevelEntity(Token firstToken)
            : base(firstToken)
        {
        }

        protected void ApplyModifiers(Dictionary<string, Token> modifiers)
        {
            foreach (string key in modifiers.Keys)
            {
                switch (key)
                {
                    case "abstract": this.IsAbstract = true; break;
                    case "static": this.IsStatic = true; break;
                    case "public": this.Access = AccessLevel.PUBLIC; break;
                    case "private": this.Access = AccessLevel.PRIVATE; break;
                    case "internal": this.Access = AccessLevel.INTERNAL; break;
                    case "protected": this.Access = AccessLevel.PROTECTED; break;
                    case "override": this.IsOverride = true; break;
                    default: throw new System.NotImplementedException();
                }
            }
        }
    }
}
