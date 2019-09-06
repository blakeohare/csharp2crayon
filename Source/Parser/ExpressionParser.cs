using CSharp2Crayon.Parser.Nodes;
using System;
using System.Collections.Generic;

namespace CSharp2Crayon.Parser
{
    public static class ExpressionParser
    {
        public static Expression Parse(ParserContext context, TokenStream tokens)
        {
            return ParseTernary(context, tokens);
        }

        private static Expression ParseTernary(ParserContext context, TokenStream tokens)
        {
            Expression root = ParseNullCoalescer(context, tokens);
            if (tokens.IsNext("?"))
            {
                Token ternaryToken = tokens.Pop();
                Expression trueExpression = ParseTernary(context, tokens);
                tokens.PopExpected(":");
                Expression falseExpression = ParseTernary(context, tokens);
                return new TernaryExpression(root, ternaryToken, trueExpression, falseExpression);
            }
            return root;
        }

        private static Expression ParseNullCoalescer(ParserContext context, TokenStream tokens)
        {
            Expression root = ParseBooleanCombination(context, tokens);
            if (tokens.IsNext("??"))
            {
                List<Expression> expressions = new List<Expression>() { root };
                List<Token> ops = new List<Token>();
                while (tokens.IsNext("??"))
                {
                    ops.Add(tokens.Pop());
                    expressions.Add(ParseBooleanCombination(context, tokens));
                }
                return new OpChain(expressions, ops);
            }
            return root;
        }

        private static Expression ParseBooleanCombination(ParserContext context, TokenStream tokens)
        {
            Expression root = ParseBitwiseOps(context, tokens);
            string next = tokens.PeekValue();
            if (next == "&&" || next == "||")
            {
                List<Expression> expressions = new List<Expression>() { root };
                List<Token> ops = new List<Token>();
                while (tokens.IsNext("&&") || tokens.IsNext("||"))
                {
                    ops.Add(tokens.Pop());
                    expressions.Add(ParseBitwiseOps(context, tokens));
                }
                return new OpChain(expressions, ops);
            }
            return root;
        }

        private static Expression ParseBitwiseOps(ParserContext context, TokenStream tokens)
        {
            Expression root = ParseEqualityComparison(context, tokens);
            string next = tokens.PeekValue();
            if (next == "|" || next == "&" || next == "^")
            {
                List<Expression> expressions = new List<Expression>() { root };
                List<Token> ops = new List<Token>();
                while (tokens.IsNext("|") || tokens.IsNext("&") || tokens.IsNext("^"))
                {
                    ops.Add(tokens.Pop());
                    expressions.Add(ParseEqualityComparison(context, tokens));
                }
                return new OpChain(expressions, ops);
            }
            return root;
        }

        private static Expression ParseEqualityComparison(ParserContext context, TokenStream tokens)
        {
            Expression root = ParseInequalityComparison(context, tokens);
            string next = tokens.PeekValue();
            if (next == "==" || next == "!=")
            {
                List<Token> ops = new List<Token>() { tokens.Pop() };
                List<Expression> expressions = new List<Expression>() { root, ParseInequalityComparison(context, tokens) };
                return new OpChain(expressions, ops);
            }
            return root;
        }

        private static Expression ParseInequalityComparison(ParserContext context, TokenStream tokens)
        {
            Expression root = ParseBitShift(context, tokens);
            string next = tokens.PeekValue();
            if (next == "<" || next == "<=" || next == ">" || next == ">=")
            {
                List<Token> ops = new List<Token>() { tokens.Pop() };
                List<Expression> expressions = new List<Expression>() { root, ParseBitShift(context, tokens) };
                return new OpChain(expressions, ops);
            }
            if (tokens.IsNext("is") || tokens.IsNext("as"))
            {
                Token isToken = tokens.Pop();
                CSharpType type = CSharpType.Parse(tokens);
                if (isToken.Value == "is")
                {
                    return new IsComparison(root.FirstToken, root, isToken, type);
                }
                else
                {
                    return new AsCasting(root.FirstToken, root, isToken, type);
                }
            }
            return root;
        }

        private static Expression ParseBitShift(ParserContext context, TokenStream tokens)
        {
            Expression root = ParseAddition(context, tokens);
            string next = tokens.PeekValue();
            if (next == "<<" || next == ">>")
            {
                List<Expression> expressions = new List<Expression>() { root };
                List<Token> ops = new List<Token>();
                while (tokens.IsNext("<<") || tokens.IsNext(">>"))
                {
                    ops.Add(tokens.Pop());
                    expressions.Add(ParseAddition(context, tokens));
                }
                return new OpChain(expressions, ops);
            }
            return root;
        }

