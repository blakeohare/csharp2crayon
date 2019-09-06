﻿namespace CSharp2Crayon.Parser.Nodes
{
    public class AssignmentStatement : Executable
    {
        public Expression TargetExpression { get; private set; }
        public Expression ValueExpression { get; private set; }
        public Token AssignmentOp { get; private set; }

        public AssignmentStatement(Token firstToken, Expression targetExpression, Token assignmentToken, Expression valueExpression)
            : base(firstToken)
        {
            this.AssignmentOp = assignmentToken;
            this.TargetExpression = targetExpression;
            this.ValueExpression = valueExpression;
        }
    }
}