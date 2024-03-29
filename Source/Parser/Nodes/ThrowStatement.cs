﻿using System.Collections.Generic;

namespace CSharp2Crayon.Parser.Nodes
{
    public class ThrowStatement : Executable
    {
        public Expression Expression { get; private set; }

        public ThrowStatement(Token throwToken, Expression expr, TopLevelEntity parent)
            : base(throwToken, parent)
        {
            this.Expression = expr;
        }

        public override IList<Executable> ResolveTypes(ParserContext context, VariableScope varScope)
        {
            this.Expression = this.Expression.ResolveTypes(context, varScope);

            if (!this.Expression.ResolvedType.IsException(context))
            {
                throw new ParserException(this.FirstToken, "Cannot throw a non-System.Exception type.");
            }
            return Listify(this);
        }
    }
}
