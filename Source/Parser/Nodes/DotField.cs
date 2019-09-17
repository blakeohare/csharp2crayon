using System.Collections.Generic;

namespace CSharp2Crayon.Parser.Nodes
{
    public class DotField : Expression
    {
        public Expression Root { get; internal set; }
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

        internal static Expression AttemptToResolveDotFieldChainIntoDirectReference(IList<Token> chain, ParserContext context, Expression scope)
        {
            CSharpType cst = CSharpType.Fabricate(chain);
            ResolvedType existingThing = scope.DoTypeLookupFailSilently(cst, context);
            if (existingThing != null)
            {
                if (existingThing.CustomType != null)
                {
                    TopLevelEntity tle = existingThing.CustomType;
                    if (tle is ClassLikeDefinition)
                    {
                        return new StaticClassReference(scope.FirstToken, scope.Parent, (ClassLikeDefinition)tle);
                    }
                    else
                    {
                        throw new System.NotImplementedException();
                    }
                }
                else if (existingThing.FrameworkClass != null)
                {
                    return new StaticFrameworkClassReference(scope.FirstToken, scope.Parent, existingThing);
                }
                else
                {
                    throw new System.NotImplementedException();
                }
            }
            return null;
        }

        public override Expression ResolveTypes(ParserContext context, VariableScope varScope)
        {
            // This might be a long chain of Namespace references.
            // Determine if this is a class name. Otherwise recurse down until one is.
            DotField walker = this;
            List<Token> chain = new List<Token>() { walker.FieldName };
            while (walker.Root is DotField)
            {
                chain.Add(((DotField)walker.Root).FieldName);
                walker = (DotField)walker.Root;
            }
            Variable deepestVariable = walker.Root as Variable;
            bool isChain = false;
            if (deepestVariable != null)
            {
                char c = deepestVariable.Name.Value[0];
                isChain = c >= 'A' && c <= 'Z'; // This is a lame optimization.
                chain.Add(deepestVariable.FirstToken);
                chain.Reverse();
            }

            if (isChain)
            {
                Expression newExpr = AttemptToResolveDotFieldChainIntoDirectReference(chain, context, this);
                if (newExpr != null)
                {
                    return newExpr;
                }
            }

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
