using System.Collections.Generic;

namespace CSharp2Crayon.Parser.Nodes
{
    public class DotField : Expression
    {
        public Expression Root { get; private set; }
        public Token DotToken { get; private set; }
        public Token FieldName { get; private set; }
        public CSharpType[] InlineTypeSpecification { get; set; } // e.g. .Cast<string>()

        public DotField(Token firstToken, Expression root, Token dotToken, Token fieldName, TopLevelEntity parent)
            : base(firstToken, parent)
        {
            this.Root = root;
            this.DotToken = dotToken;
            this.FieldName = fieldName;
        }

        public override Expression ResolveTypes(ParserContext context, VariableScope varScope)
        {
            return this.ResolveTypesWithoutArgs(context, varScope);
        }

        public VerifiedFieldReference ResolveTypesWithoutArgs(ParserContext context, VariableScope varScope)
        {
            this.Root = this.Root.ResolveTypes(context, varScope);

            if (this.Root.ResolvedType == null)
            {
                // should always be resolved by now.
                throw new System.InvalidOperationException();
            }

            ResolvedType rootType = this.Root.ResolvedType;
            VerifiedFieldReference fieldRef = new VerifiedFieldReference(this.FirstToken, this.parent, this.FieldName, this.Root, null);
            fieldRef.TryResolveFieldReference(context, null, varScope);
            return fieldRef;
        }
    }
}
