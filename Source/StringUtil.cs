using System.Text;

namespace CSharp2Crayon
{
    public static class StringUtil
    {
        public static string ConvertStringTokenToValue(Token token)
        {
            string value = token.Value;
            int length = value.Length - 1;
            StringBuilder sb = new StringBuilder();
            char c;
            for (int i = 1; i < length; ++i)
            {
                c = value[i];
                if (c == '\\')
                {
                    // Don't worry about catching backslashes at the end of the string. The tokenizer prevents that from happening.
                    c = value[++i];
                    switch (c)
                    {
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case '\\': sb.Append('\\'); break;
                        case 't': sb.Append('\t'); break;
                        case '"': sb.Append('"'); break;
                        case '\'': sb.Append("'"); break;
                        case '0': sb.Append('\0'); break;
                        case 'b': sb.Append('\b'); break;
                        default:
                            throw new ParserException(token, "Invalid escape sequence: \\" + c);
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }
}
