﻿using System.Collections.Generic;

namespace CSharp2Crayon.Parser.Nodes
{
    public class DoWhileLoop : Executable
    {
        public Executable[] Code { get; private set; }
        public Expression Condition { get; private set; }

        public DoWhileLoop(Token doToken, Executable[] code, Expression condition, TopLevelEntity parent)
            : base(doToken, parent)
        {
            this.Code = code;
            this.Condition = condition;
        }

        public override IList<Executable> ResolveTypes(ParserContext context, VariableScope varScope)
        {
            throw new System.NotImplementedException();
        }
    }
}
