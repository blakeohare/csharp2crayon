using CSharp2Crayon.Parser.Nodes;
using System.Collections.Generic;

namespace CSharp2Crayon.Parser
{
    public static class TopLevelParser
    {
        public static TopLevelEntity Parse(ParserContext context, TokenStream tokens)
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

                case "interface":
                case "class":
                    return ParseClass(context, firstToken, modifiers, tokens);

                case "enum":
                    return ParseEnumDefinition(context, firstToken, modifiers, tokens);

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
                    case "virtual":
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

        public static ClassLikeDefinition ParseClass(ParserContext context, Token firstToken, Dictionary<string, Token> modifiers, TokenStream tokens)
        {
            if (!tokens.IsNext("class") && !tokens.IsNext("interface"))
            {
                tokens.PopExpected("class"); // intentionally throw
            }
            Token classToken = tokens.Pop();
            bool isInterface = classToken.Value == "interface";

            Token classNameToken = tokens.PopWord();
            List<CSharpType> subClassesAndSuch = new List<CSharpType>();
            if (tokens.PopIfPresent(":"))
            {
                while (!tokens.IsNext("{"))
                {
                    if (subClassesAndSuch.Count > 0)
                    {
                        tokens.PopExpected(",");
                    }
                    subClassesAndSuch.Add(CSharpType.Parse(tokens));
                }
            }

            tokens.PopExpected("{");
            ClassLikeDefinition cd;
            if (isInterface)
            {
                cd = new InterfaceDefinition(firstToken, modifiers, classToken, classNameToken, subClassesAndSuch);
            }
            else
            {
                cd = new ClassDefinition(firstToken, modifiers, classToken, classNameToken, subClassesAndSuch);
            }

            while (!tokens.PopIfPresent("}"))
            {
                TopLevelEntity classMember = ParseClassMember(cd, classNameToken, context, tokens);
                cd.AddMember(classMember);
            }
            return cd;
        }

        public static TopLevelEntity ParseClassMember(
            ClassLikeDefinition classDef,
            Token className,
            ParserContext context,
            TokenStream tokens)
        {
            Token firstToken = tokens.Peek();
            Dictionary<string, Token> modifiers = ParseModifiers(context, tokens);

            if (tokens.IsNext("enum"))
            {
                return ParseEnumDefinition(context, firstToken, modifiers, tokens);
            }

            if (tokens.IsNext("class"))
            {
                return ParseClass(context, firstToken, modifiers, tokens);
            }

            if (tokens.IsNext("const"))
            {
                return ParseConstDefinition(context, firstToken, modifiers, tokens);
            }

            CSharpType type = CSharpType.TryParse(tokens);

            if (tokens.IsNext("(") && type.SimpleTypeName == className.Value)
            {
                return ParseClassConstructor(context, firstToken, modifiers, type, tokens);
            }

            Token memberName = tokens.PopWord();

            if (tokens.IsNext(";") || tokens.IsNext("="))
            {
                return ParseClassField(context, firstToken, modifiers, type, memberName, tokens);
            }

            if (tokens.IsNext("[") && memberName.Value == "this")
            {
                return ParseIndexProperty(context, firstToken, modifiers, type, tokens);
            }

            if (tokens.IsNext("{"))
            {
                return ParseClassProperty(context, firstToken, modifiers, type, memberName, tokens);
            }

            if (tokens.IsNext("("))
            {
                return ParseClassMethod(classDef, context, firstToken, modifiers, type, memberName, tokens);
            }

            throw new ParserException(tokens.Peek(), "Not implemented");
        }

