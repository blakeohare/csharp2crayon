﻿using System.Collections.Generic;

namespace CSharp2Crayon.Parser.Nodes
{
    public abstract class Executable : Entity
    {
        private TopLevelEntity parent;

        public Executable(Token firstToken, TopLevelEntity parent)
            : base(firstToken)
        {
            this.parent = parent;
        }

        protected ResolvedType DoTypeLookup(CSharpType type, ParserContext context)
        {
            return this.parent.DoTypeLookup(type, context);
        }

        public abstract IList<Executable> ResolveTypes(ParserContext context);

        public static Executable[] ResolveTypesForCode(IList<Executable> code, ParserContext context)
        {
            List<Executable> output = new List<Executable>();
            foreach (Executable exec in code)
            {
                output.AddRange(exec.ResolveTypes(context));
            }
            return output.ToArray();
        }

        public static Executable[] Listify(Executable e)
        {
            return new Executable[] { e };
        }
    }
}
