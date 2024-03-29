﻿using System.Collections.Generic;
using System.Linq;

namespace CSharp2Crayon.Parser.Nodes
{
    public class Lambda : Expression
    {
        public Token[] Args { get; private set; }
        public Token ArrowToken { get; private set; }
        public Executable[] Code { get; private set; }

        public Lambda(Token firstToken, IList<Token> args, Token arrowToken, IList<Executable> code, TopLevelEntity parent)
            : base(firstToken, parent)
        {
            this.Args = args.ToArray();
            this.ArrowToken = arrowToken;
            this.Code = code.ToArray();
        }

        public override Expression ResolveTypes(ParserContext context, VariableScope varScope)
        {
            throw new System.NotImplementedException();
        }

        public Expression ResolveTypesWithExteriorHint(
            ParserContext context,
            VariableScope varScope,
            ResolvedType[] expectedArgsAndReturnTypes)
        {
            VariableScope lambdaScope = new VariableScope(varScope);
            if (this.Args.Length != expectedArgsAndReturnTypes.Length - 1)
            {
                throw new ParserException(this.ArrowToken, "The expected number of args was " + (expectedArgsAndReturnTypes.Length - 1));
            }

            for (int i = 0; i < this.Args.Length; ++i)
            {
                lambdaScope.DeclareVariable(this.Args[i].Value, expectedArgsAndReturnTypes[i]);
            }

            this.Code = Executable.ResolveTypesForCode(this.Code, context, lambdaScope);
            ResolvedType returnType = null;
            if (this.Code.Length == 1 && this.Code[0] is ExpressionAsExecutable)
            {
                returnType = ((ExpressionAsExecutable)this.Code[0]).Expression.ResolvedType;
            }
            else if (this.Code.Length > 0 && this.Code[this.Code.Length - 1] is ReturnStatement)
            {
                ReturnStatement ret = (ReturnStatement)this.Code[this.Code.Length - 1];
                if (ret.Value == null)
                {
                    throw new ParserException(ret.FirstToken, "Return statement in lambda must have a value.");
                }
                returnType = ret.Value.ResolvedType;
            }
            else
            {
                throw new ParserException(this.FirstToken, "Not implemented: the return is hiding in this lambda.");
            }

            ResolvedType expectedReturnType = expectedArgsAndReturnTypes[expectedArgsAndReturnTypes.Length - 1];
            if (expectedReturnType == null)
            {
                expectedReturnType = returnType;
            }
            else
            {
                if (!returnType.CanBeAssignedTo(expectedReturnType, context))
                {
                    throw new ParserException(this.FirstToken, "This lambda does not seem to be returning the expected type.");
                }
            }

            List<ResolvedType> argTypes = new List<ResolvedType>(expectedArgsAndReturnTypes);
            argTypes.RemoveAt(argTypes.Count - 1);

            this.ResolvedType = ResolvedType.CreateFunction(expectedReturnType, argTypes.ToArray());
            return this;
        }
    }
}
