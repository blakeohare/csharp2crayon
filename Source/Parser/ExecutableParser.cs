using CSharp2Crayon.Parser.Nodes;
using System;
using System.Collections.Generic;

namespace CSharp2Crayon.Parser
{
    public static class ExecutableParser
    {
        public static Executable[] ParseCodeBlock(ParserContext context, TokenStream tokens, bool requireBrackets)
        {
            bool hasBrackets = requireBrackets || tokens.IsNext("{");

            List<Executable> lines = new List<Executable>();
            if (hasBrackets)
            {
                tokens.PopExpected("{");
                while (!tokens.PopIfPresent("}"))
                {
                    Executable line = Parse(context, tokens);
                    lines.Add(line);
                }
            }
            else
            {
                lines.Add(Parse(context, tokens));
            }
            return lines.ToArray();
        }

        public static Executable Parse(ParserContext context, TokenStream tokens)
        {
            return Parse(context, tokens, true);
        }

        public static Executable Parse(ParserContext context, TokenStream tokens, bool enableSemicolon)
        {
            switch (tokens.PeekValue())
            {
                case "for": return ParseForLoop(context, tokens);
                case "foreach": return ParseForEachLoop(context, tokens);
                case "if": return ParseIfStatement(context, tokens);
                case "while": throw new NotImplementedException();
                case "do": throw new NotImplementedException();
                case "switch": throw new NotImplementedException();
                case "throw": return ParseThrowStatement(context, tokens);
                case "return": return ParseReturnStatement(context, tokens);
                case "using": return ParseUsingStatement(context, tokens);
                default:
                    break;
            }

            // check for variable declaration
            int state = tokens.CurrentState;
            CSharpType variableDeclarationType = CSharpType.TryParse(tokens);
            if (variableDeclarationType != null)
            {
                Token variableName = tokens.PopWordIfPresent();
                if (tokens.IsNext(";") || tokens.IsNext("="))
                {
                    // This is a variable declaration.
                    Executable varDecl = ParseVariableDeclaration(context, tokens, variableDeclarationType, variableName);
                    if (enableSemicolon) tokens.PopExpected(";");
                    return varDecl;
                }

                tokens.RestoreState(state);
            }

            Expression expr = ExpressionParser.Parse(context, tokens);

            Executable exec;

            string nextToken = tokens.PeekValue();
            switch (nextToken)
            {
                case "=":
                case "+=":
                case "-=":
                case "*=":
                case "/=":
                case "%=":
                case "|=":
                case "&=":
                case "^=":
                case "<<=":
                case ">>=":
                    Token assignmentOpToken = tokens.Pop();
                    Expression assignmentValue = ExpressionParser.Parse(context, tokens);
                    exec = new AssignmentStatement(expr.FirstToken, expr, assignmentOpToken, assignmentValue);
                    break;

                default:
                    exec = new ExpressionAsExecutable(expr);
                    break;
            }

            if (enableSemicolon)
            {
                tokens.PopExpected(";");
            }
            return exec;
        }

        private static Executable ParseVariableDeclaration(ParserContext context, TokenStream tokens, CSharpType type, Token name)
        {
            Expression targetValue = null;
            Token assignmentToken = null;
            if (tokens.IsNext("="))
            {
                assignmentToken = tokens.Pop();
                targetValue = ExpressionParser.Parse(context, tokens);
            }

            VariableDeclaration varDecl = new VariableDeclaration(type.FirstToken, type, name, assignmentToken, targetValue);
            return varDecl;
        }

        private static Executable ParseIfStatement(ParserContext context, TokenStream tokens)
        {
            Token ifToken = tokens.PopExpected("if");
            tokens.PopExpected("(");
            Expression condition = ExpressionParser.Parse(context, tokens);
            tokens.PopExpected(")");
            Executable[] ifCode = ParseCodeBlock(context, tokens, false);
            Executable[] elseCode = null;
            Token elseToken = null;
            if (tokens.IsNext("else"))
            {
                elseToken = tokens.Pop();
                elseCode = ParseCodeBlock(context, tokens, false);
            }
            return new IfStatement(ifToken, condition, ifCode, elseToken, elseCode);
        }

        private static Executable ParseReturnStatement(ParserContext context, TokenStream tokens)
        {
            Token returnToken = tokens.PopExpected("return");
            if (tokens.PopIfPresent(";"))
            {
                return new ReturnStatement(returnToken, null);
            }

            Expression expression = ExpressionParser.Parse(context, tokens);
            tokens.PopExpected(";");
            return new ReturnStatement(returnToken, expression);
        }

        private static Executable ParseForLoop(ParserContext context, TokenStream tokens)
        {
            Token forToken = tokens.PopExpected("for");
            tokens.PopExpected("(");
            List<Executable> initCode = new List<Executable>();
            while (!tokens.PopIfPresent(";"))
            {
                if (initCode.Count > 0) tokens.PopExpected(",");
                initCode.Add(Parse(context, tokens, false));
            }
            Expression condition = null;
            if (!tokens.IsNext(";"))
            {
                condition = ExpressionParser.Parse(context, tokens);
            }
            tokens.PopExpected(";");
            List<Executable> stepCode = new List<Executable>();

            while (!tokens.PopIfPresent(")"))
            {
                if (stepCode.Count > 0) tokens.PopExpected(",");
                stepCode.Add(Parse(context, tokens, false));
            }

            Executable[] loopBody = ParseCodeBlock(context, tokens, false);
            return new ForLoop(forToken, initCode, condition, stepCode, loopBody);
        }

        private static Executable ParseForEachLoop(ParserContext context, TokenStream tokens)
        {
            Token foreachToken = tokens.PopExpected("foreach");
            tokens.PopExpected("(");
            CSharpType type = CSharpType.Parse(tokens);
            Token variableToken = tokens.PopWord();
            tokens.PopExpected("in");
            Expression listExpression = ExpressionParser.Parse(context, tokens);
            tokens.PopExpected(")");
            Executable[] loopBody = ParseCodeBlock(context, tokens, false);
            return new ForEachLoop(foreachToken, type, variableToken, listExpression, loopBody);
        }

        private static Executable ParseThrowStatement(ParserContext context, TokenStream tokens)
        {
            Token throwToken = tokens.PopExpected("throw");
            Expression expr = ExpressionParser.Parse(context, tokens);
            tokens.PopExpected(";");
            return new ThrowStatement(throwToken, expr);
        }

        private static Executable ParseUsingStatement(ParserContext context, TokenStream tokens)
        {
            Token usingToken = tokens.PopExpected("using");
            tokens.PopExpected("(");
            CSharpType type = CSharpType.Parse(tokens);
            Token variable = tokens.PopWord();
            Token equalsToken = tokens.PopExpected("=");
            Expression expression = ExpressionParser.Parse(context, tokens);
            tokens.PopExpected(")");
            Executable[] code = ExecutableParser.ParseCodeBlock(context, tokens, false);
            return new UsingStatement(usingToken, type, variable, expression, code);
        }
    }
}
