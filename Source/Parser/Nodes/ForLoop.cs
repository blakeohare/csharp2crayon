using System.Collections.Generic;
using System.Linq;

namespace CSharp2Crayon.Parser.Nodes
{
    public class ForLoop : Executable
    {
        public Executable[] InitCode { get; private set; }
        public Expression Condition { get; private set; }
        public Executable[] StepCode { get; private set; }
        public Executable[] Code { get; private set; }

        public ForLoop(Token forToken, IList<Executable> initCode, Expression condition, IList<Executable> stepCode, IList<Executable> loopBody)
            : base(forToken)
        {
            this.InitCode = initCode.ToArray();
            this.Condition = condition;
            this.StepCode = stepCode.ToArray();
            this.Code = loopBody.ToArray();
        }
    }
}
