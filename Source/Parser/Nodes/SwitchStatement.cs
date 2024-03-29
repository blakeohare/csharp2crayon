﻿using System.Collections.Generic;

namespace CSharp2Crayon.Parser.Nodes
{
    public class SwitchStatement : Executable
    {
        public Expression Condition { get; private set; }
        public SwitchStatementChunk[] Chunks { get; private set; }

        public SwitchStatement(
            Token firstToken,
            Expression condition,
            IList<Token> caseTokens,
            IList<Expression> caseConstants,
            IList<Executable[]> codeForCases,
            TopLevelEntity parent)
            : base(firstToken, parent)
        {
            this.Condition = condition;

            List<SwitchStatementChunk> chunks = new List<SwitchStatementChunk>();
            List<Token> caseTokensForCurrentChunk = new List<Token>();
            List<Expression> caseConstantsForCurrentChunk = new List<Expression>();

            for (int i = 0; i < codeForCases.Count; ++i)
            {
                caseTokensForCurrentChunk.Add(caseTokens[i]);
                caseConstantsForCurrentChunk.Add(caseConstants[i]);
                if (codeForCases[i] == null || codeForCases[i].Length == 0)
                {
                    // carry it over to the next chunk
                }
                else
                {
                    chunks.Add(new SwitchStatementChunk()
                    {
                        Cases = caseConstantsForCurrentChunk.ToArray(),
                        CaseTokens = caseTokensForCurrentChunk.ToArray(),
                        Code = codeForCases[i],
                    });
                    caseConstantsForCurrentChunk.Clear();
                    caseTokensForCurrentChunk.Clear();
                }
            }

            if (caseConstantsForCurrentChunk.Count > 0)
            {
                throw new ParserException(caseTokens[0], "This case does not have any code and falls through to the bottom of the switch statement.");
            }

            this.Chunks = chunks.ToArray();
        }

        public override IList<Executable> ResolveTypes(ParserContext context, VariableScope varScope)
        {
            this.Condition = this.Condition.ResolveTypes(context, varScope);

            VariableScope switchScope = new VariableScope(varScope);

            foreach (SwitchStatementChunk chunk in this.Chunks)
            {
                for (int i = 0; i < chunk.Cases.Length; ++i)
                {
                    Expression caseExpr = chunk.Cases[i];
                    if (caseExpr != null)
                    {
                        chunk.Cases[i] = caseExpr.ResolveTypes(context, switchScope);
                    }
                }

                chunk.Code = Executable.ResolveTypesForCode(chunk.Code, context, switchScope);
            }
            return Listify(this);
        }
    }

    public class SwitchStatementChunk
    {
        public Expression[] Cases { get; set; }
        public Token[] CaseTokens { get; set; }
        public Executable[] Code { get; set; }

        public SwitchStatementChunk() { }
    }
}
