﻿namespace CSharp2Crayon.Parser.Nodes
{
    public class IntegerConstant : Expression
    {
        public int Value { get; private set; }

        public IntegerConstant(Token firstToken, int value) : base(firstToken)
        {
            this.Value = value;
        }
    }
}