using System;
namespace CSharp2Crayon
{
    public class Token
    {
        public string File { get; private set; }
        public string Value { get; private set; }
        public int Column { get; private set; }
        public int Line { get; private set; }

        public Token(string file, string value, int col, int line)
        {
            this.File = file;
            this.Value = value;
            this.Column = col;
            this.Line = line;
        }
    }
}
