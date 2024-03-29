﻿using System.Collections.Generic;
using System.Linq;

namespace CSharp2Crayon.Parser.Nodes
{
    public class ForEachLoop : Executable
    {
        public CSharpType VariableType { get; private set; }
        public Token VariableToken { get; private set; }
        public Expression ListExpression { get; private set; }
        public Executable[] Code { get; private set; }
        public ForEachLoop(Token foreachToken, CSharpType type, Token variableToken, Expression listExpression, IList<Executable> loopBody, TopLevelEntity parent)
            : base(foreachToken, parent)
        {
            this.VariableType = type;
            this.VariableToken = variableToken;
            this.ListExpression = listExpression;
            this.Code = loopBody.ToArray();
        }

        public override IList<Executable> ResolveTypes(ParserContext context, VariableScope varScope)
        {
            this.ListExpression = this.ListExpression.ResolveTypes(context, varScope);
            VariableScope loopScope = new VariableScope(varScope);
            ResolvedType itemType = this.ListExpression.ResolvedType.GetEnumerableItemType();
            if (itemType == null)
            {
                throw new ParserException(this.ListExpression.FirstToken, "This expression is not enumerable.");
            }
            loopScope.DeclareVariable(this.VariableToken.Value, itemType);
            this.Code = Executable.ResolveTypesForCode(this.Code, context, loopScope);
            return Listify(this);
        }
    }
}
