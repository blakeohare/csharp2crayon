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
                case "for": throw new NotImplementedException();
                case "if": throw new NotImplementedException();
                case "while": throw new NotImplementedException();
                case "do": throw new NotImplementedException();
                case "switch": throw new NotImplementedException();
                case "throw": throw new NotImplementedException();
                case "return": throw new NotImplementedException();
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

            throw new NotImplementedException();
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
    }
}
