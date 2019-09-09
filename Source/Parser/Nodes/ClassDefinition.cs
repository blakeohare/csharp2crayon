using System.Collections.Generic;
using System.Linq;

namespace CSharp2Crayon.Parser.Nodes
{
    public abstract class ClassLikeDefinition : TopLevelEntity
    {
        public Token ClassToken { get; private set; }
        public Token Name { get; private set; }
        public CSharpType[] RawParentClassInfoTokens { get; private set; }
        public ResolvedType[] ParentClasses { get; private set; }

        private List<TopLevelEntity> membersBuilder = new List<TopLevelEntity>();
        private TopLevelEntity[] members = null;

        public override string NameValue { get { return this.Name.Value; } }

        public ClassLikeDefinition(
            Token firstToken,
            Dictionary<string, Token> modifiers,
            Token classToken,
            Token classNameToken,
            List<CSharpType> parentClassesAndSuch,
            TopLevelEntity parent)
            : base(firstToken, parent)
        {
            this.ClassToken = classToken;
            this.Name = classNameToken;
            this.ApplyModifiers(modifiers);
            this.RawParentClassInfoTokens = parentClassesAndSuch.ToArray();
        }

        public void AddMember(TopLevelEntity tle)
        {
            // TODO: check types
            this.members = null;
            this.membersBuilder.Add(tle);
        }

        public TopLevelEntity[] Members
        {
            get
            {
                if (this.members == null)
                {
                    this.members = this.membersBuilder.ToArray();
                }
                return this.members;
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

        public void ResolveParentClasses(ParserContext context)
        {
            string[] prefixes = this.GetAllNamespaceSearchPrefixes();

            List<ResolvedType> resolvedParentTypes = new List<ResolvedType>();
            foreach (CSharpType parentClassType in this.RawParentClassInfoTokens)
            {
                ResolvedType type = ResolvedType.Create(parentClassType, prefixes, context);
                if (type == null)
                {
                    throw new ParserException(parentClassType.FirstToken, "Could not resolve parent class or interface: " + parentClassType.ToString());
                }
                resolvedParentTypes.Add(type);
            }
            this.ParentClasses = resolvedParentTypes.ToArray();
        }
    }

    public class ClassDefinition : ClassLikeDefinition
    {
        public ClassDefinition(
            Token firstToken,
            Dictionary<string, Token> modifiers,
            Token classToken,
            Token classNameToken,
            List<CSharpType> subClassesAndSuch,
            TopLevelEntity parent)
            : base(firstToken, modifiers, classToken, classNameToken, subClassesAndSuch, parent)
        { }

        public ClassLikeDefinition[] NestedClasses
        {
            get { return this.Members.OfType<ClassLikeDefinition>().ToArray(); }
        }

        public EnumDefinition[] NestedEnums
        {
            get { return this.Members.OfType<EnumDefinition>().ToArray(); }
        }
    }

    public class InterfaceDefinition : ClassLikeDefinition
    {
        public InterfaceDefinition(
            Token firstToken,
            Dictionary<string, Token> modifiers,
            Token classToken,
            Token classNameToken,
            List<CSharpType> subClassesAndSuch,
            TopLevelEntity parent)
            : base(firstToken, modifiers, classToken, classNameToken, subClassesAndSuch, parent)
        { }
    }
}
