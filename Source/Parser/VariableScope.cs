using System.Collections.Generic;

namespace CSharp2Crayon.Parser
{
    public class VariableScope
    {
        private Dictionary<string, ResolvedType> varsToTypes = new Dictionary<string, ResolvedType>();
        private VariableScope parent;

        public VariableScope() : this(null) { }

        public VariableScope(VariableScope parent)
        {
            this.parent = parent;
        }

        public void DeclareVariable(string name, ResolvedType type)
        {
            this.varsToTypes[name] = type;
        }

        public ResolvedType GetVariableType(string name)
        {
            ResolvedType output;
            return this.varsToTypes.TryGetValue(name, out output)
                ? output
                : (this.parent == null
                    ? null
                    : this.parent.GetVariableType(name));
        }
    }
}
