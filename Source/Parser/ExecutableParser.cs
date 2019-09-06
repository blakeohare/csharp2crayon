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
                case "while": return ParseWhileLoop(context, tokens);
                case "do": return ParseDoWhileLoop(context, tokens);
                case "switch": return ParseSwitchStatement(context, tokens);
                case "throw": return ParseThrowStatement(context, tokens);
                case "return": return ParseReturnStatement(context, tokens);
                case "using": return ParseUsingStatement(context, tokens);
                case "try": return ParseTryStatement(context, tokens);
                default:
                    break;
            }

            // check for variable declaration
            int state = tokens.CurrentState;
            CSharpType variableDeclarationType = CSharpType.TryParse(tokens);
            if (variableDeclarationType != null)
            {
                Token variableName = tokens.PopWordIfPresent();
                if (tokens.IsNext(";") || tokens.IsNext("=") || tokens.IsNext(","))
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
            else if (tokens.IsNext(","))
            {
                List<Token> variableNames = new List<Token>() { name };
                while (tokens.PopIfPresent(","))
                {
                    variableNames.Add(tokens.PopWord());
                }
                return new MultiVariableDeclaration(type.FirstToken, type, variableNames);
            }

            return new VariableDeclaration(type.FirstToken, type, name, assignmentToken, targetValue);
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

        private static Executable ParseWhileLoop(ParserContext context, TokenStream tokens)
        {
            Token whileToken =  tokens.PopExpected("while");
            tokens.PopExpected("(");
            Expression condition = ExpressionParser.Parse(context, tokens);
            tokens.PopExpected(")");
            Executable[] code = ParseCodeBlock(context, tokens, false);
            return new WhileLoop(whileToken, condition, code);
        }

        private static Executable ParseDoWhileLoop(ParserContext context, TokenStream tokens)
        {
            Token doToken = tokens.PopExpected("do");
            Executable[] code = ParseCodeBlock(context, tokens, true);
            Token whileToken = tokens.PopExpected("while");
            tokens.PopExpected("(");
            Expression condition = ExpressionParser.Parse(context, tokens);
            tokens.PopExpected(")");
            tokens.PopExpected(";");
            return new DoWhileLoop(doToken, code, condition);
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

        private static Executable ParseSwitchStatement(ParserContext context, TokenStream tokens)
        {
            Token switchToken = tokens.PopExpected("switch");
            tokens.PopExpected("(");
            Expression condition = ExpressionParser.Parse(context, tokens);
            tokens.PopExpected(")");
            tokens.PopExpected("{");
            List<Token> caseTokens = new List<Token>();
            List<Expression> cases = new List<Expression>(); // a null entry indicates default
            List<Executable[]> codeForCase = new List<Executable[]>();
            while (!tokens.PopIfPresent("}"))
            {
                if (tokens.IsNext("case") || tokens.IsNext("default"))
                {
                    if (tokens.IsNext("case"))
                    {
                        caseTokens.Add(tokens.PopExpected("case"));
                        cases.Add(ExpressionParser.Parse(context, tokens));
                        tokens.PopExpected(":");
                    }
                    else
                    {
                        caseTokens.Add(tokens.PopExpected("default"));
                        tokens.PopExpected(":");
                        cases.Add(null);
                    }
                }

                List<Executable> codeForCurrentBlock = new List<Executable>();
                while (!tokens.IsNext("case") && !tokens.IsNext("default") && !tokens.IsNext("}"))
                {
                    // these are the 3 things that can appear in a switch statement. If it's not one of these,
                    // then it's a line of code that belongs to the previous case/default.
                    codeForCurrentBlock.Add(Parse(context, tokens));
                }
                codeForCase.Add(codeForCurrentBlock == null ? null : codeForCurrentBlock.ToArray());
            }

            return new SwitchStatement(switchToken, condition, caseTokens, cases, codeForCase);
        }

        private static Executable ParseThrowStatement(ParserContext context, TokenStream tokens)
        {
            Token throwToken = tokens.PopExpected("throw");
            Expression expr = ExpressionParser.Parse(context, tokens);
            tokens.PopExpected(";");
            return new ThrowStatement(throwToken, expr);
        }

        private static Executable ParseTryStatement(ParserContext context, TokenStream tokens)
        {
            Token tryToken = tokens.PopExpected("try");
            Executable[] tryCode = ExecutableParser.ParseCodeBlock(context, tokens, true);

            List<Token> catchTokens = new List<Token>();
            List<CSharpType> catchBlockTypes = new List<CSharpType>();
            List<Token> catchBlockVariables = new List<Token>();
            List<Executable[]> catchBlockCode = new List<Executable[]>();
            Token finallyToken = null;
            Executable[] finallyCode = null;

            while (tokens.IsNext("catch"))
            {
                catchTokens.Add(tokens.Pop());
                tokens.PopExpected("(");
                catchBlockTypes.Add(CSharpType.Parse(tokens));
                if (!tokens.PopIfPresent(")"))
                {
                    catchBlockVariables.Add(tokens.PopWord());
                    tokens.PopExpected(")");
                }
                else
                {
                    catchBlockVariables.Add(null);
                }
                catchBlockCode.Add(ParseCodeBlock(context, tokens, true));
            }

            if (tokens.IsNext("finally"))
            {
                finallyToken = tokens.Pop();
                finallyCode = ParseCodeBlock(context, tokens, true);
            }

            return new TryStatement(
                tryToken,
                tryCode,
                catchTokens,
                catchBlockTypes,
                catchBlockVariables,
                catchBlockCode,
                finallyToken,
                finallyCode);
        }

        private static Executable ParseUsingStatement(ParserContext context, TokenStream tokens)
        {
            Token usingToken = tokens.PopExpected("using");
            tokens.PopExpected("(");
            CSharpType type = null;
            Token variable = null;
            Token equalsToken = null;
            if (!tokens.IsNext("new"))
            {
                type = CSharpType.Parse(tokens);
                variable = tokens.PopWord();
                equalsToken = tokens.PopExpected("=");
            }
            Expression expression = ExpressionParser.Parse(context, tokens);
            tokens.PopExpected(")");
            Executable[] code = ExecutableParser.ParseCodeBlock(context, tokens, false);
            return new UsingStatement(usingToken, type, variable, expression, code);
        }
    }
}
