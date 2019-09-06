using System.Collections.Generic;
using System.Linq;

namespace CSharp2Crayon.Parser.Nodes
{
    public class ConstructorDefinition : TopLevelEntity
    {
        public CSharpType[] ArgTypes { get; private set; }
        public Token[] ArgNames { get; private set; }
        public Token BaseConstructorInvocation { get; private set; }
        public Expression[] BaseConstructorArgs { get; private set; }
        public Executable[] Code { get; internal set; }

        public ConstructorDefinition(
            Token firstToken,
            Dictionary<string, Token> modifiers,
            IList<CSharpType> argTypes,
            IList<Token> argNames,
            Token nullableBaseConstructorInvocation,
            IList<Expression> nullableBaseConstructorArguments)
            : base(firstToken)
        {
            this.ApplyModifiers(modifiers);
            this.ArgTypes = argTypes.ToArray();
            this.ArgNames = argNames.ToArray();
            this.BaseConstructorInvocation = nullableBaseConstructorInvocation;
            this.BaseConstructorArgs = nullableBaseConstructorArguments == null ? null : nullableBaseConstructorArguments.ToArray();
        }
    }
}
