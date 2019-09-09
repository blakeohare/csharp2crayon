using System.Collections.Generic;

namespace CSharp2Crayon.Parser.Nodes
{
    public class VariableDeclaration : Executable
    {
        public CSharpType Type { get; private set; }
        public ResolvedType ResolvedType { get; private set; }
        public Token AssignmentToken { get; private set; }
        public Token VariableName { get; private set; }
        public Expression InitialValue { get; private set; }

        public VariableDeclaration(
            Token firstToken,
            CSharpType variableType,
            Token variableName,
            Token assignmentToken,
            Expression value,
            TopLevelEntity parent)
            : base(firstToken, parent)
        {
            this.VariableName = variableName;
            this.Type = variableType;
            this.AssignmentToken = assignmentToken;
            this.InitialValue = value;
        }

        public override IList<Executable> ResolveTypes(ParserContext context)
        {
            this.ResolvedType = this.DoTypeLookup(this.Type, context);
            if (this.InitialValue != null)
            {
                this.InitialValue = this.InitialValue.ResolveTypes(context);
            }
            else
            {
                // default value
                throw new System.NotImplementedException();
            }
            return Listify(this);
        }
    }
}
