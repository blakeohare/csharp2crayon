using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharp2Crayon.Parser
{
    public class CSharpType
    {
        public Token FirstToken { get; private set; }
        public Token[] RootType { get; private set; }
        public CSharpType[] Generics { get; private set; }
        public bool IsArray { get { return this.RootType[0].Value == "["; } }
        public bool IsNullable { get { return this.RootType[0].Value == "?"; } }

        private string rootTypeString = null;
        public string RootTypeString
        {
            get
            {
                if (this.rootTypeString == null)
                {
                    this.rootTypeString = string.Join('.', this.RootType.Select(t => t.Value));
                }
                return this.rootTypeString;
            }
        }

        public static CSharpType Fabricate(IList<Token> root)
        {
            return new CSharpType(root, EMPTY_GENERICS);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            this.ToStringImpl(sb);
            return sb.ToString();
        }

        private void ToStringImpl(StringBuilder sb)
        {
            if (this.IsArray)
            {
                this.Generics[0].ToStringImpl(sb);
                sb.Append("[]");
            }
            else if (this.IsNullable)
            {
                this.Generics[0].ToStringImpl(sb);
                sb.Append('?');
            }
            else
            {
                for (int i = 0; i < this.RootType.Length; ++i)
                {
                    if (i > 0) sb.Append('.');
                    sb.Append(this.RootType[i]);
                }
                if (this.Generics.Length > 0)
                {
                    sb.Append('<');
                    for (int i = 0; i < this.Generics.Length; ++i)
                    {
                        if (i > 0) sb.Append(", ");
                        this.Generics[i].ToStringImpl(sb);
                    }
                    sb.Append('>');
                }
            }
        }

        public string SimpleTypeName
        {
            get
            {
                return this.RootType.Length == 1
                    ? this.RootType[0].Value
                    : string.Join(".", this.RootType.Select(token => token.Value));
            }
        }

        private CSharpType(IList<Token> rootType, IList<CSharpType> generics)
        {
            this.FirstToken = rootType[0];
            this.RootType = rootType.ToArray();
            this.Generics = generics.ToArray();
        }

        private static HashSet<string> PRIMITIVES_LOOKUP = new HashSet<string>() {
            "int", "double", "float", "string", "object",
            "byte", "char", "bool", "long",
        };

        private static readonly IList<CSharpType> EMPTY_GENERICS = new CSharpType[0];

        public static CSharpType Parse(TokenStream tokens)
        {
            CSharpType type = TryParse(tokens);
            if (type == null) throw new ParserException(tokens.Peek(), "Expected a type.");
            return type;
        }

        public static CSharpType ParseWithoutNullable(TokenStream tokens)
        {
            CSharpType type = TryParseWithoutNullable(tokens);
            if (type == null) throw new ParserException(tokens.Peek(), "Expected a type.");
            return type;
        }

        public static CSharpType TryParseWithoutNullable(TokenStream tokens)
        {
            tokens.SetTypeParsingMode(true);
            CSharpType type = TryParseImpl(tokens, false);
            tokens.SetTypeParsingMode(false);
            return type;
        }

        public static CSharpType TryParse(TokenStream tokens)
        {
            tokens.SetTypeParsingMode(true);
            CSharpType type = TryParseImpl(tokens, true);
            tokens.SetTypeParsingMode(false);
            return type;
        }

        private static CSharpType TryParseImpl(TokenStream tokens, bool allowNullable)
        {
            int tokenState = tokens.CurrentState;
            IList<Token> rootType = GetRootType(tokens);
            if (rootType == null)
            {
                tokens.RestoreState(tokenState);
                return null;
            }

            if (!PRIMITIVES_LOOKUP.Contains(rootType[0].Value))
            {
                int preGenerics = tokens.CurrentState;
                if (tokens.PopIfPresent("<"))
                {
                    bool isValid = true;
                    CSharpType first = TryParseImpl(tokens, allowNullable);
                    List<CSharpType> generics = new List<CSharpType>();
                    if (first != null)
                    {
                        generics.Add(first);

                        while (isValid && !tokens.IsNext(">"))
                        {
                            if (!tokens.PopIfPresent(","))
                            {
                                isValid = false;
                            }
                            else
                            {
                                CSharpType next = TryParseImpl(tokens, allowNullable);
                                if (next == null)
                                {
                                    isValid = false;
                                }

                                generics.Add(next);
                            }
                        }

                        if (isValid && tokens.PopIfPresent(">"))
                        {
                            CSharpType o = new CSharpType(rootType, generics);
                            if (tokens.IsNext("["))
                            {
                                o = ParseOutArraySuffix(o, tokens);
                            }
                            return o;
                        }
                    }
                }
                tokens.RestoreState(preGenerics);
            }

            CSharpType output = new CSharpType(rootType, EMPTY_GENERICS);
            if (allowNullable && tokens.IsNext("?"))
            {
                Token questionMark = tokens.Pop();
                output = new CSharpType(new Token[] { questionMark }, new CSharpType[] { output });
            }

            if (tokens.IsNext("["))
            {
                output = ParseOutArraySuffix(output, tokens);
            }
            return output;
        }

        private static CSharpType ParseOutArraySuffix(CSharpType current, TokenStream tokens)
        {
            while (tokens.AreNext("[", "]"))
            {
                Token openBracket = tokens.Pop();
                tokens.Pop();
                Token firstToken = current.FirstToken;
                current = new CSharpType(new Token[] { openBracket }, new CSharpType[] { current });
                current.FirstToken = firstToken;
            }
            return current;
        }

        private static IList<Token> GetRootType(TokenStream tokens)
        {
            Token first = tokens.PopWordIfPresent();
            if (first == null) return null;
            List<Token> parts = new List<Token>() { first };
            if (!PRIMITIVES_LOOKUP.Contains(first.Value))
            {
                while (tokens.IsNext("."))
                {
                    int index = tokens.CurrentState;
                    tokens.Pop();
                    Token next = tokens.PopWordIfPresent();
                    if (next == null)
                    {
                        tokens.RestoreState(index);
                        return parts;
                    }
                    parts.Add(next);
                }
            }
            return parts;

        }
    }
}
