using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharp2Crayon.Parser
{
    public class CSharpType
    {
        public Token FirstToken { get; private set; }
        public Token[] RootType { get; private set; }
        public CSharpType[] Generics { get; private set; }
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

        public static CSharpType TryParse(TokenStream tokens)
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
                    List<CSharpType> generics = new List<CSharpType>();
                    CSharpType first = TryParse(tokens);
                    if (first != null)
                    {
                        while (isValid && !tokens.IsNext(">"))
                        {
                            if (!tokens.PopIfPresent(","))
                            {
                                isValid = false;
                            }
                            else
                            {
                                CSharpType next = TryParse(tokens);
                                if (next == null)
                                {
                                    isValid = false;
                                }

                                generics.Add(next);
                            }
                        }

                        if (isValid && tokens.PopIfPresent(">"))
                        {
                            return new CSharpType(rootType, generics);
                        }
                    }
                }
                tokens.RestoreState(preGenerics);
            }

            return new CSharpType(rootType, EMPTY_GENERICS);
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
