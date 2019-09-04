using System;
namespace CSharp2Crayon.Parser.Nodes
{
    public abstract class Entity
    {
        public Token FirstToken { get; private set; }

        public Entity(Token firstToken)
        {
            this.FirstToken = firstToken;
        }
    }
}
