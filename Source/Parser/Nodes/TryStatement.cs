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
            Executable[] finallyCode)
            : base(tryToken)
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

        public override IList<Executable> ResolveTypes(ParserContext context)
        {
            throw new System.NotImplementedException();
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