        private static Expression ParseAddition(ParserContext context, TokenStream tokens)
        {
            Expression root = ParseMultiplication(context, tokens);
            string next = tokens.PeekValue();
            if (next == "+" || next == "-")
            {
                List<Expression> expressions = new List<Expression>() { root };
                List<Token> ops = new List<Token>();
                while (tokens.IsNext("+") || tokens.IsNext("-"))
                {
                    ops.Add(tokens.Pop());
                    expressions.Add(ParseMultiplication(context, tokens));
                }
                return new OpChain(expressions, ops);
            }
            return root;
        }

        private static Expression ParseMultiplication(ParserContext context, TokenStream tokens)
        {
            Expression root = ParseUnary(context, tokens);
            string next = tokens.PeekValue();
            if (next == "*" || next == "/" || next == "%")
            {
                List<Expression> expressions = new List<Expression>() { root };
                List<Token> ops = new List<Token>();
                while (tokens.IsNext("*") || tokens.IsNext("/") || tokens.IsNext("%"))
                {
                    ops.Add(tokens.Pop());
                    expressions.Add(ParseUnary(context, tokens));
                }
                return new OpChain(expressions, ops);
            }
            return root;
        }

        private static Expression ParseUnary(ParserContext context, TokenStream tokens)
        {
            string next = tokens.PeekValue();
            if (next == "!" || next == "-" || next == "--" || next == "++")
            {
                Token unaryToken = tokens.Pop();
                Expression root = ParseAtomWithSuffix(context, tokens);
                switch (next)
                {
                    case "!": return new BooleanNot(unaryToken, root);
                    case "-": return new NegativeSign(unaryToken, root);

                    case "++":
                    case "--":
                        return new InlineIncrement(unaryToken, unaryToken, true, root);
                    default: throw new NotImplementedException();
                }
            }

            return ParseAtomWithSuffix(context, tokens);
        }

        private static CSharpType[] MaybeParseOutOneOfThoseInlineTypeSpecificationsForFunctionInvocations(TokenStream tokens)
        {
            int state = tokens.CurrentState;
            if (!tokens.PopIfPresent("<")) return null;

            List<CSharpType> types = new List<CSharpType>();
            CSharpType type = CSharpType.TryParse(tokens);
            if (type == null)
            {
                tokens.RestoreState(state);
                return null;
            }
            types.Add(type);
            while (tokens.PopIfPresent(","))
            {
                type = CSharpType.TryParse(tokens);
                if (type == null)
                {
                    tokens.RestoreState(state);
                    return null;
                }
                types.Add(type);
            }
            if (tokens.PopIfPresent(">") && tokens.IsNext("("))
            {
                return types.ToArray();
            }
            tokens.RestoreState(state);
            return null;
        }

        private enum ParenthesisSituation
        {
            CAST,
            LAMBDA_ARG,
            WRAPPED_EXPRESSION,
        }

        private static ParenthesisSituation IdentifyParenthesisSituation(TokenStream tokens)
        {
            int currentState = tokens.CurrentState;
            ParenthesisSituation output = IdentifyParenthesisSituationImpl(tokens);
            tokens.RestoreState(currentState);
            return output;
        }

