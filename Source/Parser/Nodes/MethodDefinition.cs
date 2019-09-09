﻿using System.Collections.Generic;
using System.Linq;

namespace CSharp2Crayon.Parser.Nodes
{
    public class MethodDefinition : TopLevelEntity
    {
        public Executable[] Code { get; internal set; }
        public CSharpType ReturnType { get; private set; }
        public Token Name { get; private set; }
        public Token[] ArgNames { get; private set; }
        public Token[] ArgModifiers { get; private set; }
        public CSharpType[] ArgTypes { get; private set; }

        public override string NameValue { get { return this.Name.Value; } }

        public MethodDefinition(
            Token firstToken,
            Dictionary<string, Token> modifiers,
            CSharpType returnType,
            Token methodName,
            IList<Token> argNames,
            IList<CSharpType> argTypes,
            IList<Token> argModifiers,
            ClassLikeDefinition parent)
            : base(firstToken, parent)
        {
            this.ApplyModifiers(modifiers);
            this.ReturnType = returnType;
            this.Name = methodName;
            this.ArgNames = argNames.ToArray();
            this.ArgTypes = argTypes.ToArray();
            this.ArgModifiers = argModifiers.ToArray();
        }
    }
}
