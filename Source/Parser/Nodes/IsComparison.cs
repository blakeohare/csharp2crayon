﻿namespace CSharp2Crayon.Parser.Nodes
{
    public class IsComparison : Expression
    {
        public CSharpType Type { get; private set; }
        public Expression Expression { get; private set; }
        public Token IsToken { get; private set; }

        public IsComparison(Token firstToken, Expression expression, Token isToken, CSharpType type)
            : base(firstToken)
        {
            this.IsToken = isToken;
            this.Expression = expression;
            this.Type = type;
        }
    }
}