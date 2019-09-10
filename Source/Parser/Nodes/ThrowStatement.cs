using System.Collections.Generic;

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

        private static readonly ResolvedType SYSTEM_EXCEPTION = ResolvedType.CreateFrameworkType(new string[] { "System", "Exception" });

        public override IList<Executable> ResolveTypes(ParserContext context, VariableScope varScope)
        {
            this.Expression = this.Expression.ResolveTypes(context, varScope);

            if (!this.Expression.ResolvedType.CanBeAssignedTo(SYSTEM_EXCEPTION, context))
            {
                throw new ParserException(this.FirstToken, "Cannot throw a non-System.Exception type.");
            }
            return Listify(this);
        }
    }
}
