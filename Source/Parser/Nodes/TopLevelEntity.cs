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
        public bool IsReadOnly { get; private set; }
        public bool IsVirtual { get; private set; }
        public FileContext FileContext { get; set; }

        public TopLevelEntity Parent { get; private set; }

        private string[] fullyQualifiedNameParts = null;
        public string[] FullyQualifiedNameParts
        {
            get
            {
                if (this.fullyQualifiedNameParts == null)
                {
                    List<string> path = new List<string>();
                    if (this.Parent != null)
                    {
                        path.AddRange(this.Parent.FullyQualifiedNameParts);
                    }
                    path.Add(this.NameValue);
                    this.fullyQualifiedNameParts = path.ToArray();
                }
                return this.fullyQualifiedNameParts;
            }
        }

        public virtual string NameValue { get; }

        public TopLevelEntity(Token firstToken, TopLevelEntity parent)
            : base(firstToken)
        {
            this.Parent = parent;
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
                    case "readonly": this.IsReadOnly = true; break;
                    case "virtual": this.IsVirtual = true; break;
                    default: throw new System.NotImplementedException();
                }
            }
        }
    }
}
