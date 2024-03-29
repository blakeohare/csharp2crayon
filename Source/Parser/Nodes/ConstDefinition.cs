﻿using System.Collections.Generic;

namespace CSharp2Crayon.Parser.Nodes
{
    public class ConstDefinition : TopLevelEntity
    {
        public Token Name { get; private set; }
        public CSharpType Type { get; private set; }
        public ResolvedType ResolvedType { get; private set; }
        public Expression Value { get; internal set; }

        public override string NameValue { get { return this.Name.Value; } }

        public ConstDefinition(
            Token firstToken,
            Dictionary<string, Token> modifiers,
            CSharpType constType,
            Token name,
            TopLevelEntity parent)
            : base(firstToken, parent)
        {
            this.ApplyModifiers(modifiers);
            this.Name = name;
            this.Type = constType;
        }

        public override void ResolveTypesForSignatures(ParserContext context)
        {
            this.ResolvedType = this.DoTypeLookup(this.Type, context);
        }

        public override void ResolveTypesInCode(ParserContext context)
        {
            this.Value = this.Value.ResolveTypes(context, new VariableScope());
        }
    }
}
