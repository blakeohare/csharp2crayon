using System.Collections.Generic;
using System.Linq;

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

        private FileContext fileContext = null;
        public FileContext FileContext
        {
            get
            {
                if (this.fileContext != null) return this.fileContext;
                if (this.Parent != null) return this.Parent.FileContext;
                return null;
            }
            set
            {
                this.fileContext = value;
            }
        }

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
                    path.AddRange(this.NameValue.Split('.'));
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

        private string[] namespaceSearchPrefixes = null;
        private string[] GetAllNamespaceSearchPrefixes()
        {
            if (this.namespaceSearchPrefixes == null)
            {
                List<string> output = new List<string>() { "" };
                List<string> namespaceChain = new List<string>(this.FullyQualifiedNameParts);
                while (namespaceChain.Count > 0)
                {
                    output.Add(string.Join(".", namespaceChain));
                    namespaceChain.RemoveAt(namespaceChain.Count - 1);
                }
                output.AddRange(this.FileContext.NamespaceSearchPrefixes);

                this.namespaceSearchPrefixes = output
                    .Select(ns => (ns.Length == 0) ? ns : (ns + "."))
                    .ToArray();
            }
            return this.namespaceSearchPrefixes;
        }

        protected ResolvedType DoTypeLookup(CSharpType type, ParserContext context)
        {
            ResolvedType rType = ResolvedType.Create(type, this.GetAllNamespaceSearchPrefixes(), context);
            if (rType == null) throw new ParserException(type.FirstToken, "Could not resolve parent class or interface: " + type.ToString());
            return rType;
        }

        public abstract void ResolveTypesForSignatures(ParserContext context);
    }
}
