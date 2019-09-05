using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharp2Crayon
{
    public class TokenStream
    {
        private int index = 0;
        private int length = 0;
        private List<Token> tokens = new List<Token>();

        public TokenStream AddFile(string name, string content)
        {
            this.tokens.AddRange(Tokenizer.Tokenize(name, content));
            this.length = this.tokens.Count;
            return this;
        }

        public int CurrentState { get { return this.index; } }
        public void RestoreState(int value) { this.index = value; }

        public bool HasMore
        {
            get { return this.index < this.length; }
        }

        public Token Pop()
        {
            if (this.index < this.length)
            {
                return this.tokens[this.index++];
            }
            throw new ParserException("Unexpected EOF");
        }

        public string PeekValue()
        {
            if (this.index < this.length)
            {
                return this.tokens[this.index].Value;
            }
            return null;
        }

        public Token Peek()
        {
            if (this.index < this.length)
            {
                return this.tokens[this.index];
            }
            return null;
        }

        public bool PopIfPresent(string value)
        {
            if (this.index >= this.length) return false;
            Token t = this.tokens[this.index];
            if (t.Value == value)
            {
                this.index++;
                return true;
            }
            return false;
        }

        public Token PopExpected(string value)
        {
            if (this.index < this.length)
            {
                Token t = this.tokens[this.index++];
                if (t.Value == value) return t;
                throw new ParserException(t, "Expected '" + value + "' but found '" + t.Value + "'");
            }
            throw new ParserException("Unexpected EOF. Expected '" + value + "'");
        }

        public bool IsNext(string value)
        {
            return this.index < this.length && this.tokens[this.index].Value == value;
        }

        public Token PopWord()
        {
            if (this.index >= this.length) throw new ParserException("Unexpected EOF");
            Token word = this.PopWordIfPresent();
            if (word != null) return word;
            Token t = this.tokens[this.index];
            throw new ParserException(t, "Expected an alphanumeric word but found '" + t.Value + "'");
        }

        public Token PopWordIfPresent()
        {
            if (this.index >= this.length) return null;
            Token t = this.tokens[this.index++];
            char c = t.Value[0];
            if ((c >= 'a' && c <= 'z') ||
                (c >= 'A' && c <= 'Z') ||
                (c >= '0' && c <= '9') ||
                c == '_')
            {
                return t;
            }
            return null;
        }
    }
}
