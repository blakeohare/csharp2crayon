using System.Collections.Generic;
using System.Linq;

namespace CSharp2Crayon.Parser.Nodes
{
    public class ArrayInitialization : Expression
    {
        public CSharpType ItemType { get; private set; }

        // one of these will be null but not both
        public Expression ArrayLengthExpression { get; private set; }
        public Expression[] ArrayItems { get; private set; }

        public ArrayInitialization(
            Token firstToken,
            CSharpType itemType,
            Expression arrayLength,
            IList<Expression> arrayElements,
            TopLevelEntity parent)
            : base(firstToken, parent)
        {
            this.ItemType = itemType;
            this.ArrayLengthExpression = arrayLength;
            this.ArrayItems = arrayElements == null ? null : arrayElements.ToArray();
        }

        public override Expression ResolveTypes(ParserContext context, VariableScope varScope)
        {
            if (this.ArrayLengthExpression != null)
            {
                this.ArrayLengthExpression = this.ArrayLengthExpression.ResolveTypes(context, varScope);
            }

            if (this.ArrayItems != null)
            {
                for (int i = 0; i < this.ArrayItems.Length; ++i)
                {
                    this.ArrayItems[i] = this.ArrayItems[i].ResolveTypes(context, varScope);
                }
            }
            return this;
        }
    }
}
