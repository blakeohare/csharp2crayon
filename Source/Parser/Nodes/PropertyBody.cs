using System.Collections.Generic;

namespace CSharp2Crayon.Parser.Nodes
{
    public class PropertyBody : TopLevelEntity
    {
        public bool IsGetter { get; private set; }
        public bool IsSetter { get { return !this.IsGetter; } }
        public Executable[] Code { get; internal set; }
        public override string NameValue { get { return this.IsGetter ? "get" : "set"; } }

        public PropertyBody(Token firstToken, Dictionary<string, Token> modifiers, bool isGetter, PropertyDefinition parent)
            : base(firstToken, parent)
        {
            this.ApplyModifiers(modifiers);
            this.IsGetter = isGetter;
        }

        public override void ResolveTypesForSignatures(ParserContext context)
        {

        }

        public override void ResolveTypesInCode(ParserContext context)
        {
            PropertyDefinition propertyDef = (PropertyDefinition)this.Parent;
            VariableScope varScope = new VariableScope();
            if (this.IsSetter)
            {
                varScope.DeclareVariable("value", propertyDef.ResolvedType);
            }
            if (propertyDef.IsIndex)
            {
                varScope.DeclareVariable(propertyDef.IndexVariableName.Value, propertyDef.IndexResolvedType);
            }
            this.Code = Executable.ResolveTypesForCode(this.Code, context, varScope);
        }
    }
}
