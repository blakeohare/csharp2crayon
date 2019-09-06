﻿using System.Collections.Generic;

namespace CSharp2Crayon.Parser.Nodes
{
    public class ConstDefinition : TopLevelEntity
    {
        public Token Name { get; private set; }
        public CSharpType Type { get; private set; }
        public Expression Value { get; private set; }

        public ConstDefinition(Token firstToken, Dictionary<string, Token> modifiers, CSharpType constType, Token name, Expression value)
            : base(firstToken)
        {
            this.ApplyModifiers(modifiers);
            this.Name = name;
            this.Type = constType;
            this.Value = value;
        }
    }
}
