using System;
namespace CSharp2Crayon
{
    public class ParserException : Exception
    {
        public ParserException(string message) : base(message)
        {
        }

        public ParserException(Token token, string message)
            : base(ParserException.LineInfo(token) + message) { }

        private static string LineInfo(Token token)
        {
            return token.File + ", Line: " + token.Line + ", Column: " + token.Column + " -- ";
        }
    }
}
