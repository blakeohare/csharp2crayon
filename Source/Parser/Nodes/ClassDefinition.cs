using System.Collections.Generic;

namespace CSharp2Crayon.Parser.Nodes
{
    public class ClassDefinition : TopLevelEntity
    {
        public Token ClassToken { get; private set; }
        public Token Name { get; private set; }
        public CSharpType[] RawSubClassInfoTokens { get; private set; }

        private List<TopLevelEntity> membersBuilder = new List<TopLevelEntity>();

        public ClassDefinition(
            Token firstToken,
            Dictionary<string, Token> modifiers,
            Token classToken,
            Token classNameToken,
            List<CSharpType> subClassesAndSuch)
            : base(firstToken)
        {
            this.ClassToken = classToken;
            this.Name = classNameToken;
            this.ApplyModifiers(modifiers);
            this.RawSubClassInfoTokens = subClassesAndSuch.ToArray();
        }

        public void AddMember(TopLevelEntity tle)
        {
            // TODO: check types
            this.membersBuilder.Add(tle);
        }
    }
}
