using System.Collections.Generic;
using System.Text;

namespace CSharp2Crayon
{
    public static class Tokenizer
    {
        private static HashSet<string> MULTICHAR_TOKENS = new HashSet<string>() {
            "++",
            "--",
            "+=",
            "-=",
            "*=",
            "/=",
            "%=",
            "|=",
            "&=",
            "^=",
            "&&",
            "||",
            "??",
            "==",
            "!=",
            "<<",
            ">>",
            "<=",
            ">=",
        };

        private enum Mode
        {
            NORMAL,
            TOKEN,
            STRING,
            COMMENT,
        }

        public static IList<Token> Tokenize(string filename, string content)
        {
            content += " ";
            int[] lines = new int[content.Length];
            int[] cols = new int[content.Length];
            int line = 1;
            int col = 1;
            for (int i = 0; i < content.Length; ++i)
            {
                lines[i] = line;
                cols[i] = col;
                if (content[i] == '\n')
                {
                    col = 1;
                    line++;
                }
                else
                {
                    col++;
                }
            }

            Mode mode = Mode.NORMAL;
            char commentType = ' ';
            char stringType = '"';
            StringBuilder tokenBuilder = null;
            int tokenStart = 0;
            char c, c2;
            int length = content.Length;
            List<Token> tokens = new List<Token>();

            for (int i = 0; i < length; ++i)
            {
                c = content[i];
                switch (mode)
                {
                    case Mode.NORMAL:
                        switch (c)
                        {
                            case ' ':
                            case '\t':
                            case '\n':
                            case '\r':
                                // whitespace, do nothing
                                break;

                            case '/':
                                c2 = content[i + 1];
                                if (c2 == '*' || c2 == '/')
                                {
                                    mode = Mode.COMMENT;
                                    commentType = c2;
                                    ++i;
                                }
                                else
                                {
                                    tokens.Add(new Token(filename, "/", cols[i], lines[i]));
                                }
                                break;

                            case '"':
                            case '\'':
                                mode = Mode.STRING;
                                stringType = c;
                                tokenBuilder = new StringBuilder();
                                tokenBuilder.Append(c);
                                tokenStart = i;
                                break;

                            default:
                                if ((c >= 'a' && c <= 'z') ||
                                    (c >= 'A' && c <= 'Z') ||
                                    (c >= '0' && c <= '9') ||
                                    c == '_')
                                {
                                    tokenStart = i;
                                    mode = Mode.TOKEN;
                                    tokenBuilder = new StringBuilder();
                                    --i;
                                }
                                else
                                {
                                    string value = c.ToString();
                                    bool isMultiChar = false;
                                    if (i + 1 < length)
                                    {
                                        value += content[i + 1];
                                        if (MULTICHAR_TOKENS.Contains(value))
                                        {
                                            tokens.Add(new Token(filename, value, cols[i], lines[i]));
                                            ++i;
                                            isMultiChar = true;
                                        }
                                    }
                                    if (!isMultiChar)
                                    {
                                        tokens.Add(new Token(filename, c.ToString(), cols[i], lines[i]));
                                    }
                                }
                                break;
                        }
                        break;

                    case Mode.COMMENT:
                        if (commentType == '*' && c == '*' && content[i + 1] == '/')
                        {
                            mode = Mode.NORMAL;
                            ++i;
                        }
                        else if (commentType == '/' && c == '\n')
                        {
                            mode = Mode.NORMAL;
                        }
                        break;

                    case Mode.STRING:
                        if (c == '\\')
                        {
                            tokenBuilder.Append('\\');
                            tokenBuilder.Append(content[i + 1]);
                            ++i;
                        }
                        else if (c == stringType)
                        {
                            tokenBuilder.Append(c);
                            tokens.Add(new Token(filename, tokenBuilder.ToString(), cols[tokenStart], lines[tokenStart]));
                            mode = Mode.NORMAL;
                            tokenBuilder = null;
                        }
                        else
                        {
                            tokenBuilder.Append(c);
                        }
                        break;

                    case Mode.TOKEN:
                        if ((c >= 'a' && c <= 'z') ||
                            (c >= 'A' && c <= 'Z') ||
                            (c >= '0' && c <= '9') ||
                            c == '_')
                        {
                            tokenBuilder.Append(c);
                        }
                        else
                        {
                            tokens.Add(new Token(filename, tokenBuilder.ToString(), cols[tokenStart], lines[tokenStart]));
                            mode = Mode.NORMAL;
                            --i;
                            tokenBuilder = null;
                        }
                        break;
                }
            }

            if (mode != Mode.NORMAL)
            {
                throw new ParserException("Unexpected EOF in " + filename);
            }

            return tokens;
        }
    }
}
