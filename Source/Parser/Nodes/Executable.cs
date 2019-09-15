using System.Collections.Generic;

namespace CSharp2Crayon.Parser.Nodes
{
    public abstract class Executable : Entity
    {
        private TopLevelEntity parent;

        public TopLevelEntity Parent { get { return this.parent; } } // TODO: get rid of the field and just use this.

        public Executable(Token firstToken, TopLevelEntity parent)
            : base(firstToken)
        {
            this.parent = parent;
        }

        protected ResolvedType DoTypeLookup(CSharpType type, ParserContext context)
        {
            return this.parent.DoTypeLookup(type, context);
        }

        public abstract IList<Executable> ResolveTypes(ParserContext context, VariableScope varScope);

        public static Executable[] ResolveTypesForCode(IList<Executable> code, ParserContext context, VariableScope varScope)
        {
            List<Executable> output = new List<Executable>();
            foreach (Executable exec in code)
            {
                output.AddRange(exec.ResolveTypes(context, varScope));
            }
            return output.ToArray();
        }

        public static Executable[] Listify(Executable e)
        {
            return new Executable[] { e };
        }
    }
}