        private static TopLevelEntity ParseEnumDefinition(
            ParserContext context,
            Token firstToken,
            Dictionary<string, Token> modifiers,
            TokenStream tokens)
        {
            Token enumToken = tokens.PopExpected("enum");
            Token enumName = tokens.PopWord();
            List<Token> fieldNames = new List<Token>();
            List<Expression> fieldValues = new List<Expression>();
            tokens.PopExpected("{");
            bool nextAllowed = true;
            while (!tokens.PopIfPresent("}"))
            {
                if (!nextAllowed) tokens.PopExpected("}"); // intentionally throw
                Token enumFieldName = tokens.PopWord();
                Expression enumFieldValue = null;
                if (tokens.PopIfPresent("="))
                {
                    enumFieldValue = ExpressionParser.Parse(context, tokens);
                }
                fieldNames.Add(enumFieldName);
                fieldValues.Add(enumFieldValue);

                nextAllowed = tokens.PopIfPresent(",");
            }
            return new EnumDefinition(firstToken, modifiers, enumName, fieldNames, fieldValues);
        }

        private static TopLevelEntity ParseConstDefinition(
            ParserContext context,
            Token firstToken,
            Dictionary<string, Token> modifiers,
            TokenStream tokens)
        {
            Token constToken = tokens.PopExpected("const");
            CSharpType constType = CSharpType.Parse(tokens);
            Token name = tokens.PopWord();
            tokens.PopExpected("=");
            Expression value = ExpressionParser.Parse(context, tokens);
            tokens.PopExpected(";");

            return new ConstDefinition(firstToken, modifiers, constType, name, value);
        }

        private static TopLevelEntity ParseClassConstructor(
            ParserContext context,
            Token firstToken,
            Dictionary<string, Token> modifiers,
            CSharpType type,
            TokenStream tokens)
        {
            List<CSharpType> argTypes = new List<CSharpType>();
            List<Token> argNames = new List<Token>();
            List<Token> argModifiers = new List<Token>();
            ParseArgList(argTypes, argNames, argModifiers, tokens);

            Token baseConstructorInvocation = null;
            List<Expression> baseConstructorArgs = null;
            if (tokens.PopIfPresent(":"))
            {
                if (tokens.IsNext("base") || tokens.IsNext("this"))
                {
                    baseConstructorInvocation = tokens.Pop();
                }
                else
                {
                    tokens.PopExpected("base"); // intentionally throw
                }
                tokens.PopExpected("(");
                baseConstructorArgs = new List<Expression>();
                while (!tokens.PopIfPresent(")"))
                {
                    if (baseConstructorArgs.Count > 0) tokens.PopExpected(",");
                    baseConstructorArgs.Add(ExpressionParser.Parse(context, tokens));
                }
            }

            ConstructorDefinition constructorDef = new ConstructorDefinition(
                firstToken,
                modifiers,
                argTypes,
                argNames,
                argModifiers,
                baseConstructorInvocation,
                baseConstructorArgs);
            constructorDef.Code = ExecutableParser.ParseCodeBlock(context, tokens, true);

            return constructorDef;
        }

        private static TopLevelEntity ParseClassField(
            ParserContext context,
            Token firstToken,
            Dictionary<string, Token> modifiers,
            CSharpType type,
            Token fieldName,
            TokenStream tokens)
        {
            Expression initialValue = null;
            if (!tokens.PopIfPresent(";"))
            {
                tokens.PopExpected("=");
                initialValue = ExpressionParser.Parse(context, tokens);
                tokens.PopExpected(";");
            }
            return new FieldDefinition(firstToken, modifiers, type, fieldName, initialValue);
        }