        // this function is called after the first ( is popped off the token stream.
        // It is destructive to the token stream.
        private static ParenthesisSituation IdentifyParenthesisSituationImpl(TokenStream tokens)
        {
            int state = tokens.CurrentState;

            // if any of these cause an EOF exception, it's a legit EOF exception.
            // The }'s alone for any body of code that an expression can appear in would be enough padding
            // to avoid non-legit errors.
            Token t1 = tokens.Pop();
            Token t2 = tokens.Pop();
            Token t3 = tokens.Pop();

            if (!t1.IsIdentifier) return ParenthesisSituation.WRAPPED_EXPRESSION;
            if (t2.Value == ",") return ParenthesisSituation.LAMBDA_ARG;
            if (t2.Value == ")" && t3.Value == "=>") return ParenthesisSituation.LAMBDA_ARG;

            if (t2.Value == ")")
            {
                switch (t1.Value)
                {
                    case "string":
                    case "int":
                    case "float":
                    case "double":
                    case "bool":
                    case "object":
                    case "byte":
                    case "char":
                    case "long":
                        return ParenthesisSituation.CAST;
                }
            }

            tokens.RestoreState(state);

            CSharpType type = CSharpType.TryParse(tokens);
            if (type == null || !tokens.IsNext(")")) return ParenthesisSituation.WRAPPED_EXPRESSION;
            if (type.Generics.Length > 0) return ParenthesisSituation.CAST;
            if (!tokens.PopIfPresent(")")) return ParenthesisSituation.WRAPPED_EXPRESSION;
            if (!tokens.HasMore) return ParenthesisSituation.WRAPPED_EXPRESSION;
            // At this point you have a sequence words and dots in parentheses.
            Token next = tokens.Peek();
            if (next.IsIdentifier || next.IsNumber) return ParenthesisSituation.CAST;
            char c = next.Value[0];
            if (c == '(') return ParenthesisSituation.CAST;
            if (c == '@') return ParenthesisSituation.CAST;
            if (c == '.') return ParenthesisSituation.WRAPPED_EXPRESSION;
            if (c == '!') return ParenthesisSituation.CAST;
            return ParenthesisSituation.WRAPPED_EXPRESSION;
        }

        private enum ConstructorSuffixData
        {
            SEQUENTIAL_ITEMS,
            KVP_ENTRIES,
            PROPERTIES,
        }

        // called after the { is popped
        private static ConstructorSuffixData DetermineConstructorSuffixDataFormat(TokenStream tokens)
        {
            if (!tokens.HasMore) return ConstructorSuffixData.SEQUENTIAL_ITEMS;
            if (tokens.IsNext("{")) return ConstructorSuffixData.KVP_ENTRIES;
            int state = tokens.CurrentState;
            Token nextToken = tokens.Pop();
            Token skipToken = tokens.Peek();
            tokens.RestoreState(state);
            if (skipToken == null) return ConstructorSuffixData.SEQUENTIAL_ITEMS;
            if (nextToken.IsIdentifier && skipToken.Value == "=") return ConstructorSuffixData.PROPERTIES;
            return ConstructorSuffixData.SEQUENTIAL_ITEMS;
        }

