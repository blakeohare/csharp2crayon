using System;
using System.Collections.Generic;

namespace CSharp2Crayon.Parser.Nodes
{
    public class ClassDefinition : TopLevelEntity
    {
        public Token ClassToken { get; private set; }
        public bool IsStatic { get; private set; }
        public bool IsAbstract { get; private set; }
        public Token Name { get; private set; }
        public Token[][] RawSubClassInfoTokens { get; private set; }

        private List<TopLevelEntity> membersBuilder = new List<TopLevelEntity>();

        public ClassDefinition(
            Token firstToken, 
            Dictionary<string, Token> modifiers, 
            Token classToken, 
            Token classNameToken, 
            List<Token[]> subClassesAndSuch) 
            : base(firstToken)
        {
            this.ClassToken = classToken;
            this.Name = classNameToken;
            this.IsStatic = modifiers.ContainsKey("static");
            this.IsAbstract = modifiers.ContainsKey("abstract");
            this.RawSubClassInfoTokens = subClassesAndSuch.ToArray();
        }

        public void AddMember(TopLevelEntity tle)
        {
            // TODO: check types
            this.membersBuilder.Add(tle);
        }
    }
}
