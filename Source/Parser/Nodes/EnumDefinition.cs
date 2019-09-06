using System.Collections.Generic;
using System.Linq;

namespace CSharp2Crayon.Parser.Nodes
{
    public class EnumDefinition : TopLevelEntity
    {
        public Token Name { get; private set; }
        public Token[] FieldNames { get; private set; }
        public Expression[] FieldValues { get; private set; }

        public EnumDefinition(Token firstToken, Dictionary<string, Token> modifiers, Token name, IList<Token> fieldNames, IList<Expression> fieldValues)
            : base(firstToken)
        {
            this.ApplyModifiers(modifiers);
            this.Name = name;
            this.FieldNames = fieldNames.ToArray();
            this.FieldValues = fieldValues.ToArray();
        }
    }
}