        private static TopLevelEntity ParseClassProperty(
            ParserContext context,
            Token firstToken,
            Dictionary<string, Token> modifiers,
            CSharpType type,
            Token propertyName,
            TokenStream tokens)
        {
            tokens.PopExpected("{");

            PropertyBody getter = null;
            PropertyBody setter = null;

            while (!tokens.IsNext("}") && (getter == null || setter == null))
            {
                Token bodyFirstToken = tokens.Peek();
                Dictionary<string, Token> bodyModifiers = ParseModifiers(context, tokens);
                if (tokens.IsNext("get") && getter == null)
                {
                    Token getToken = tokens.Pop();
                    getter = new PropertyBody(bodyFirstToken, bodyModifiers, true);
                    if (!tokens.PopIfPresent(";")) getter.Code = ExecutableParser.ParseCodeBlock(context, tokens, true);
                }
                else if (tokens.IsNext("set") && setter == null)
                {
                    Token setToken = tokens.Pop();
                    setter = new PropertyBody(bodyFirstToken, bodyModifiers, false);
                    if (!tokens.PopIfPresent(";")) setter.Code = ExecutableParser.ParseCodeBlock(context, tokens, true);
                }
                else if (getter == null)
                {
                    tokens.PopExpected("get"); // intentionally throw
                }
                else
                {
                    tokens.PopExpected("set"); // intentionally throw
                }
            }
            tokens.PopExpected("}");

            return new PropertyDefinition(firstToken, modifiers, type, propertyName, getter, setter);
        }

        private static TopLevelEntity ParseClassMethod(
            ClassLikeDefinition classDef,
            ParserContext context,
            Token firstToken,
            Dictionary<string, Token> modifiers,
            CSharpType returnType,
            Token methodName,
            TokenStream tokens)
        {
            List<CSharpType> argTypes = new List<CSharpType>();
            List<Token> argNames = new List<Token>();
            List<Token> argModifiers = new List<Token>();
            ParseArgList(argTypes, argNames, argModifiers, tokens);

            MethodDefinition methodDef = new MethodDefinition(
                firstToken,
                modifiers,
                returnType,
                methodName,
                argNames,
                argTypes,
                argModifiers);

            if (methodDef.IsAbstract || classDef is InterfaceDefinition)
            {
                tokens.PopExpected(";");
            }
            else
            {
                methodDef.Code = ExecutableParser.ParseCodeBlock(context, tokens, true);
            }

            return methodDef;
        }
        private static TopLevelEntity ParseIndexProperty(
            ParserContext context,
            Token firstToken,
            Dictionary<string, Token> modifiers,
            CSharpType type,
            TokenStream tokens)
        {
            tokens.PopExpected("[");
            CSharpType indexType = CSharpType.Parse(tokens);
            Token indexVariableName = tokens.PopWord();
            tokens.PopExpected("]");
            tokens.PopExpected("{");

            PropertyBody getter = null;
            PropertyBody setter = null;

            while (tokens.IsNext("get") || tokens.IsNext("set"))
            {
                Token getOrSetToken = tokens.Peek();
                bool isGet = getOrSetToken.Value == "get";
                if (!isGet && setter != null) tokens.PopExpected("}"); // intentionally throw
                if (isGet && getter != null) tokens.PopExpected("set"); //intentionally throw
                tokens.Pop(); // get/set already fetched with Peek() above.
                Executable[] code = ExecutableParser.ParseCodeBlock(context, tokens, true);
                PropertyBody body = new PropertyBody(getOrSetToken, new Dictionary<string, Token>(), isGet);
                body.Code = code;
                if (isGet) getter = body;
                else setter = body;
            }

            PropertyDefinition indexProperty = new PropertyDefinition(firstToken, modifiers, type, null, getter, setter);
            indexProperty.IndexType = indexType;
            indexProperty.IndexVariableName = indexVariableName;
            return indexProperty;
        }

        private static void ParseArgList(List<CSharpType> typesOut, List<Token> namesOut, List<Token> modifiers, TokenStream tokens)
        {
            tokens.PopExpected("(");

            while (!tokens.IsNext(")"))
            {
                if (namesOut.Count > 0) tokens.PopExpected(",");
                if (tokens.IsNext("params") || tokens.IsNext("out"))
                {
                    modifiers.Add(tokens.Pop());
                }
                else
                {
                    modifiers.Add(null);
                }
                typesOut.Add(CSharpType.Parse(tokens));
                namesOut.Add(tokens.PopWord());
            }
            tokens.PopExpected(")");
        }
    }
}
