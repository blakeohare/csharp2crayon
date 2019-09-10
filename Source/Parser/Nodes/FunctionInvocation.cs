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
            if (this.Root is DotField)
            {
                this.Root = ((DotField)this.Root).ResolveTypesWithoutArgs(context, varScope);
            }
            else
            {
                this.Root = this.Root.ResolveTypes(context, varScope);
            }

            List<ResolvedType> argTypes = new List<ResolvedType>();
            for (int i = 0; i < this.Args.Length; ++i)
            {
                this.Args[i] = this.Args[i].ResolveTypes(context, varScope);
                argTypes.Add(this.Args[i].ResolvedType);
            }

            if (this.Root is VerifiedFieldReference)
            {
                ((VerifiedFieldReference)this.Root).ResolveMethodReference(context, argTypes.ToArray());
                ResolvedType rootType = this.Root.ResolvedType;
                if (rootType.FrameworkClass != "System.Function")
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
