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

        private static Expression ParseAtomWithSuffix(ParserContext context, TokenStream tokens)
        {
            Expression root;
            if (tokens.IsNext("("))
            {
                tokens.Pop();
                root = Parse(context, tokens);
                tokens.PopExpected(")");
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
                    case "(":
                        Token openParen = tokens.Pop();
                        List<Expression> args = new List<Expression>();
                        if (!tokens.PopIfPresent(")"))
                        {
                            args.Add(Parse(context, tokens));
                            while (tokens.PopIfPresent(","))
                            {
                                args.Add(Parse(context, tokens));
                            }
                            tokens.PopExpected(")");
                        }
                        root = new FunctionInvocation(root.FirstToken, root, openParen, args);
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
                    if (!tokens.IsNext("(")) tokens.PopExpected("(");
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
                throw new NotImplementedException(); // Parse hex literal
            }

            if (c == '.' && next.Length > 1)
            {
                throw new NotImplementedException(); // parse float
            }

            if (c >= '0' && c <= '9')
            {
                if (next.Contains('.'))
                {
                    throw new NotImplementedException(); // parse float
                }
                else
                {
                    throw new NotImplementedException(); // parse decimal integer
                }
            }

            if ((c >= 'a' && c <= 'z') ||
                (c >= 'A' && c <= 'Z') ||
                c == '_')
            {
                return new Variable(token);
            }

            throw new ParserException(token, "Unexpected token: '" + token.Value + "'");
        }
    }
}