        private static Expression ParseAtomWithSuffix(ParserContext context, TokenStream tokens)
        {
            Expression root;
            if (tokens.IsNext("("))
            {
                Token openParen = tokens.Pop();

                switch (IdentifyParenthesisSituation(tokens))
                {
                    case ParenthesisSituation.CAST:
                        CSharpType castType = CSharpType.Parse(tokens);
                        tokens.PopExpected(")");
                        Expression castValue = ParseAtomWithSuffix(context, tokens);
                        return new CastExpression(openParen, castType, castValue);

                    case ParenthesisSituation.LAMBDA_ARG:
                        List<Token> lambdaArgs = new List<Token>() { tokens.PopWord() };

                        while (tokens.PopIfPresent(","))
                        {
                            lambdaArgs.Add(tokens.PopWord());
                        }
                        tokens.PopExpected(")");
                        Token arrowToken = tokens.PopExpected("=>");
                        Executable[] lambdaBody;
                        if (tokens.IsNext("{"))
                        {
                            lambdaBody = ExecutableParser.ParseCodeBlock(context, tokens, true);
                        }
                        else
                        {
                            Expression expr = Parse(context, tokens);
                            lambdaBody = new Executable[] {
                                new ReturnStatement(expr.FirstToken, expr)
                            };
                        }
                        return new Lambda(openParen, lambdaArgs, arrowToken, lambdaBody);

                    case ParenthesisSituation.WRAPPED_EXPRESSION:
                        root = Parse(context, tokens);
                        tokens.PopExpected(")");
                        break;

                    default:
                        throw new Exception(); // not valid
                }
            }
            else
            {
                root = ParseAtom(context, tokens);
            }
            bool anythingInteresting = true;
            string next = tokens.PeekValue();
            while (anythingInteresting)
            {
                switch (next)
                {
                    case ".":
                        Token dotToken = tokens.Pop();
                        Token fieldName = tokens.PopWord();
                        root = new DotField(root.FirstToken, root, dotToken, fieldName);
                        break;
                    case "[":
                        Token openBracket = tokens.Pop();
                        Expression index = Parse(context, tokens);
                        tokens.PopExpected("]");
                        root = new BracketIndex(root, openBracket, index);
                        break;
                    case "<":
                        if (root is DotField)
                        {
                            CSharpType[] functionInvocationTypeSpecification = MaybeParseOutOneOfThoseInlineTypeSpecificationsForFunctionInvocations(tokens);
                            if (functionInvocationTypeSpecification != null)
                            {
                                ((DotField)root).InlineTypeSpecification = functionInvocationTypeSpecification;
                            }
                            else
                            {
                                anythingInteresting = false;
                            }
                        }
                        else
                        {
                            anythingInteresting = false;
                        }
                        break;
                    case "(":
                        Token openParen = tokens.Pop();
                        List<Expression> args = new List<Expression>();
                        List<Token> outTokens = new List<Token>();
                        while (!tokens.PopIfPresent(")"))
                        {
                            if (args.Count > 0) tokens.PopExpected(",");
                            if (tokens.IsNext("out"))
                            {
                                outTokens.Add(tokens.Pop());
                            }
                            else
                            {
                                outTokens.Add(null);
                            }
                            args.Add(Parse(context, tokens));
                        }
                        root = new FunctionInvocation(root.FirstToken, root, openParen, args, outTokens);
                        break;
                    case "{": // e.g. new List<int>() { 1, 2, 3 }. This only follows a constructor.
                        FunctionInvocation fi = root as FunctionInvocation;
                        ConstructorInvocationFragment cif = fi == null ? null : (ConstructorInvocationFragment)fi.Root;
                        if (root is FunctionInvocation && ((FunctionInvocation)root).Root is ConstructorInvocationFragment)
                        {
                            tokens.Pop(); // {

                            ConstructorSuffixData format = DetermineConstructorSuffixDataFormat(tokens);

                            bool nextAllowed = true;
                            List<Expression> values = new List<Expression>();
                            List<Expression> kvpKeys = new List<Expression>();
                            List<Token> propertyNames = new List<Token>();
                            while (!tokens.PopIfPresent("}"))
                            {
                                if (!nextAllowed) tokens.PopExpected("}"); // intentionally throw
                                switch (format)
                                {
                                    case ConstructorSuffixData.KVP_ENTRIES:
                                        tokens.PopExpected("{");
                                        kvpKeys.Add(Parse(context, tokens));
                                        tokens.PopExpected(",");
                                        values.Add(Parse(context, tokens));
                                        tokens.PopExpected("}");
                                        break;

                                    case ConstructorSuffixData.PROPERTIES:
                                        propertyNames.Add(tokens.PopWord());
                                        tokens.PopExpected("=");
                                        values.Add(Parse(context, tokens));
                                        break;

                                    case ConstructorSuffixData.SEQUENTIAL_ITEMS:
                                        values.Add(Parse(context, tokens));
                                        break;
                                }
                                nextAllowed = tokens.PopIfPresent(",");
                            }
                            cif.InitialDataValues = values.ToArray();
                            int t = cif.InitialDataValues.Length;
                            if (kvpKeys.Count == t) cif.InitialDataKeys = kvpKeys.ToArray();
                            if (propertyNames.Count == t) cif.InitialDataPropertyNames = propertyNames.ToArray();
                        }
                        else
                        {
                            throw new ParserException(tokens.Peek(), "Unexpected '{'");
                        }
                        break;
                    default:
                        anythingInteresting = false;
                        break;
                }
                next = tokens.PeekValue();
            }

            if (next == "++" || next == "--")
            {
                Token incrementToken = tokens.Pop();
                root = new InlineIncrement(root.FirstToken, incrementToken, false, root);
            }

            return root;
        }

