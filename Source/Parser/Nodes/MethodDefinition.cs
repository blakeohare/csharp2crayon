using System.Collections.Generic;

namespace CSharp2Crayon.Parser.Nodes
{
    public class MethodDefinition : TopLevelEntity
    {
        public Executable[] Code { get; internal set; }
        public CSharpType ReturnType { get; private set; }
        public Token Name { get; private set; }

        public MethodDefinition(
            Token firstToken,
            Dictionary<string, Token> modifiers,
            CSharpType returnType,
            Token methodName)
            : base(firstToken)
        {
            this.ApplyModifiers(modifiers);
            this.ReturnType = returnType;
            this.Name = methodName;
        }
    }
}
