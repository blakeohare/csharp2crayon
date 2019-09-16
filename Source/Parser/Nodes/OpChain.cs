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
                ResolvedType leftType = leftExpr.ResolvedType;
                ResolvedType rightType = rightExpr.ResolvedType;
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
                        if (!leftType.IsNumber) throw new ParserException(leftExpr.FirstToken, NOT_A_NUMBER_ERROR);
                        if (!rightType.IsNumber) throw new ParserException(rightExpr.FirstToken, NOT_A_NUMBER_ERROR);
                        cumulativeType = ResolvedType.Bool();
                        break;

                    case "+":
                        if (leftType.IsString || rightType.IsString)
                        {
                            cumulativeType = ResolvedType.String();
                        }
                        else
                        {
                            cumulativeType = CombineNumberTypes(leftExpr, rightExpr);
                        }
                        break;

                    case "-":
                    case "/":
                    case "*":
                        cumulativeType = CombineNumberTypes(leftExpr, rightExpr);
                        break;

                    case "<<":
                    case ">>":
                    case "&":
                    case "|":
                    case "^":
                        if (!leftType.IsIntLike) throw new ParserException(leftExpr.FirstToken, INT_REQUIRED_ERROR);
                        if (!rightType.IsIntLike) throw new ParserException(rightExpr.FirstToken, INT_REQUIRED_ERROR);
                        cumulativeType = CombineNumberTypes(leftExpr, rightExpr);
                        break;


                    case "&&":
                    case "||":
                        if (!leftType.IsBool) throw new ParserException(leftExpr.FirstToken, BOOLEAN_REQUIRED_ERROR);
                        if (!rightType.IsBool) throw new ParserException(rightExpr.FirstToken, BOOLEAN_REQUIRED_ERROR);
                        cumulativeType = ResolvedType.Bool();
                        break;

                    case "??":
                        if (!leftType.IsReferenceType) throw new ParserException(leftExpr.FirstToken, "This type is not nullable and cannot be used with '??'");
                        if (!rightType.IsReferenceType) throw new ParserException(rightExpr.FirstToken, "This type is not nullable and cannot be used with '??'");
                        if (leftType.CanBeAssignedTo(rightType, context)) cumulativeType = rightType;
                        else if (rightType.CanBeAssignedTo(leftType, context)) cumulativeType = leftType;
                        else
                        {
                            throw new ParserException(leftExpr.FirstToken, "Cannot use ?? on these two types. It is unclear what the resulting type should be.");
                        }
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
