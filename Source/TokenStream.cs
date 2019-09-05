using System.Collections.Generic;
using System.Linq;

namespace CSharp2Crayon
{
    public class TokenStream
    {
        private class InternalStream
        {
            private Token[] tokens;
            public Token next = null;
            public int index;
            private int length;
            private bool isRegularMode = true; // as opposed to typed mode.

            public InternalStream(IList<Token> tokens)
            {
                this.tokens = tokens.ToArray();
                this.index = 0;
                this.length = this.tokens.Length;
            }

            public Token Peek()
            {
                if (this.next != null) return this.next;
                if (this.index >= this.length) return null;

                Token token = this.tokens[this.index];
                if (this.isRegularMode && token.Value == ">" && this.index + 1 < this.length)
                {
                    Token nextToken = this.tokens[this.index + 1];
                    if (nextToken.Value == ">" && nextToken.Column == token.Column + 1)
                    {
                        token = new Token(token.File, ">>", token.Column, token.Line);
                        token.Size = 2;
                    }
                }
                this.next = token;
                return this.next;
            }

            public Token Pop()
            {
                Token token = this.Peek();
                if (token != null)
                {
                    this.index += token.Size;
                }
                this.next = null;
                return token;
            }

            public void EnableTypedMode()
            {
                this.isRegularMode = false;
                this.next = null;
            }

            public void DisableTypedMode()
            {
                this.isRegularMode = true;
                this.next = null;
            }
        }

        private InternalStream stream;

        public TokenStream(string name, string content)
        {
            this.stream = new InternalStream(Tokenizer.Tokenize(name, content));
        }

        public int CurrentState { get { return this.stream.index; } }
        public void RestoreState(int value) { this.stream.index = value; this.stream.next = null; }

        public void SetTypeParsingMode(bool value)
        {
            if (value)
            {
                this.stream.EnableTypedMode();
            }
            else
            {
                this.stream.DisableTypedMode();
            }
        }

        public bool HasMore
        {
            get { return this.stream.Peek() != null; }
        }

        public Token Pop()
        {
            Token token = this.stream.Pop();
            if (token == null) throw new ParserException("Unexpected EOF");
            return token;
        }

        public string PeekValue()
        {
            Token token = this.stream.Peek();
            if (token == null) return null;
            return token.Value;
        }

        public Token Peek()
        {
            return this.stream.Peek();
        }

        public bool PopIfPresent(string value)
        {
            Token token = this.stream.Peek();
            if (token == null) return false;
            if (token.Value == value)
            {
                this.stream.Pop();
                return true;
            }
            return false;
        }

        public Token PopExpected(string value)
        {
            Token token = this.stream.Pop();
            if (token == null)
            {
                throw new ParserException("Unexpected EOF. Expected '" + value + "'");
            }

            if (token.Value != value)
            {
                throw new ParserException(token, "Expected '" + value + "' but found '" + token.Value + "'");
            }
            return token;
        }

        public bool IsNext(string value)
        {
            Token token = this.stream.Peek();
            if (token == null) return false;
            return token.Value == value;
        }

        public bool AreNext(params string[] values)
        {
            Token token = this.stream.Peek();
            if (token == null) return false;
            if (token.Value != values[0]) return false;
            int index = this.stream.index;
            this.stream.Pop();
            bool result = true;
            for (int i = 1; i < values.Length; ++i)
            {
                token = this.stream.Peek();
                if (token == null)
                {
                    result = false;
                    break;
                }
                this.stream.Pop();
            }
            this.stream.index = index;
            this.stream.next = null;
            return result;
        }

        public Token PopWord()
        {
            if (this.stream.Peek() == null) throw new ParserException("Unexpected EOF");
            Token word = this.PopWordIfPresent();
            if (word != null) return word;
            Token token = this.stream.Peek();
            throw new ParserException(token, "Expected an alphanumeric word but found '" + token.Value + "'");
        }

        public Token PopWordIfPresent()
        {
            Token token = this.stream.Peek();
            if (token == null) return null;
            char c = token.Value[0];
            if ((c >= 'a' && c <= 'z') ||
                (c >= 'A' && c <= 'Z') ||
                (c >= '0' && c <= '9') ||
                c == '_')
            {
                return this.stream.Pop();
            }
            return null;
        }
    }
}
