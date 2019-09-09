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

        public Token BaseConstructorInvocation { get; internal set; }
        public Expression[] BaseConstructorArgs { get; internal set; }
        public Executable[] Code { get; internal set; }

        public override string NameValue { get { return "ctor"; } }

        public ConstructorDefinition(
            Token firstToken,
            Dictionary<string, Token> modifiers,
            IList<CSharpType> argTypes,
            IList<Token> argNames,
            IList<Token> argModifiers,
            TopLevelEntity parent)
            : base(firstToken, parent)
        {
            this.ApplyModifiers(modifiers);
            this.ArgTypes = argTypes.ToArray();
            this.ArgNames = argNames.ToArray();
            this.ArgModifiers = argModifiers.ToArray();
            this.BaseConstructorInvocation = null;
            this.BaseConstructorArgs = null;
        }

        public override void ResolveTypesForSignatures(ParserContext context)
        {
            this.ResolvedArgTypes = this.ArgTypes.Select(t => this.DoTypeLookup(t, context)).ToArray();
        }

        public override void ResolveTypesInCode(ParserContext context)
        {
            VariableScope varScope = new VariableScope();

            for (int i = 0; i < this.ArgNames.Length; ++i)
            {
                varScope.DeclareVariable(this.ArgNames[i].Value, this.ResolvedArgTypes[i]);
            }

            if (this.BaseConstructorArgs != null)
            {
                for (int i = 0; i < this.BaseConstructorArgs.Length; ++i)
                {
                    this.BaseConstructorArgs[i] = this.BaseConstructorArgs[i].ResolveTypes(context, varScope);
                }
            }

            this.Code = Executable.ResolveTypesForCode(this.Code, context, varScope);
        }
    }
}
