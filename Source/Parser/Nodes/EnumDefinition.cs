using System.Collections.Generic;

namespace CSharp2Crayon.Parser.Nodes
{
    public class EnumDefinition : TopLevelEntity
    {
        public Token Name { get; private set; }
        public Token[] FieldNames { get; internal set; }
        public Expression[] FieldValues { get; internal set; }

        public override string NameValue { get { return this.Name.Value; } }

        public EnumDefinition(
            Token firstToken,
            Dictionary<string, Token> modifiers,
            Token name,
            TopLevelEntity parent)
            : base(firstToken, parent)
        {
            this.ApplyModifiers(modifiers);
            this.Name = name;
        }

        private Dictionary<string, Expression> nameToValue = null;

        public Expression GetValue(string field)
        {
            if (this.nameToValue == null)
            {
                this.HasField("X");
            }
            Expression expr;
            return this.nameToValue.TryGetValue(field, out expr) ? expr : null;
        }

        public bool HasField(string field)
        {
            if (this.nameToValue == null)
            {
                this.nameToValue = new Dictionary<string, Expression>();
                for (int i = 0; i < this.FieldNames.Length; ++i)
                {
                    this.nameToValue[this.FieldNames[i].Value] = this.FieldValues[i];
                }
            }
            return this.nameToValue.ContainsKey(field);
        }

        public override void ResolveTypesForSignatures(ParserContext context)
        {

        }

        public override void ResolveTypesInCode(ParserContext context)
        {
            for (int i = 0; i < this.FieldValues.Length; ++i)
            {
                Expression fieldValue = this.FieldValues[i];
                if (fieldValue != null)
                {
                    this.FieldValues[i] = fieldValue.ResolveTypes(context, new VariableScope());
                    ResolvedType t = this.FieldValues[i].ResolvedType;
                    if (t.PrimitiveType != "int" && !t.IsEnumField)
                    {
                        throw new ParserException(this.FieldValues[i].FirstToken, "This is not a valid value for an enum field.");
                    }
                }
            }
            this.nameToValue = null;
        }
    }
}
