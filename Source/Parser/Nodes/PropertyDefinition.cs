using System.Collections.Generic;

namespace CSharp2Crayon.Parser.Nodes
{
    public class PropertyDefinition : TopLevelEntity
    {
        public CSharpType Type { get; private set; }
        public ResolvedType ResolvedType { get; private set; }
        public Token Name { get; private set; }
        public PropertyBody Getter { get; internal set; }
        public PropertyBody Setter { get; internal set; }

        public bool IsIndex { get { return this.IndexVariableName != null; } }
        public CSharpType IndexType { get; internal set; }
        public ResolvedType IndexResolvedType { get; private set; }
        public Token IndexVariableName { get; internal set; }

        public override string NameValue { get { return this.Name.Value; } }

        public PropertyDefinition(
            Token firstToken,
            Dictionary<string, Token> topLevelModifiers,
            CSharpType type,
            Token name,
            TopLevelEntity parent)
            : base(firstToken, parent)
        {
            this.ApplyModifiers(topLevelModifiers);
            this.Type = type;
            this.Name = name;
        }

        public override void ResolveTypesForSignatures(ParserContext context)
        {
            this.ResolvedType = this.DoTypeLookup(this.Type, context);
            if (this.Getter != null) this.Getter.ResolveTypesForSignatures(context);
            if (this.Setter != null) this.Setter.ResolveTypesForSignatures(context);
            if (this.IndexType != null) this.IndexResolvedType = this.DoTypeLookup(this.IndexType, context);
        }
    }
}
