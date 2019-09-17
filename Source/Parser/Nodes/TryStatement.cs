using System.Collections.Generic;

namespace CSharp2Crayon.Parser.Nodes
{
    public class TryStatement : Executable
    {
        public Executable[] TryCode { get; private set; }
        public CatchBlock[] CatchBlocks { get; private set; }
        public Token FinallyToken { get; private set; }
        public Executable[] FinallyCode { get; private set; }

        public TryStatement(
            Token tryToken,
            Executable[] tryCode,
            IList<Token> catchTokens,
            IList<CSharpType> catchBlockTypes,
            IList<Token> catchBlockVariables,
            IList<Executable[]> catchBlockCode,
            Token finallyToken,
            Executable[] finallyCode,
            TopLevelEntity parent)
            : base(tryToken, parent)
        {

            this.TryCode = tryCode;
            List<CatchBlock> catches = new List<CatchBlock>();
            for (int i = 0; i < catchBlockCode.Count; ++i)
            {
                catches.Add(new CatchBlock()
                {
                    CatchToken = catchTokens[i],
                    ExceptionType = catchBlockTypes[i],
                    ExceptionVariable = catchBlockVariables[i],
                    Code = catchBlockCode[i],
                });
            }
            this.CatchBlocks = catches.ToArray();
            this.FinallyToken = finallyToken;
            this.FinallyCode = finallyCode;
        }

        public override IList<Executable> ResolveTypes(ParserContext context, VariableScope varScope)
        {
            this.TryCode = Executable.ResolveTypesForCode(this.TryCode, context, new VariableScope(varScope));
            foreach (CatchBlock cb in this.CatchBlocks)
            {
                ResolvedType exceptionType = this.Parent.DoTypeLookupFailSilently(cb.ExceptionType, context);
                if (exceptionType == null) throw new ParserException(cb.ExceptionType.FirstToken, "Exception type not found.");
                if (!exceptionType.IsException(context)) throw new ParserException(cb.ExceptionType.FirstToken, "This type does not extend from System.Exception");
                VariableScope catchVarScope = new VariableScope(varScope);
                if (cb.ExceptionVariable != null)
                {
                    catchVarScope.DeclareVariable(cb.ExceptionVariable.Value, exceptionType);
                }
                cb.Code = Executable.ResolveTypesForCode(cb.Code, context, catchVarScope);
            }

            if (this.FinallyCode != null)
            {
                this.FinallyCode = Executable.ResolveTypesForCode(this.FinallyCode, context, new VariableScope(varScope));
            }
            return Listify(this);
        }
    }

    public class CatchBlock
    {
        public Token CatchToken { get; set; }
        public CSharpType ExceptionType { get; set; }
        public Token ExceptionVariable { get; set; }
        public Executable[] Code { get; set; }
    }
}