        private static Expression ParseAtom(ParserContext context, TokenStream tokens)
        {
            string next = tokens.PeekValue();
            switch (next)
            {
                case "true":
                case "false":
                    Token booleanToken = tokens.Pop();
                    return new BooleanConstant(booleanToken, booleanToken.Value == "true");

                case "null":
                    Token nullToken = tokens.Pop();
                    return new NullConstant(nullToken);

                case "new":
                    Token newToken = tokens.Pop();
                    CSharpType className = CSharpType.TryParse(tokens);
                    if (className == null)
                    {
                        throw new ParserException(newToken, "'new' keyword must be followed by a className");
                    }
                    if (!tokens.IsNext("("))
                    {
                        // could be a new array of which there are two ways to define it...
                        if (className.IsArray && tokens.IsNext("{"))
                        {
                            List<Expression> arrayItems = new List<Expression>();
                            tokens.PopExpected("{");
                            bool nextAllowed = true;
                            while (!tokens.PopIfPresent("}"))
                            {
                                if (!nextAllowed) tokens.PopExpected("}");
                                arrayItems.Add(ExpressionParser.Parse(context, tokens));
                                nextAllowed = tokens.PopIfPresent(",");
                            }
                            return new ArrayInitialization(newToken, className.Generics[0], null, arrayItems);
                        }

                        if (tokens.IsNext("["))
                        {
                            // a new array with specified length
                            tokens.PopExpected("[");
                            Expression arrayLength = ExpressionParser.Parse(context, tokens);
                            tokens.PopExpected("]");
                            return new ArrayInitialization(newToken, className, arrayLength, null);
                        }

                        // if this isn't an array construction, then give a reasonable error message...
                        tokens.PopExpected("(");
                    }
                    return new ConstructorInvocationFragment(newToken, className);

                case "@":
                    // raw string
                    Token rawStringAt = tokens.Pop();
                    Token stringValue = tokens.Pop();
                    string stringValueActual = stringValue.Value;
                    if (stringValueActual[0] != '"')
                    {
                        throw new ParserException(stringValue, "Expected a string value");
                    }
                    return new StringConstant(rawStringAt, stringValueActual.Substring(1, stringValueActual.Length - 2));

                default: break;
            }

            Token token = tokens.Pop();
            char c = next[0];

            if (c == '"' || c == '\'')
            {
                return new StringConstant(token, StringUtil.ConvertStringTokenToValue(token));
            }

            if (c == '0' && next.Length > 2 && next[1] == 'x')
            {
                string hex = next.Substring(2);
                int parsedValue = 0;

                for (int i = 0; i < hex.Length; ++i)
                {
                    c = hex[i];
                    if (c >= '0' && c <= '9')
                    {
                        parsedValue = parsedValue * 16 + (c - '0');
                    }
                    else if (c >= 'a' && c <= 'f')
                    {
                        parsedValue = parsedValue * 16 + (10 + c - 'a');
                    }
                    else if (c >= 'A' && c <= 'F')
                    {
                        parsedValue = parsedValue * 16 + (10 + c - 'A');
                    }
                    else
                    {
                        throw new ParserException(token, "Invalid hexidecimal value: '" + hex + "'");
                    }
                }
                return new IntegerConstant(token, parsedValue);
            }

            if (c == '.' && next.Length > 1)
            {
                throw new NotImplementedException(); // parse float
            }

            if (c >= '0' && c <= '9')
            {
                if (next.Contains('.'))
                {
                    if (next.EndsWith("f") || next.EndsWith("F"))
                    {
                        throw new NotImplementedException();
                    }
                    double floatValue;
                    if (!double.TryParse(next, out floatValue))
                    {
                        throw new ParserException(token, "Invalid number: '" + next + "'");
                    }
                    return new DoubleConstant(token, floatValue);
                }
                else
                {
                    if (next.EndsWith("f") || next.EndsWith("F"))
                    {
                        throw new NotImplementedException();
                    }

                    int intValue;
                    if (!int.TryParse(next, out intValue))
                    {
                        throw new ParserException(token, "Invalid number: '" + next + "'");
                    }

                    return new IntegerConstant(token, intValue);
                }
            }

            if ((c >= 'a' && c <= 'z') ||
                (c >= 'A' && c <= 'Z') ||
                c == '_')
            {
                if (tokens.IsNext("=>"))
                {
                    List<Token> args = new List<Token>() { token };
                    Token arrowToken = tokens.Pop();
                    Executable[] lambdaBody;
                    if (tokens.IsNext("{"))
                    {
                        lambdaBody = ExecutableParser.ParseCodeBlock(context, tokens, true);
                    }
                    else
                    {
                        Expression lambdaBodyExpr = Parse(context, tokens);
                        lambdaBody = new Executable[] {
                            new ReturnStatement(lambdaBodyExpr.FirstToken, lambdaBodyExpr),
                        };
                    }
                    return new Lambda(token, args, arrowToken, lambdaBody);
                }
                return new Variable(token);
            }

            throw new ParserException(token, "Unexpected token: '" + token.Value + "'");
        }
    }
}
