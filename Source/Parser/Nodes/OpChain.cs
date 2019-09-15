using System.Collections.Generic;
using System.Linq;

namespace CSharp2Crayon.Parser.Nodes
{
    public class OpChain : Expression
    {
        public Expression[] Expressions { get; private set; }
        public Token[] Ops { get; private set; }

        public OpChain(IList<Expression> expressions, IList<Token> ops, TopLevelEntity parent)
            : base(expressions[0].FirstToken, parent)
        {
            this.Expressions = expressions.ToArray();
            this.Ops = ops.ToArray();
        }

        public override Expression ResolveTypes(ParserContext context, VariableScope varScope)
        {
            for (int i = 0; i < this.Expressions.Length; ++i)
            {
                this.Expressions[i] = this.Expressions[i].ResolveTypes(context, varScope);
            }

            ResolvedType cumulativeType = this.Expressions[0].ResolvedType;

            for (int i = 0; i < this.Ops.Length; ++i)
            {
                Expression leftExpr = this.Expressions[i];
                Expression rightExpr = this.Expressions[i + 1];
                switch (this.Ops[i].Value)
                {
                    case "==":
                    case "!=":
                        // anything is fine on either end.
                        cumulativeType = ResolvedType.Bool();
                        break;

                    case ">=":
                    case ">":
                    case "<=":
                    case "<":
                        if (!leftExpr.ResolvedType.IsNumber) throw new ParserException(leftExpr.FirstToken, NOT_A_NUMBER_ERROR);
                        if (!rightExpr.ResolvedType.IsNumber) throw new ParserException(rightExpr.FirstToken, NOT_A_NUMBER_ERROR);
                        cumulativeType = ResolvedType.Bool();
                        break;

                    case "+":
                        if (this.Expressions[i].ResolvedType.IsString || this.Expressions[i + 1].ResolvedType.IsString)
                        {
                            cumulativeType = ResolvedType.String();
                        }
                        else
                        {
                            cumulativeType = CombineNumberTypes(this.Expressions[i], this.Expressions[i + 1]);
                        }
                        break;

                    case "-":
                    case "/":
                    case "*":
                        cumulativeType = CombineNumberTypes(this.Expressions[i], this.Expressions[i + 1]);
                        break;

                    case "<<":
                    case ">>":
                    case "&":
                    case "|":
                    case "^":
                        if (!leftExpr.ResolvedType.IsIntLike) throw new ParserException(leftExpr.FirstToken, INT_REQUIRED_ERROR);
                        if (!rightExpr.ResolvedType.IsIntLike) throw new ParserException(rightExpr.FirstToken, INT_REQUIRED_ERROR);
                        cumulativeType = CombineNumberTypes(leftExpr, rightExpr);
                        break;


                    case "&&":
                    case "||":
                        if (!leftExpr.ResolvedType.IsBool) throw new ParserException(leftExpr.FirstToken, BOOLEAN_REQUIRED_ERROR);
                        if (!rightExpr.ResolvedType.IsBool) throw new ParserException(rightExpr.FirstToken, BOOLEAN_REQUIRED_ERROR);
                        cumulativeType = ResolvedType.Bool();
                        break;

                    default:
                        throw new ParserException(this.Ops[i], "The type resolution for this op is not yet implemented.");
                }
            }

            this.ResolvedType = cumulativeType;
            return this;
        }

        private static readonly string BOOLEAN_REQUIRED_ERROR = "This expression neesd to be a boolean.";
        private static readonly string INT_REQUIRED_ERROR = "This expression needs to be an integer.";
        private static readonly string NOT_A_NUMBER_ERROR = "This expression needs to be a number.";

        private static ResolvedType CombineNumberTypes(Expression left, Expression right)
        {
            if (!left.ResolvedType.IsNumber) throw new ParserException(left.FirstToken, NOT_A_NUMBER_ERROR);
            if (!right.ResolvedType.IsNumber) throw new ParserException(right.FirstToken, NOT_A_NUMBER_ERROR);
            ResolvedType leftT = left.ResolvedType;
            ResolvedType rightT = right.ResolvedType;
            string leftP = leftT.PrimitiveType;
            string rightP = rightT.PrimitiveType;

            if (leftP == "double") return leftT;
            if (rightP == "double") return rightT;
            if (leftP == "float") return leftT;
            if (rightP == "float") return rightT;
            if (leftP == "long") return leftT;
            if (rightP == "long") return rightT;
            if (leftP == "byte") return leftT;
            if (rightP == "byte") return rightT;
            if (leftP == "int") return leftT;
            if (rightP == "int") return rightT;
            throw new System.NotImplementedException();
        }
    }
}
