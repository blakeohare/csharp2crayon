namespace CSharp2Crayon
{
    public class Token
    {
        public string File { get; private set; }
        public string Value { get; private set; }
        public int Column { get; private set; }
        public int Line { get; private set; }
        public bool IsIdentifier
        {
            get
            {
                char c = this.Value[0];
                return (
                    (c >= 'a' && c <= 'z') ||
                    (c >= 'A' && c <= 'Z') ||
                    c == '_');
            }
        }

        public bool IsNumber
        {
            get
            {
                char c = this.Value[0];
                if (c == '.')
                {
                    if (this.Value.Length == 1) return false;
                    c = this.Value[1];
                }

                return c >= '0' && c <= '9';
            }
        }

        internal int Size { get; set; }

        public override string ToString()
        {
            return this.Value == null ? "(null)" : this.Value;
        }

        public Token(string file, string value, int col, int line)
        {
            this.File = file;
            this.Value = value;
            this.Column = col;
            this.Line = line;
            this.Size = 1;
        }
    }
}
