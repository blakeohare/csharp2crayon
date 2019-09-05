using System;
namespace CSharp2Crayon.Parser.Nodes
{
    public abstract class TopLevelEntity : Entity
    {
        public TopLevelEntity(Token firstToken)
            : base(firstToken)
        {
        }
    }
}
