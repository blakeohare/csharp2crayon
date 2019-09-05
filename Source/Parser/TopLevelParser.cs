using CSharp2Crayon.Parser.Nodes;
using System;
using System.Collections.Generic;

namespace CSharp2Crayon.Parser
{
    public static class TopLevelParser
    {
        public static Nodes.TopLevelEntity Parse(ParserContext context, TokenStream tokens)
        {
            if (!tokens.HasMore)
            {
                throw new ParserException("Unexpected EOF");
            }
            Token firstToken = tokens.Peek();
            Dictionary<string, Token> modifiers = ParseModifiers(context, tokens);
            string next = tokens.PeekValue() ?? "";
            switch (next)
            {
                case "using":
                    EnsureModifiersEmpty(modifiers, firstToken);
                    return ParseUsing(context, tokens);
                case "namespace":
                    EnsureModifiersEmpty(modifiers, firstToken);
                    return ParseNamespace(context, tokens);

                case "class":
                    return ParseClass(context, firstToken, modifiers, tokens);

                default:
                    throw new ParserException(tokens.Peek(), "Unexpected token: '" + next + "'");
            }
        }

        private static void EnsureModifiersEmpty(Dictionary<string, Token> modifiers, Token firstModifier)
        {
            if (modifiers.Count > 0)
            {
                throw new ParserException(firstModifier, "Unexpected token: '" + firstModifier.Value + "'");
            }
        }
        private static readonly Dictionary<string, Token> EMPTY_DICTIONARY = new Dictionary<string, Token>();

        private static Dictionary<string, Token> ParseModifiers(ParserContext context, TokenStream tokens)
        {
            List<Token> modifiers = null;
            bool keepGoing = true;
            while (tokens.HasMore && keepGoing)
            {
                switch (tokens.PeekValue() ?? "")
                {
                    case "public":
                    case "private":
                    case "protected":
                    case "static":
                    case "readonly":
                    case "internal":
                    case "abstract":
                    case "override":
                        if (modifiers == null) modifiers = new List<Token>();
                        modifiers.Add(tokens.Pop());
                        break;

                    default:
                        keepGoing = false;
                        break;

                }
            }

            if (modifiers == null) return EMPTY_DICTIONARY;
            Dictionary<string, Token> output = new Dictionary<string, Token>();
            foreach (Token modifier in modifiers)
            {
                output[modifier.Value] = modifier;
            }
            return output;
        }

        public static UsingDirective ParseUsing(ParserContext context, TokenStream tokens)
        {
            Token usingToken = tokens.PopExpected("using");
            List<Token> parts = new List<Token>();
            parts.Add(tokens.PopWord());
            while (tokens.IsNext("."))
            {
                tokens.PopExpected(".");
                parts.Add(tokens.PopWord());
            }
            tokens.PopExpected(";");
            return new UsingDirective(usingToken, parts);
        }

        public static Namespace ParseNamespace(ParserContext context, TokenStream tokens)
        {
            Token namespaceToken = tokens.PopExpected("namespace");
            List<Token> nsNameParts = new List<Token>();
            nsNameParts.Add(tokens.PopWord());
            while (tokens.IsNext("."))
            {
                tokens.PopExpected(".");
                nsNameParts.Add(tokens.PopWord());
            }
            Namespace ns = new Namespace(namespaceToken, nsNameParts);
            tokens.PopExpected("{");
            while (!tokens.PopIfPresent("}"))
            {
                TopLevelEntity tle = Parse(context, tokens);
                ns.AddMember(tle);
            }
            return ns;
        }

        public static ClassDefinition ParseClass(ParserContext context, Token firstToken, Dictionary<string, Token> modifiers, TokenStream tokens)
        {
            Token classToken = tokens.PopExpected("class");
            Token classNameToken = tokens.PopWord();
            List<Token[]> subClassesAndSuch = new List<Token[]>();
            if (tokens.PopIfPresent(":"))
            {
                while (!tokens.IsNext("{"))
                {
                    if (subClassesAndSuch.Count > 0)
                    {
                        tokens.PopExpected(",");
                    }
                    List<Token> classNameBuilder = new List<Token>();
                    classNameBuilder.Add(tokens.PopWord());
                    while (tokens.PopIfPresent("."))
                    {
                        classNameBuilder.Add(tokens.PopWord());
                    }
                    subClassesAndSuch.Add(classNameBuilder.ToArray());
                }

            }

            tokens.PopExpected("{");
            ClassDefinition cd = new ClassDefinition(firstToken, modifiers, classToken, classNameToken, subClassesAndSuch);
            while (!tokens.PopIfPresent("}"))
            {
                TopLevelEntity classMember = ParseClassMember(cd, context, tokens);
                cd.AddMember(classMember);
            }
            return cd;
        }

        public static TopLevelEntity ParseClassMember(ClassDefinition classDef, ParserContext context, TokenStream tokens)
        {
            Token firstToken = tokens.Peek();
            Dictionary<string, Token> modifiers = ParseModifiers(context, tokens);

            CSharpType type = CSharpType.TryParse(tokens);

            if (tokens.IsNext("(") && type.SimpleTypeName == classDef.Name.Value)
            {
                return ParseClassConstructor(classDef, firstToken, modifiers, type, tokens);
            }

            Token memberName = tokens.PopWord();

            if (tokens.IsNext(";") || tokens.IsNext("="))
            {
                return ParseClassField(classDef, firstToken, modifiers, type, memberName);
            }

            if (tokens.IsNext("{"))
            {
                return ParseClassProperty(classDef, firstToken, modifiers, type, memberName);
            }

            if (tokens.IsNext("("))
            {
                return ParseClassMethod(classDef, firstToken, modifiers, type, memberName);
            }

            throw new NotImplementedException();
        }

        private static TopLevelEntity ParseClassConstructor(ClassDefinition classDef, Token firstToken, Dictionary<string, Token> modifiers, CSharpType type, TokenStream tokens)
        {
            List<CSharpType> argTypes = new List<CSharpType>();
            List<Token> argNames = new List<Token>();
            ParseArgList(argTypes, argNames, tokens);
            throw new NotImplementedException();
        }

        private static TopLevelEntity ParseClassField(ClassDefinition classDef, Token firstToken, Dictionary<string, Token> modifiers, CSharpType type, Token fieldName)
        {
            throw new NotImplementedException();
        }

        private static TopLevelEntity ParseClassProperty(ClassDefinition classDef, Token firstToken, Dictionary<string, Token> modifiers, CSharpType type, Token propertyName)
        {
            throw new NotImplementedException();
        }

        private static TopLevelEntity ParseClassMethod(ClassDefinition classDef, Token firstToken, Dictionary<string, Token> modifiers, CSharpType type, Token methodName)
        {
            throw new NotImplementedException();
        }

        private static void ParseArgList(List<CSharpType> typesOut, List<Token> namesOut, TokenStream tokens)
        {
            tokens.PopExpected("(");
            if (tokens.PopIfPresent(")")) return;
            typesOut.Add(CSharpType.Parse(tokens));
            namesOut.Add(tokens.PopWord());
            while (tokens.PopIfPresent(","))
            {
                typesOut.Add(CSharpType.Parse(tokens));
                namesOut.Add(tokens.PopWord());
            }
            tokens.PopExpected(")");
        }
    }
}
