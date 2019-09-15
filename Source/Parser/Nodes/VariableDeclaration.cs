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

            if (this.FirstToken.File.Contains("AssemblyDependencyResolver.cs") && this.FirstToken.Line == 53)
            {

            }
            this.VariableName = variableName;
            this.Type = variableType;
            this.AssignmentToken = assignmentToken;
            this.InitialValue = value;
        }

        public override IList<Executable> ResolveTypes(ParserContext context, VariableScope varScope)
        {
            this.ResolvedType = this.DoTypeLookup(this.Type, context);
            if (this.InitialValue != null)
            {
                this.InitialValue = this.InitialValue.ResolveTypes(context, varScope);
            }
            else
            {
                if (this.ResolvedType.FrameworkClass != null ||
                    this.ResolvedType.CustomType != null ||
                    this.ResolvedType.PrimitiveType == "string")
                {
                    this.InitialValue = new NullConstant(this.FirstToken, this.Parent);
                    this.InitialValue.ResolvedType = this.ResolvedType;
                }
                else if (this.ResolvedType.PrimitiveType != null)
                {
                    switch (this.ResolvedType.PrimitiveType)
                    {
                        case "int":
                            this.InitialValue = new IntegerConstant(this.FirstToken, 0, this.Parent)
                            {
                                ResolvedType = ResolvedType.Int()
                            };
                            break;

                        case "double":
                            this.InitialValue = new DoubleConstant(this.FirstToken, 0.0, this.Parent)
                            {
                                ResolvedType = ResolvedType.Double()
                            };
                            break;

                        default:
                            throw new System.NotImplementedException();
                    }
                }
                else
                {
                    throw new System.NotImplementedException();
                }
            }

            varScope.DeclareVariable(this.VariableName.Value, this.ResolvedType);

            return Listify(this);
        }
    }
}
