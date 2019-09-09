﻿using System.Collections.Generic;

namespace CSharp2Crayon.Parser.Nodes
{
    public class ReturnStatement : Executable
    {
        public Expression Value { get; private set; }

        public ReturnStatement(Token firstToken, Expression nullableExpression)
            : base(firstToken)
        {
            this.Value = nullableExpression;
        }

        public override IList<Executable> ResolveTypes(ParserContext context)
        {
            if (this.Value != null) this.Value = this.Value.ResolveTypes(context);
            return Listify(this);
        }
    }
}
