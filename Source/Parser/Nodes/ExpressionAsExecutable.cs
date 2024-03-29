﻿using System.Collections.Generic;

namespace CSharp2Crayon.Parser.Nodes
{
    public class ExpressionAsExecutable : Executable
    {
        public Expression Expression { get; private set; }

        public ExpressionAsExecutable(Expression expr, TopLevelEntity parent) : base(expr.FirstToken, parent)
        {
            this.Expression = expr;
        }

        public override IList<Executable> ResolveTypes(ParserContext context, VariableScope varScope)
        {
            this.Expression = this.Expression.ResolveTypes(context, varScope);
            return Listify(this);
        }
    }
}
