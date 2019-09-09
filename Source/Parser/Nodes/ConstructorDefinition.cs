using System.Collections.Generic;
using System.Linq;

namespace CSharp2Crayon.Parser.Nodes
{
    public class ConstructorDefinition : TopLevelEntity
    {
        public CSharpType[] ArgTypes { get; private set; }
        public Token[] ArgNames { get; private set; }
        public Token[] ArgModifiers { get; private set; }
        public ResolvedType[] ResolvedArgTypes { get; private set; }

        public Token BaseConstructorInvocation { get; private set; }
        public Expression[] BaseConstructorArgs { get; private set; }
        public Executable[] Code { get; internal set; }

        public override string NameValue { get { return "ctor"; } }

        public ConstructorDefinition(
            Token firstToken,
            Dictionary<string, Token> modifiers,
            IList<CSharpType> argTypes,
            IList<Token> argNames,
            IList<Token> argModifiers,
            Token nullableBaseConstructorInvocation,
            IList<Expression> nullableBaseConstructorArguments,
            TopLevelEntity parent)
            : base(firstToken, parent)
        {
            this.ApplyModifiers(modifiers);
            this.ArgTypes = argTypes.ToArray();
            this.ArgNames = argNames.ToArray();
            this.ArgModifiers = argModifiers.ToArray();
            this.BaseConstructorInvocation = nullableBaseConstructorInvocation;
            this.BaseConstructorArgs = nullableBaseConstructorArguments == null ? null : nullableBaseConstructorArguments.ToArray();
        }

        public override void ResolveTypes(ParserContext context)
        {
            this.ResolvedArgTypes = this.ArgTypes.Select(t => this.DoTypeLookup(t, context)).ToArray();
        }
    }
}
