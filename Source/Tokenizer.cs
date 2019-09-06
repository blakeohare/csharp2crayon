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
            // ">>", This is INTENTIONALLY omitted. The token stream will consolidate '>>' into a single token conditionally depending on whether or not a type is being parsed.
            // >>= as well.
            "<=",
            ">=",
            "=>",
        };

        private enum Mode
        {
            NORMAL,
            TOKEN,
            STRING,
            COMMENT,
        }

        public static IList<Token> Tokenize(string filename, string content, Dictionary<string, bool> preprocessorConstants)
        {
            content = Tokenizer.ApplyPreprocessorConstants(filename, content, preprocessorConstants);

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
                                    if (i + 1 < length)
                                    {
                                        c2 = content[i + 1];
                                        value += c2;
                                        if (MULTICHAR_TOKENS.Contains(value))
                                        {
                                            tokens.Add(new Token(filename, value, cols[i], lines[i]));
                                            ++i;
                                            break;
                                        }

                                        // floats that begin with a decimal
                                        if (c == '.' && c2 >= '0' && c2 <= '9')
                                        {
                                            // go ahead and advance past the decimal and treat it as a normal token.
                                            tokenStart = i;
                                            i++;
                                            mode = Mode.TOKEN;
                                            tokenBuilder = new StringBuilder();
                                            tokenBuilder.Append(value);
                                            break;
                                        }
                                    }
                                    tokens.Add(new Token(filename, c.ToString(), cols[i], lines[i]));
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
                        else if (c == '.' && (content[i - 1] >= '0' && content[i - 1] <= '9'))
                        {
                            // float. Treat it as a continuous token.
                            tokenBuilder.Append('.');
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

        private enum PreprocessToggleState
        {
            NONE,
            NOT_APPLIED,
            APPLY,
            DONE,
        }
        private static string ApplyPreprocessorConstants(string filename, string code, Dictionary<string, bool> constants)
        {
            string[] lines = code.Split('\n');
            PreprocessToggleState state = PreprocessToggleState.NONE;
            for (int i = 0; i < lines.Length; ++i)
            {
                string line = lines[i];
                if (line.Length > 0 && line[0] == '#')
                {
                    if (line.StartsWith("#pragma warning"))
                    {
                        line = "";
                    }
                    else if (line.StartsWith("#if ") || line.StartsWith("#elif "))
                    {
                        bool isElif = line.StartsWith("#elif");
                        bool isIf = !isElif;

                        if (isIf)
                        {
                            if (state != PreprocessToggleState.NONE)
                            {
                                throw new ParserException(filename + ": Unexpected #if on line " + (i + 1));
                            }
                        }
                        else
                        {
                            if (state == PreprocessToggleState.NONE)
                            {
                                throw new ParserException(filename + ": Unexpected #elif on line " + (i + 1));
                            }
                        }
                        if (state == PreprocessToggleState.APPLY)
                        {
                            state = PreprocessToggleState.DONE;
                        }
                        else if (state == PreprocessToggleState.DONE)
                        {
                            // ignore
                        }
                        else
                        {
                            string constantName = line.Substring(line.IndexOf(' ') + 1).Trim();
                            bool value = constants.ContainsKey(constantName) && constants[constantName];
                            if (value)
                            {
                                state = PreprocessToggleState.APPLY;
                            }
                            else
                            {
                                state = PreprocessToggleState.NOT_APPLIED;
                            }
                        }
                    }
                    else if (line.StartsWith("#else"))
                    {
                        if (state == PreprocessToggleState.NOT_APPLIED)
                        {
                            state = PreprocessToggleState.APPLY;
                        }
                        else if (state == PreprocessToggleState.APPLY)
                        {
                            state = PreprocessToggleState.DONE;
                        }
                    }
                    else if (line.StartsWith("#endif"))
                    {
                        state = PreprocessToggleState.NONE;
                    }
                    else
                    {
                        throw new ParserException(filename + ": Unexpected # directive on line " + (i + 1));
                    }
                    line = "";
                }
                else
                {
                    switch (state)
                    {
                        case PreprocessToggleState.NONE:
                        case PreprocessToggleState.APPLY:
                            break;

                        // If the condition doesn't apply or has already been applied, ignore the line
                        case PreprocessToggleState.DONE:
                        case PreprocessToggleState.NOT_APPLIED:
                            line = "";
                            break;
                    }
                }
                lines[i] = line;
            }

            return string.Join('\n', lines);
        }
    }
}
