using System.Collections.Generic;
using System.Linq;

namespace CSharp2Crayon.Parser.Nodes
{
    public class FunctionInvocation : Expression
    {
        public Expression Root { get; private set; }
        public Token OpenParen { get; private set; }
        public Expression[] Args { get; private set; }
        public Token[] OutTokens { get; private set; }

        public enum InvocationType
        {
            USER_METHOD,
            USER_STATIC_METHOD,
            CONSTRUCTOR,
        }
        public DotField RootAsDotField { get { return (DotField)this.Root; } }
        public ConstructorInvocationFragment RootAsConstructor { get { return (ConstructorInvocationFragment)this.Root; } }

        public FunctionInvocation(
            Token firstToken,
            Expression root,
            Token openParen,
            IList<Expression> args,
            IList<Token> outTokens,
            TopLevelEntity parent)
            : base(firstToken, parent)
        {
            this.Root = root;
            this.OpenParen = openParen;
            this.Args = args.ToArray();
            this.OutTokens = outTokens.ToArray();
        }

        public override Expression ResolveTypes(ParserContext context, VariableScope varScope)
        {
            VerifiedFieldReference vfr = null;
            if (this.Root is DotField)
            {
                vfr = ((DotField)this.Root).ResolveTypesWithoutArgs(context, varScope);
                this.Root = vfr;
            }
            else
            {
                this.Root = this.Root.ResolveTypes(context, varScope);
            }

            List<ResolvedType> argTypes = new List<ResolvedType>();
            if (vfr != null && this.Args.Length == 1 && this.Args[0] is Lambda)
            {
                ResolvedType resolvedType = vfr.GetEnumerableItemTypeGuess(context);
                ((Lambda)this.Args[0]).ResolveTypesWithExteriorHint(context, varScope, new ResolvedType[] { resolvedType });
            }
            else
            {
                for (int i = 0; i < this.Args.Length; ++i)
                {
                    this.Args[i] = this.Args[i].ResolveTypes(context, varScope);
                    argTypes.Add(this.Args[i].ResolvedType);
                }
            }

            if (this.Root is VerifiedFieldReference)
            {
                ((VerifiedFieldReference)this.Root).ResolveFieldReference(context, argTypes.ToArray(), varScope);
                ResolvedType rootType = this.Root.ResolvedType;
                if (rootType.FrameworkClass != "System.Func")
                {
                    throw new ParserException(this.OpenParen, "Cannot invoke this like a function.");
                }
                this.ResolvedType = rootType.Generics[rootType.Generics.Length - 1];
            }
            else if (this.Root is ConstructorInvocationFragment)
            {
                this.RootAsConstructor.ResolveTypesForInitialData(context, varScope);
                this.ResolvedType = this.RootAsConstructor.Class;
            }
            else
            {
                throw new System.NotImplementedException();
            }
            return this;
        }
    }
}
